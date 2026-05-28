using UnityEngine;

namespace ElectionTactics
{
    public class LegendEntryData
    {
        public string Id;
        public Color Color;
        public string Label;
        public int SortingOrder;

        public LegendEntryData(string id, Color color, string label, int sortingOrder = 0)
        {
            Id = id;
            Color = color;
            Label = label;
            SortingOrder = sortingOrder;
        }
    }
}
