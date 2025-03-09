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
        simulationTime = 40;

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
            maxAcceptedCost: 100000f, // Best would be finish level with fast time
            player: player
        );

        parameters = new Parameters(
            populationSize: 100,
            birthRatePerGeneration: 1,
            exploreCrossoverRange: 0.2f,
            geneMutationRate: 0.1f,
            geneMutationRange: 0.3f,
            maxNumberOfGenerations: 20
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
        gm.wasDeadThisRun = false;
        gm.ResetGameState();
        player.ResetPlayer();

        // Assign chromosome with split
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

        // Start simulation timer
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
        const float GOAL_REWARD_BASE = 100000f;  // Base reward for reaching goal
        const float TIME_PENALTY_SCALE = 500f;   // Scaling factor for time-based penalty
        const float MILESTONE_X_POSITION = 47f;  // X position milestone for bonus
        const float MILESTONE_REWARD = 5000f;    // Reward for passing milestone
        const float DEATH_PENALTY_BASE = -50f;   // Base penalty for dying
        const float DEATH_PENALTY_SCALE = 1f;    // Scale factor for death penalty

        float progressReward = individual.BestProgress;

        // Goal bonus: Reward reaching goal and favour faster times
        float goalBonus = gm.reachedGoal ? (GOAL_REWARD_BASE - gm.gameTimer * TIME_PENALTY_SCALE) : 0f;

        // Ensure reaching goal always results in a positive bonus
        goalBonus = Mathf.Max(goalBonus, 0);

        // Milestone bonus: Reward for passing key progress point
        float milestoneBonus = individual.BestProgress > MILESTONE_X_POSITION ? MILESTONE_REWARD : 0f;

        // Death penalty: avoid early deaths (scaled based on simulation time)
        float deathPenalty = gm.wasDeadThisRun ? DEATH_PENALTY_BASE * (DEATH_PENALTY_SCALE - (gm.gameTimer / simulationTime)) : 0f;

        // Final fitness score
        float fitness = progressReward + goalBonus + milestoneBonus + deathPenalty;

        Debug.Log($"Fitness: Progress={progressReward}, Goal={goalBonus}, Milestone={milestoneBonus}, Death={deathPenalty}, Total={fitness}");
        return fitness;
    }

}