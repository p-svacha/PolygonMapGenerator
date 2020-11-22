using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphDataPoint
{
    public string Label;
    public float Value;
    public Color Color;

    public GraphDataPoint(string label, float value, Color color)
    {
        Label = label;
        Value = value;
        Color = color;
    }
}
