using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ANNGeneticAlgorithmsList
{
    SliceCrossover = 0,
    RandomCrossover,

    // ---------
    Custom
}

public static class ANNGeneticAlgorithms {

    /// VARIABLES ///

    private static float PercentageToSwap = 0.5f;
   // private static float MutationRate = 0.2f;
    #region // Getters & Setters ---
    public static float percentageToSwap
    {
        get
        {
            return PercentageToSwap;
        }
    }
    public static void SetPercentageToSwap(float percentage)
    {
        if(percentage > 1 || percentage < 0.01)
        {
            Debug.LogWarning("Trying to SetPercentageToSwap(percentage) with values out of the 0.01~1 range");
            return;
        }

        PercentageToSwap = percentage;
    }
    //public static float mutationRate
    //{
    //    get
    //    {
    //        return MutationRate;
    //    }
    //}
    //public static void SetMutationRate(float rate)
    //{
    //    if (rate > 1 || rate < 0)
    //    {
    //        Debug.LogWarning("Trying to SetMutationRate(rate) with values out of the 0~1 range");
    //        return;
    //    }
    //
    //    MutationRate = rate;
    //}
    #endregion

    // Mutation Delegate ---
    public delegate float MutationFunction(float value);
    public static MutationFunction MutationDelegate = DefaultMutationMethod;

    // ¡¡ Must be 'ANNThoughtProcess [fooname](ANNThoughtProcess , ANNThoughtProcess )' !!

    public delegate ANNThoughtProcess GeneticAlgorithm(ANNThoughtProcess TPa, ANNThoughtProcess TPb, float mutationRate);

    /// METHODS ///
    public static ANNThoughtProcess SliceCrossover(ANNThoughtProcess TPa, ANNThoughtProcess TPb, float mutationRate)
    {
        ANNThoughtProcess newTP = new ANNThoughtProcess();
        newTP = TPa;

        int nNeurons = TPa.thoughts.Count;
        int numberToCut = (int)(nNeurons * PercentageToSwap);

        int cutPosition = Random.Range(0, nNeurons);

        for(int i = 0; i < numberToCut; ++i)
        {
            newTP.thoughts[cutPosition] = TPb.thoughts[cutPosition];
            cutPosition++;
            if(cutPosition >= nNeurons)
            {
                cutPosition = 0;
            }
        }

        // Mutations ---
        MutateTP(newTP, mutationRate);

        return newTP; 
    }

    public static ANNThoughtProcess RandomCrossover(ANNThoughtProcess TPa, ANNThoughtProcess TPb, float mutationRate)
    {
        ANNThoughtProcess newTP = new ANNThoughtProcess();

        int nNeurons = TPa.thoughts.Count;

        for(int i = 0; i < nNeurons; ++i)
        {
            newTP.thoughts.Add(Random.Range(0f, 1f) > 0.5f ? TPa.thoughts[i] : TPb.thoughts[i]);
        }

        // Mutations ---
        MutateTP(newTP, mutationRate);

        return newTP;
    }

    /// MUTATIONS ///
    // Methods ---
    public static float DefaultMutationMethod(float value)
    {
        return value + value * (Random.Range(0f, 1f) - 0.5f) + 3 * (Random.Range(0f, 1f) - 0.5f);
    } 

    // Utils ---
    public static void MutateTP(ANNThoughtProcess tp, float rate)
    {
        for(int ti = 0; ti < tp.thoughts.Count; ++ti)
        {
            MutateBias(tp.thoughts[ti], rate);
            MutateWeights(tp.thoughts[ti], rate);
        }
    }
    
    public static void MutateBias(ANNNodeThought thought, float rate)
    {
        thought.bias = MutateFloat(thought.bias, rate);
    }
    public static void MutateWeights(ANNNodeThought thought, float rate)
    {
        for(int i = 0; i < thought.weights.Count; ++i)
        {
            thought.weights[i] = MutateFloat(thought.weights[i], rate);
        }
    }
    private static float MutateFloat(float value, float rate)
    {
        if(Random.Range(0f, 1f) <= rate)
        {
            return MutationDelegate(value); 
        }
        return value;
    }

}
