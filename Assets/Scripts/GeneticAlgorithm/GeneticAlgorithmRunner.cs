using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneticAlgorithmRunner : MonoBehaviour
{
    public PlayerController player;
    public Vector3 startPosition;
    public int simulationTime;

    private Problem problem;
    private Parameters parameters;
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;
        simulationTime = 90;

        int numberOfGenes = 60; // Size of overall array for movement (must be divisible by 3)
        if (numberOfGenes % 3 != 0)
        {
            Debug.LogError("numberOfGenes must be divisible by 3. will round down.");
            numberOfGenes = (numberOfGenes / 3) * 3; // Round down to the nearest multiple of 3
        }

        problem = new Problem(
            numberOfGenes: numberOfGenes,
            minValue: 0,
            maxValue: 10,
            costFunction: FitnessFunction,
            maxAcceptedCost: 1000f, // Best would be finish level with fast time
            player: player
        );

        parameters = new Parameters(
            populationSize: 50,
            birthRatePerGeneration: 1,
            exploreCrossoverRange: 0.2f,
            geneMutationRate: 0.2f,
            geneMutationRange: 0.5f,
            maxNumberOfGenerations: 10
        );

        // Initialize bestSolution with a default value
        bestSolution = new Individual(problem) { Cost = float.MinValue };

        // Start genetic algorithm
        StartCoroutine(RunGeneticAlgorithm());
    }

    private IEnumerator RunGeneticAlgorithm()
    {
        Debug.Log("Starting Genetic Algorithm:");

        List<Individual> population = new List<Individual>();

        // Initialise population
        for (int i = 0; i < parameters.PopulationSize; i++)
        {
            Individual newIndividual = new Individual(problem);
            population.Add(newIndividual);
            Debug.Log($"[Generation 0] Created individual {i + 1} with chromosome: {string.Join(", ", newIndividual.Chromosome)}");
        }

        // Run generations
        for (int generation = 0; generation < parameters.MaxNumberOfGenerations; generation++)
        {
            Debug.Log($"===== Starting Generation {generation + 1}/{parameters.MaxNumberOfGenerations} =====");
            Debug.Log($"Current Population Size: {population.Count}");

            // Fitness for each individual
            for (int i = 0; i < population.Count; i++)
            {
                Debug.Log($"[Generation {generation + 1}] Evaluating Individual {i + 1}/{population.Count}");
                yield return StartCoroutine(SimulateIndividual(population[i]));
                population[i].Cost = problem.CostFunction(population[i]);
            }

            // Population by cost (higher cost = better)
            population = population.OrderByDescending(ind => ind.Cost).ToList();

            // Check if the current best solution is better than the stored best solution
            if (population[0].Cost > bestSolution.Cost)
            {
                StoreBestSolution(population[0]);
                Debug.Log($"[Generation {generation + 1}] New best solution found with fitness {bestSolution.Cost}");
            }

            // Create next generation
            List<Individual> nextGeneration = new List<Individual>();

            // Add elites - top 10%
            int eliteCount = Mathf.FloorToInt(parameters.PopulationSize * 0.1f);
            nextGeneration.AddRange(population.Take(eliteCount));

            // Add children
            List<Individual> children = new List<Individual>();
            while (children.Count < parameters.PopulationSize * parameters.BirthRatePerGeneration)
            {
                Individual parent1 = SelectParent(population);
                Individual parent2 = SelectParent(population);

                // Ensure parents are different
                while (parent1 == parent2)
                {
                    parent2 = SelectParent(population);
                }

                (Individual child1, Individual child2) = parent1.Crossover(parent2, parameters.ExploreCrossoverRange);
                child1.Mutate(parameters.GeneMutationRate, parameters.GeneMutationRange);
                child2.Mutate(parameters.GeneMutationRate, parameters.GeneMutationRange);

                children.Add(child1);
                children.Add(child2);
            }
            nextGeneration.AddRange(children);

            while (nextGeneration.Count < parameters.PopulationSize)
            {
                // Fill remaining with tournament style selection
                nextGeneration.Add(SelectParent(population));
            }

            population = nextGeneration.OrderByDescending(ind => ind.Cost).Take(parameters.PopulationSize).ToList();

            Debug.Log($"===== End of Generation {generation + 1} | Best Cost: {bestSolution.Cost} =====");
        }

        Debug.Log("Genetic Algorithm Complete!");
        StoreBestSolution(bestSolution);
    }

    internal Individual bestSolution;

    private void StoreBestSolution(Individual solution)
    {
        if (solution.Cost > bestSolution.Cost)
        {
            bestSolution = new Individual(solution);
            Debug.Log($"Best solution stored with fitness: {bestSolution.Cost}");
        }
    }

    private IEnumerator SimulateIndividual(Individual individual)
    {
        gm.wasDeadThisRun = false;  // Reset death before starting simulation
        gm.ResetGameState();
        player.ResetPlayer();

        // Assign chromosome
        int geneCount = individual.Chromosome.Length;
        int splitPoint = geneCount / 3;
        player.InputChromosome = new Chromosome
        {
            LeftTime = individual.Chromosome.Take(splitPoint).ToList(),
            RightTime = individual.Chromosome.Skip(splitPoint).Take(splitPoint).ToList(),
            JumpTime = individual.Chromosome.Skip(2 * splitPoint).Take(splitPoint).ToList()
        };

        // Simulate player behavior
        float bestProgress = player.transform.position.x; // Track the best progress
        float startTime = Time.time;
        while (Time.time - startTime < simulationTime)
        {
            yield return new WaitForFixedUpdate(); // Attempt to ensure accurate physics

            if (player.isDead)
            {
                gm.wasDeadThisRun = true;
                Debug.Log($"Simulation Stopped. Death at {Time.time - startTime} seconds.");
                break;
            }

            if (gm.reachedGoal)
            {
                Debug.Log($"Simulation Stopped. Goal Reached at {Time.time - startTime} seconds.");
                break;
            }

            // Update best progress
            if (player.transform.position.x > bestProgress)
            {
                bestProgress = player.transform.position.x;
            }
        }

        individual.BestProgress = bestProgress;

        Debug.Log($"Simulation complete. Chromosome: {string.Join(", ", individual.Chromosome)}");
    }

    // Tournament style selection of parent
    private Individual SelectParent(List<Individual> population)
    {
        const int TOURNAMENT_SIZE = 3;
        Individual best = null;
        HashSet<int> selectedIndices = new HashSet<int>();

        for (int i = 0; i < TOURNAMENT_SIZE; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = UnityEngine.Random.Range(0, population.Count);
            } while (selectedIndices.Contains(randomIndex));

            selectedIndices.Add(randomIndex);
            Individual contender = population[randomIndex];

            if (best == null || contender.Cost > best.Cost)
            {
                best = contender;
            }
        }

        return best;
    }

    private float FitnessFunction(Individual individual)
    {
        float progressReward = individual.BestProgress; // Use the individual's best progress

        // Goal bonus: Big reward for reaching the goal
        float goalBonus = gm.reachedGoal ? 10000f : 0f;

        // Death penalty: Ideally, dying earlier is worse than dying later
        float deathPenalty = gm.wasDeadThisRun ? -50f * (1 - (gm.gameTimer / simulationTime)) : 0f;

        // Combined fitness
        float fitness = progressReward + goalBonus + deathPenalty;

        Debug.Log($"Fitness: Progress={progressReward}, Goal={goalBonus}, Death={deathPenalty}, Total={fitness}");
        return fitness;
    }
}