using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCalculator : MonoBehaviour
{
    private static readonly List<float> StarThresholds = new()
    {
        0.5f,
        0.7f,
        0.9f
    };

    public static int GetNumberOfStars(float score)
    {
        var count = 0;
        foreach (var threshold in StarThresholds)
        {
            if (score >= threshold)
                count++;
        }
        return count;
    }
}
