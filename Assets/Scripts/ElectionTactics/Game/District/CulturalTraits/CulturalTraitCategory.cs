using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class CulturalTraitCategoryDef : Def
    {
        public Color Color { get; init; }
    }

    public class CulturalTraitCategoryDefs
    {
        public static List<CulturalTraitCategoryDef> Defs => new List<CulturalTraitCategoryDef>()
        {
            new CulturalTraitCategoryDef()
            {
                DefName = "Demographic",
                Label = "Demographic",
                Color = new Color(0.4f, 0.4f, 0.2f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "Economic",
                Label = "Economic",
                Color = new Color(0.2f, 0.4f, 0.4f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "SeatDistribution",
                Label = "Seat Distribution",
                Color = new Color(0.4f, 0.2f, 0.4f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "Political",
                Label = "Political",
                Color = new Color(0.4f, 0.2f, 0.2f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "PopulationDevelopment",
                Label = "Population Development",
                Color = new Color(0.2f, 0.4f, 0.2f),
            },
        };
    }

    [DefOf]
    public static class CulturalTraitCategoryDefOf
    {
        public static CulturalTraitCategoryDef Demographic;
        public static CulturalTraitCategoryDef Economic;
        public static CulturalTraitCategoryDef SeatDistribution;
        public static CulturalTraitCategoryDef Political;
        public static CulturalTraitCategoryDef PopulationDevelopment;
    }
}
