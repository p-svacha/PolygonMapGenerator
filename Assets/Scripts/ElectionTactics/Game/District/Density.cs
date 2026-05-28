using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class DensityDef : Def
    {
        public Color Color { get; init; }
        public int SortingOrder { get; init; } // Used in policy list and map control legend, highest on top
    }

    public static class DensityDefs
    {
        public static List<DensityDef> Defs => new List<DensityDef>()
        {
            new DensityDef()
            {
                DefName = "Urban",
                Label = "Urban",
                SortingOrder = 2,
                Color = new Color(0.65f, 0.65f, 0.70f),
            },

            new DensityDef()
            {
                DefName = "Suburban",
                Label = "Suburban",
                SortingOrder = 1,
                Color = new Color(0.85f, 0.75f, 0.50f),
            },

            new DensityDef()
            {
                DefName = "Rural",
                Label = "Rural",
                SortingOrder = 0,
                Color = new Color(0.45f, 0.70f, 0.40f),
            },
        };
    }

    [DefOf]
    public static class DensityDefOf
    {
        public static DensityDef Urban;
        public static DensityDef Suburban;
        public static DensityDef Rural;
    }
}
