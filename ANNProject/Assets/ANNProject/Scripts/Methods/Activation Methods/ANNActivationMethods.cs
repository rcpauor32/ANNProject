using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ANNActivationMethodsList
{
    Linear = 0,
    Step,
    Sigmoid,
    Tanh,
    ReLu,

    // ------------
    Custom
}

public static class ANNActivationMethods {

    /// USER METHODS ///
    
    /// !_USER METHODS

    // Activation Methods --
    
    // ¡¡ Must be => 'float [fooname](float value)' !!

    // Linear Activation -
    public static float LinearActivation(float value)
    {
        return ANNProperties.Activation_Linear_Gradient * value; // A = c * x
    }
    // Step Activation -
    public static float StepActivation(float value)
    {
        return (value > ANNProperties.Activation_Step_Threshold ? 1 : 0); // x > th = 1 else = 0
    }
    // Sigmoid Activation -
    public static float SigmoidActivation(float value)
    {
        return ANNMathHelpers.Sigmoid(value); // A = sigmoid(x)
    }
    // Tanh Activation -
    public static float TanhActivation(float value)
    {
        return ANNMathHelpers.Tanh(value); // A = Tanh(x)
    }
    // ReLu Activation -
    public static float ReLuActivation(float value)
    {
        return Mathf.Max(0, value); // A = max(0, x)
    }
}
