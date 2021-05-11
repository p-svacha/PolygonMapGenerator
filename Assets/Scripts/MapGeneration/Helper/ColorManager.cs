using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColorManager
{
    private static Dictionary<Color, string> DistinctColors = new Dictionary<Color, string>()
    {
        { new Color(0.5f, 0f, 0f), "Maroon" },
        { new Color(0.66f, 0.43f, 0.16f), "Brown" },
        { new Color(0.5f, 0.5f, 0f), "Olive" },
        { new Color(0f, 0.5f, 0.5f), "Teal" },
        { new Color(0f, 0f, 0.5f), "Navy" },
        { new Color(0.1f, 0.1f, 0.1f), "Black" },
        { new Color(0.5f, 0.5f, 0.5f), "Grey" },
        { new Color(0.9f, 0.9f, 0.9f), "White" },
        { new Color(0.9f, 0.1f, 0.29f), "Red" },
        { new Color(0.96f, 0.51f, 0.19f), "Orange" },
        { new Color(1f, 0.88f, 0.1f), "Yellow" },
        { new Color(0.82f, 0.96f, 0.24f), "Lime" },
        { new Color(0.24f, 0.71f, 0.29f), "Green" },
        { new Color(0.27f, 0.94f, 0.94f), "Cyan" },
        { new Color(0f, 0.51f, 0.78f), "Blue" },
        { new Color(0.57f, 0.12f, 0.71f), "Purple" },
        { new Color(0.94f, 0.2f, 0.9f), "Magenta" },
        //{ new Color(0.98f, 0.75f, 0.83f), "Pink" },
        //{ new Color(1f, 0.84f, 0.71f), "Apricot" },
        //{ new Color(1f, 0.98f, 0.78f), "Beige" },
        //{ new Color(0.66f, 1f, 0.76f), "Mint" },
        //{ new Color(0.86f, 0.75f, 1f), "Lavender" },

    };

    /// <summary>
    /// Returns a color that is not similar to a color in otherColors
    /// </summary>
    public static Color GetRandomColor(List<Color> otherColors = null)
    {
        float diff = 0;
        Color color = Color.white;

        int counter = 0;

        while(diff < 1f && counter < 50)
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

    public static Color GetRandomDistinctColor(List<Color> otherColors, bool noGreyScale = false)
    {
        List<Color> candidates = DistinctColors.Keys.Where(x => !otherColors.Contains(x)).ToList();
        if (noGreyScale) {
            candidates.Remove(GetColorByName("White"));
            candidates.Remove(GetColorByName("Grey"));
            candidates.Remove(GetColorByName("Black"));
        }
        if (candidates.Count == 0) return Color.red;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    public static string GetColorName(Color color)
    {
        return DistinctColors[color];
    }

    public static Color GetColorByName(string color)
    {
        return DistinctColors.First(x => x.Value == color).Key;
    }
}
