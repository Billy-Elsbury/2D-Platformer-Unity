using System;
using System.Collections.Generic;

public class Problem
{
    public int NumberOfGenes { get; private set; }
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }
    public Func<float[], float> CostFunction { get; private set; }
    public float MaxAcceptedCost { get; private set; }

    public Problem(int numberOfGenes, float minValue, float maxValue, Func<float[], float> costFunction, float maxAcceptedCost)
    {
        NumberOfGenes = numberOfGenes;
        MinValue = minValue;
        MaxValue = maxValue;
        CostFunction = costFunction;
        MaxAcceptedCost = maxAcceptedCost;
    }
}
