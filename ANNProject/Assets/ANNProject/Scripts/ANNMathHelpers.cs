using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANNMathHelpers
{

    static public float Sigmoid(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }

    static public float Tanh(float x)
    {
        return 2 / (1 + Mathf.Exp(-2 * x));
    }

    static public Vector3 LinearBezierTangentPoint(float t, Vector3 p0, Vector3 p1)
    {
        return p0 + t * (p1 - p0);
    }

    public static int IntParse(string str)
    {
        int ret = 0;
        for (int i = 0; i < str.Length; ++i)
        {
            char letter = str[i];
            ret = 10 * ret + (letter - 48);
        }
        return ret;
    }

    public static string GenerateUID(string auxIdentificator = "")
    {
        return auxIdentificator + System.DateTime.UtcNow.ToString() + Random.Range(int.MinValue, int.MaxValue);
    }
}