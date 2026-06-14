using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomicSectorDef : Def
    {
        public override Sprite Sprite => ResourceManager.LoadSprite($"ElectionTactics/Icons/Economy/{DefName}");

        /// <summary>
        /// Function that defines the commonness of appearing withing a district.
        /// </summary>
        public System.Func<District, int> GetWeight { get; init; } = (district) => 100;
    }

    public static class EconomicSectorDefs
    {
        public static List<EconomicSectorDef> Defs => new List<EconomicSectorDef>()
        {

            new EconomicSectorDef()
            {
                DefName = "Crafts",
                Label = "Crafts",
                Description = "Artisanal goods and local trades.\n\nOften found in districts with a minority language or religion.",
                GetWeight = (d) => 100
                    + (d.IsMinorityLanguage ? 50 : 0)
                    + (d.IsMinorityReligion ? 50 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Farming",
                Label = "Farming",
                Description = "Agriculture and livestock.\n\nOften found in large rural districts, rarely in dense urban ones.",
                GetWeight = (d) => 100
                    + (d.Density == DensityDefOf.Low ? 70 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Large) ? 40 : 0)
                    - (d.Density == DensityDefOf.High ? 60 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Finance",
                Label = "Finance",
                Description = "Banking and services.\n\nThe lifeblood of major urban districts, rare in the countryside.",
                GetWeight = (d) => 100
                    + (d.Density == DensityDefOf.High ? 80 : 0)
                    - (d.Density == DensityDefOf.Low ? 50 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Fishing",
                Label = "Fishing",
                Description = "Commercial fishing fleets.\n\nOnly found in districts next to a coast, lake, or river.",
                GetWeight = (d) =>
                    d.Geography.Any(g => g.Def == GeographyTraitDefOf.Coastal
                                      || g.Def == GeographyTraitDefOf.Lake
                                      || g.Def == GeographyTraitDefOf.River)
                    ? 140 : 0, // hard requirement: weight 0 without water access
            },

            new EconomicSectorDef()
            {
                DefName = "Forestry",
                Label = "Forestry",
                Description = "Timber and woodland industry.\n\nFavoured in large, low-density inland regions.",
                GetWeight = (d) => 100
                    + (d.Density == DensityDefOf.Low ? 50 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Landlocked) ? 30 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "FossilFuels",
                Label = "Fossil Fuels",
                Description = "Oil, gas, and coal extraction.\n\nMore common in large, coastal or low-density districts.",
                GetWeight = (d) => 100
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Large) ? 40 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Coastal) ? 20 : 0)
                    + (d.Density == DensityDefOf.Low ? 20 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Healthcare",
                Label = "Healthcare",
                Description = "Hospitals, care homes and senior services.\n\nLess common in rural areas, more common where the population skews older.",
                GetWeight = (d) => 100
                    + (d.Density == DensityDefOf.High ? 20 : 0)
                    + (d.Density == DensityDefOf.Medium ? 20 : 0)
                    + (d.AgeGroup == AgeGroupDefOf.Boomers ? 40 : 0)
                    + (d.AgeGroup == AgeGroupDefOf.GenerationX ? 20 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Manufacturing",
                Label = "Manufacturing",
                Description = "Factories and heavy industry.\n\nCommon almost anywhere, favouring medium-density districts.",
                GetWeight = (d) => 100
                    + (d.Density == DensityDefOf.Medium ? 40 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Mining",
                Label = "Mining",
                Description = "Extraction of ore and minerals.\n\nMore common in large, sparsely populated inland districts.",
                GetWeight = (d) => 100
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Large) ? 60 : 0)
                    + (d.Density == DensityDefOf.Low ? 40 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Landlocked) ? 30 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Renewables",
                Label = "Renewables",
                Description = "Wind, solar, and hydro power.\n\nMore common near rivers and in open rural districts.",
                GetWeight = (d) => 100
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.River) ? 60 : 0)
                    + (d.Density == DensityDefOf.Low ? 40 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Shipping",
                Label = "Shipping",
                Description = "Ports and maritime trade.\n\nThrives on coasts, especially on the nation's edges.",
                GetWeight = (d) => 100
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Coastal) ? 80 : 0)
                    + (d.Geography.Any(g =>
                           g.Def == GeographyTraitDefOf.East || g.Def == GeographyTraitDefOf.West ||
                           g.Def == GeographyTraitDefOf.North || g.Def == GeographyTraitDefOf.South) ? 30 : 0)
                    - (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Landlocked) ? 70 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Tech",
                Label = "Tech",
                Description = "Software and digital services.\n\nOften found in districts with younger populations.",
                GetWeight = (d) => 100
                    + (d.AgeGroup == AgeGroupDefOf.GenerationZ ? 80 : 0)
                    + (d.AgeGroup == AgeGroupDefOf.Millenials ? 50 : 0)
                    - (d.AgeGroup == AgeGroupDefOf.Boomers ? 30 : 0),
            },

            new EconomicSectorDef()
            {
                DefName = "Tourism",
                Label = "Tourism",
                Description = "Travel and hospitality.\n\nFlourishes on islands and along coastlines and lakes.",
                GetWeight = (d) => 100
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Island) ? 90 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Coastal) ? 60 : 0)
                    + (d.Geography.Any(g => g.Def == GeographyTraitDefOf.Lake) ? 40 : 0),
            },
        };
    }

    [DefOf]
    public static class EconomicSectorDefOf
    {
        public static EconomicSectorDef Crafts;
        public static EconomicSectorDef Farming;
        public static EconomicSectorDef Finance;
        public static EconomicSectorDef Fishing;
        public static EconomicSectorDef Forestry; 
        public static EconomicSectorDef FossilFuels;
        public static EconomicSectorDef Healthcare;
        public static EconomicSectorDef Manufacturing;
        public static EconomicSectorDef Mining;
        public static EconomicSectorDef Renewables;
        public static EconomicSectorDef Shipping;
        public static EconomicSectorDef Tech;
        public static EconomicSectorDef Tourism;
    }
}
