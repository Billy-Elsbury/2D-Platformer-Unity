using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Individual
{
    public float[] Chromosome { get; set; }
    public float Cost { get; set; }

    public Individual(Problem problem)
    {
        Chromosome = new float[problem.NumberOfGenes];
        for (int i = 0; i < problem.NumberOfGenes; i++)
        {
            Chromosome[i] = UnityEngine.Random.Range(problem.MinValue, problem.MaxValue);
        }
        Cost = problem.CostFunction(Chromosome);
    }

    public void Mutate(float rateOfGeneMutation, float rangeOfGeneMutation)
    {
        for (int i = 0; i < Chromosome.Length; i++)
        {
            if (UnityEngine.Random.value < rateOfGeneMutation)
            {
                Chromosome[i] += (float)(UnityEngine.Random.Range(-1f, 1f) * rangeOfGeneMutation);
            }
        }
    }

    public (Individual, Individual) Crossover(Individual parent2, float exploreCrossover)
    {
        float alpha = UnityEngine.Random.Range(-exploreCrossover, 1 + exploreCrossover);
        Individual child1 = new Individual(this);
        Individual child2 = new Individual(parent2);

        for (int i = 0; i < Chromosome.Length; i++)
        {
            child1.Chromosome[i] = alpha * Chromosome[i] + (1 - alpha) * parent2.Chromosome[i];
            child2.Chromosome[i] = alpha * parent2.Chromosome[i] + (1 - alpha) * Chromosome[i];
        }

        return (child1, child2);
    }

    public Individual(Individual other)
    {
        Chromosome = other.Chromosome.ToArray();
        Cost = other.Cost;
    }
}