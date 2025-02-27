using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

public class GeneticAlgorithm
{
    public (List<Individual>, Individual) RunGenetic(Problem problem, Parameters parameters)
    {
        int numberInPopulation = parameters.PopulationSize;
        float rateOfGeneMutation = parameters.GeneMutationRate;
        float rangeOfGeneMutation = parameters.GeneMutationRange;
        float exploreCrossover = parameters.ExploreCrossoverRange;
        int numberOfGenerationsMax = parameters.MaxNumberOfGenerations;
        Func<float[], float> costFunction = problem.CostFunction;
        float acceptableCost = problem.MaxAcceptedCost;
        int numberOfChildrenPerGeneration = (int)(numberInPopulation * parameters.BirthRatePerGeneration);

        List<Individual> population = new List<Individual>();
        Individual bestSolution = new Individual(problem);
        bestSolution = new Individual(bestSolution) { Cost = float.MaxValue };

        for (int i = 0; i < numberInPopulation; i++)
        {
            Individual newIndividual = new Individual(problem);
            if (newIndividual.Cost > bestSolution.Cost)
            {
                bestSolution = new Individual(newIndividual);
            }
            population.Add(newIndividual);
        }

        for (int generation = 0; generation < numberOfGenerationsMax; generation++)
        {
            List<Individual> children = new List<Individual>();

            while (children.Count < numberOfChildrenPerGeneration)
            {
                int parent1Index = UnityEngine.Random.Range(0, numberInPopulation);
                int parent2Index = UnityEngine.Random.Range(0, numberInPopulation);

                if (parent1Index == parent2Index)
                {
                    continue;
                }

                Individual parent1 = population[parent1Index];
                Individual parent2 = population[parent2Index];

                (Individual child1, Individual child2) = parent1.Crossover(parent2, exploreCrossover);
                child1.Mutate(rateOfGeneMutation, rangeOfGeneMutation);
                child2.Mutate(rateOfGeneMutation, rangeOfGeneMutation);

                child1.Cost = costFunction(child1.Chromosome);
                child2.Cost = costFunction(child2.Chromosome);

                children.Add(child1);
                children.Add(child2);
            }

            population.AddRange(children);

            population = population.OrderByDescending(ind => ind.Cost).Take(numberInPopulation).ToList();

            if (population[0].Cost < bestSolution.Cost)
            {
                bestSolution = new Individual(population[0]);
            }

            Debug.WriteLine($"Generation {generation}: Best Cost = {bestSolution.Cost}");
        }

        return (population, bestSolution);
    }
}