using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANNErrorCodes
{
    private static float     p_OUTPUT_ERROR_VALUE = float.MinValue;

    // Getters //
    #region // Getters //
    public static float      OUTPUT_ERROR_VALUE
    {
        get
        {
            return p_OUTPUT_ERROR_VALUE;
        }
    }
    #endregion
}

public class ANNProperties {
    // Properties ---
    public static Vector2   RandomWeightsRange = new Vector2(0, 5);
    public static float     NonRandomWeightDefaultValue = 0f;
    public static bool      SetupWithRandomWeights = true;
    public static int       MaxHiddenLayers = 1;
    public static Vector2   BiasRandomRange = new Vector2(-20, 20);
    public static Color     ConnectionsColor = Color.red;
    public static float     GraphicWeightMultiplier = 5f;
    public static int       GenerationsBeforeDeletingTP = 5;

    // Activation Computation Values ---
    public static float     Activation_Linear_Gradient = 1f;
    public static float     Activation_Step_Threshold = 1f;
}

public enum ANNLayerOrders
{
    Input = -1,
    Output = -2,
    HiddenBegin = 0,

    Unknown = int.MinValue
}