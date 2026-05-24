using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ElectionTactics
{
    public class AgeGroupDef : Def
    {
        public Color Color { get; init; }
        public int SortingOrder { get; init; } // Used in policy list, highest on top
    }

    public static class AgeGroupDefs
    {
        public static List<AgeGroupDef> Defs => new List<AgeGroupDef>()
        {
            new AgeGroupDef()
            {
                DefName = "Boomers",
                Label = "Baby Boomers",
                SortingOrder = 3,
                Color = new Color(0.30f, 0.80f, 0.75f),
            },

            new AgeGroupDef()
            {
                DefName = "GenerationX",
                Label = "Generation X",
                SortingOrder = 2,
                Color = new Color(0.55f, 0.70f, 0.30f),
            },

            new AgeGroupDef()
            {
                DefName = "Millenials",
                Label = "Millenials",
                SortingOrder = 1,
                Color = new Color(0.80f, 0.55f, 0.25f),
            },

            new AgeGroupDef()
            {
                DefName = "GenerationZ",
                Label = "Gen Z",
                SortingOrder = 0,
                Color = new Color(0.65f, 0.40f, 0.45f),
            },
        };
    }

    [DefOf]
    public static class AgeGroupDefOf
    {
        public static AgeGroupDef Boomers;
        public static AgeGroupDef GenerationX;
        public static AgeGroupDef Millenials;
        public static AgeGroupDef GenerationZ;
    }
}
