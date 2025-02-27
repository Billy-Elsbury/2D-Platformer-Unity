using System;
using System.Collections.Generic;
using UnityEngine;

public class Problem
{
    public int NumberOfGenes { get; private set; }
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }
    public Func<Individual, float> CostFunction { get; private set; }
    public float MaxAcceptedCost { get; private set; }
    public PlayerController Player { get; private set; }

    public Problem(int numberOfGenes, float minValue, float maxValue, Func<Individual, float> costFunction, float maxAcceptedCost, PlayerController player)
    {
        NumberOfGenes = numberOfGenes;
        MinValue = minValue;
        MaxValue = maxValue;
        CostFunction = costFunction;
        MaxAcceptedCost = maxAcceptedCost;
        Player = player;
    }

}
