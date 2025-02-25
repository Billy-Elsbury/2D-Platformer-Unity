using UnityEngine;

public class GeneticAlgorithmRunner : MonoBehaviour
{
    void Start()
    {
        Problem problem = new Problem(
            numberOfGenes: 8,
            minValue: -10,
            maxValue: 10,
            costFunction: SphereFunction,
            maxAcceptedCost: 0.001f
        );

        Parameters parameters = new Parameters(
            populationSize: 1000,
            birthRatePerGeneration: 1,
            exploreCrossoverRange: 0.2f,
            geneMutationRate: 0.2f,
            geneMutationRange: 0.5f,
            maxNumberOfGenerations: 1000
        );

        GeneticAlgorithm ga = new GeneticAlgorithm();
        var (population, bestSolution) = ga.RunGenetic(problem, parameters);

        Debug.Log("Best Solution Chromosome: " + string.Join(", ", bestSolution.Chromosome));
        Debug.Log("Best Solution Cost: " + bestSolution.Cost);
    }

    private float SphereFunction(float[] x)
    {
        float total = 0;
        foreach (float value in x)
        {
            total += value * value;
        }
        return total;
    }
}