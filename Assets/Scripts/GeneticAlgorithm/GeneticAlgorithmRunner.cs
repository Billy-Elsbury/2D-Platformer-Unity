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
        simulationTime = 30;

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
            geneMutationRate: 0.1f,
            geneMutationRange: 0.3f,
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

        // Assign chromosome with a new split (60% Right, 30% Jump, 10% Left)
        int geneCount = individual.Chromosome.Length;
        int rightSplit = Mathf.FloorToInt(geneCount * 0.6f);
        int jumpSplit = Mathf.FloorToInt(geneCount * 0.3f);
        int leftSplit = geneCount - (rightSplit + jumpSplit); // Remaining genes for Left

        // Round values to two decimal places for consistency
        List<float> roundedChromosome = individual.Chromosome
            .Select(gene => Mathf.Round(gene * 100f) / 100f)
            .ToList();

        player.InputChromosome = new Chromosome
        {
            RightTime = roundedChromosome.Take(rightSplit).ToList(),
            JumpTime = roundedChromosome.Skip(rightSplit).Take(jumpSplit).ToList(),
            LeftTime = roundedChromosome.Skip(rightSplit + jumpSplit).ToList()
        };

        // Wait until the player has stopped moving significantly before starting the timer
        yield return new WaitUntil(() => player.GetComponent<Rigidbody2D>().velocity.magnitude < 0.01f);

        // Now that the player is ready, start the simulation timer
        float bestProgress = player.transform.position.x;
        float startTime = Time.time;

        while (Time.time - startTime < simulationTime)
        {
            yield return new WaitForFixedUpdate();

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

        Debug.Log($"Simulation complete. Chromosome: {string.Join(", ", roundedChromosome)}");
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
        float progressReward = individual.BestProgress; // Reward for progress

        // Goal bonus: Reward reaching the goal and favour faster times
        float goalBonus = gm.reachedGoal ? (10000f - gm.gameTimer * 500f) : 0f;

        // Ensure that reaching the goal is always positive
        if (goalBonus < 0) goalBonus = 0;

        // Reward making it past 47X
        float milestoneBonus = individual.BestProgress > 47f ? 5000f : 0f;

        // Death penalty: Avoid early deaths
        float deathPenalty = gm.wasDeadThisRun ? -50f * (1 - (gm.gameTimer / simulationTime)) : 0f;

        // Final fitness score
        float fitness = progressReward + goalBonus + milestoneBonus + deathPenalty;

        Debug.Log($"Fitness: Progress={progressReward}, Goal={goalBonus}, Milestone={milestoneBonus}, Death={deathPenalty}, Total={fitness}");
        return fitness;
    }
}