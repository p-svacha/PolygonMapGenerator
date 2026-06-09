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
        public float PopulationGrowthRateModifier { get; init; } // Additive modifier, in %
        public int Commonness { get; init; } // Commonness weight, how often this age group is dominant in a distrcit
    }

    public static class AgeGroupDefs
    {
        public static List<AgeGroupDef> Defs => new List<AgeGroupDef>()
        {
            new AgeGroupDef()
            {
                DefName = "Boomers",
                Label = "Baby Boomers",
                Description = "The population skews older. Slightly reduces the district's natural population growth rate.",
                SortingOrder = 3,
                Color = new Color(0.30f, 0.80f, 0.75f),
                PopulationGrowthRateModifier = -1f,
                Commonness = 31,
            },

            new AgeGroupDef()
            {
                DefName = "GenerationX",
                Label = "Generation X",
                Description = "The population is middle-aged. Has a small negative effect on population growth rate.",
                SortingOrder = 2,
                Color = new Color(0.55f, 0.70f, 0.30f),
                PopulationGrowthRateModifier = -0.5f,
                Commonness = 27,
            },

            new AgeGroupDef()
            {
                DefName = "Millenials",
                Label = "Millenials",
                Description = "The population skews younger. Has a small positive effect on population growth rate.",
                SortingOrder = 1,
                Color = new Color(0.80f, 0.55f, 0.25f),
                PopulationGrowthRateModifier = +0.5f,
                Commonness = 23,
            },

            new AgeGroupDef()
            {
                DefName = "GenerationZ",
                Label = "Gen Z",
                Description = "The population is predominantly young. Slightly increases the district's natural population growth rate.",
                SortingOrder = 0,
                Color = new Color(0.65f, 0.40f, 0.45f),
                PopulationGrowthRateModifier = +1f,
                Commonness = 19,
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
