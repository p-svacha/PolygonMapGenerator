using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColorManager
{

    /// <summary>
    /// Returns a color that is not similar to a color in otherColors
    /// </summary>
    public static Color GetRandomColor(List<Color> otherColors = null)
    {
        float diff = 0;
        Color color = Color.white;

        int counter = 0;

        while(diff < 0.5f && counter < 50)
        {
            counter++;

            color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

            if (otherColors == null) diff = 99;
            else
            {
                diff = float.MaxValue;
                foreach (Color c in otherColors)
                {
                    float cDiff = Math.Abs(color.r - c.r) + Math.Abs(color.g - c.g) + Math.Abs(color.b - c.b);
                    if (cDiff < diff) diff = cDiff;
                }
            }
        }

        return color;
    }
}
