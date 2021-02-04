using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphDataPoint
{
    public string Label;
    public float Value;
    public Color Color;

    // Icons that get displayed below the label
    public List<Sprite> Icons; 
    public List<string> IconTooltipTitles;
    public List<string> IconTooltipTexts;

    public GraphDataPoint(string label, float value, Color color, List<Sprite> icons = null, List<string> iconTooltipTitles = null, List<string> iconTooltipTexts = null)
    {
        Label = label;
        Value = value;
        Color = color;
        Icons = icons == null ? new List<Sprite>() : icons;
        IconTooltipTitles = iconTooltipTitles == null ? new List<string>() : iconTooltipTitles;
        IconTooltipTexts = iconTooltipTexts == null ? new List<string>() : iconTooltipTexts;
    }
}
