public class Parameters
{
    public int PopulationSize { get; private set; }
    public float BirthRatePerGeneration { get; private set; }
    public float ExploreCrossoverRange { get; private set; }
    public float GeneMutationRate { get; private set; }
    public float GeneMutationRange { get; private set; }
    public int MaxNumberOfGenerations { get; private set; }

    public Parameters(int populationSize, float birthRatePerGeneration, float exploreCrossoverRange, float geneMutationRate, float geneMutationRange, int maxNumberOfGenerations)
    {
        PopulationSize = populationSize;
        BirthRatePerGeneration = birthRatePerGeneration;
        ExploreCrossoverRange = exploreCrossoverRange;
        GeneMutationRate = geneMutationRate;
        GeneMutationRange = geneMutationRange;
        MaxNumberOfGenerations = maxNumberOfGenerations;
    }
}