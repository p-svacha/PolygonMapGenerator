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
                DefName = "PolicyImpact",
                Label = "Policy Impact",
                Color = new Color(0.4f, 0.4f, 0.2f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "Seats",
                Label = "Seats",
                Color = new Color(0.4f, 0.2f, 0.4f),
            },

            new CulturalTraitCategoryDef()
            {
                DefName = "Popularity",
                Label = "Popularity",
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
        public static CulturalTraitCategoryDef PolicyImpact;
        public static CulturalTraitCategoryDef Seats;
        public static CulturalTraitCategoryDef Popularity;
        public static CulturalTraitCategoryDef PopulationDevelopment;
    }
}
