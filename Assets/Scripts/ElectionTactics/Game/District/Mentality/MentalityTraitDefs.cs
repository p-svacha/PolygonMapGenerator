using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public static class MentalityTraitDefs
    {
        // Defname constants
        private static string RELIGIOUS = "Religious";
        private static string SECULAR = "Secular";
        private static string FANATIC = "Fanatic";
        private static string LINGUISTIC = "Linguistic";
        private static string NON_LINGUISTIC = "NonLinguistic";
        private static string STABLE = "Stable";
        private static string REBELLIOUS = "Rebellious";
        private static string REVOLUTIONARY = "Revolutionary";
        private static string ECONOMIC_POWERHOUSE = "EconomicPowerhouse";
        private static string EXPORTER = "Exporter";
        private static string GROWING_POPULATION = "GrowingPopulation";
        private static string DECLINING_POPULATION = "DecliningPopulation";

        public static List<MentalityTraitDef> Defs => new List<MentalityTraitDef>()
        {
            // Religion
            new MentalityTraitDef()
            {
                DefName = RELIGIOUS,
                Label = "religious",
                Description = "Religion policy effectiveness in this district is doubled.",
                TraitClass = typeof(MentalityTrait_Religious),
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    SECULAR,
                    FANATIC
                },
                RequiresReligion = true,
            },

            new MentalityTraitDef()
            {
                DefName = SECULAR,
                Label = "secular",
                Description = "Religion policy effectiveness in this district is halved.",
                TraitClass = typeof(MentalityTrait_Secular),
                Commonness = 40,
                ForbiddenMentalityTraits = new List<string>()
                {
                    RELIGIOUS,
                    FANATIC
                },
                RequiresReligion = true,
            },

            new MentalityTraitDef()
            {
                DefName = FANATIC,
                Label = "", // Set by trait class
                Description = "", // Set by trait class
                TraitClass = typeof(MentalityTrait_ReligionFanatics),
                Commonness = 50,
                ForbiddenMentalityTraits = new List<string>()
                {
                    RELIGIOUS,
                    SECULAR
                },
                RequiresReligion = true,
            },

            // Language 
            new MentalityTraitDef()
            {
                DefName = LINGUISTIC,
                Label = "linguistic",
                Description = "Language policy effectiveness in this district is doubled.",
                TraitClass = typeof(MentalityTrait_Linguistic),
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    NON_LINGUISTIC,
                },
            },

            new MentalityTraitDef()
            {
                DefName = NON_LINGUISTIC,
                Label = "non-linguistic",
                Description = "Language policy effectiveness in this district is halved.",
                TraitClass = typeof(MentalityTrait_NonLinguistic),
                Commonness = 40,
                ForbiddenMentalityTraits = new List<string>()
                {
                    LINGUISTIC,
                },
            },

            // Economy
            new MentalityTraitDef()
            {
                DefName = ECONOMIC_POWERHOUSE,
                Label = "economic powerhouse",
                Description = "",
                TraitClass = typeof(MentalityTrait_EconomicPowerhouse),
                Commonness = 100,
            },
            new MentalityTraitDef()
            {
                DefName = EXPORTER,
                Label = "",
                Description = "",
                TraitClass = typeof(MentalityTrait_Exporter),
                Commonness = 100,
            },

            // Population
            new MentalityTraitDef()
            {
                DefName = GROWING_POPULATION,
                Label = "growing population",
                Description = "", // set by trait
                TraitClass = typeof(MentalityTrait_GrowingPopulation),
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    DECLINING_POPULATION
                },
            },

            new MentalityTraitDef()
            {
                DefName = DECLINING_POPULATION,
                Label = "declining population",
                Description = "", // set by trait
                TraitClass = typeof(MentalityTrait_DecliningPopulation),
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    GROWING_POPULATION
                },
            },

            // Popularity General

            // Modifiers
            new MentalityTraitDef()
            {
                DefName = STABLE,
                Label = "stable",
                Description = "The party that won the last election will get a bonus for the next one.",
                TraitClass = typeof(MentalityTrait_Stable),
                Commonness = 70,
                ForbiddenMentalityTraits = new List<string>()
                {
                    REBELLIOUS,
                    REVOLUTIONARY,
                },
            },

            new MentalityTraitDef()
            {
                DefName = REBELLIOUS,
                Label = "rebellious",
                Description = "The party that won the last election will get a malus for the next one.",
                TraitClass = typeof(MentalityTrait_Rebellious),
                Commonness = 20,
                ForbiddenMentalityTraits = new List<string>()
                {
                    STABLE,
                    REVOLUTIONARY,
                },
            },

            new MentalityTraitDef()
            {
                DefName = REVOLUTIONARY,
                Label = "revolutionary",
                Description = "The party that won the last election will be excluded for the next one.",
                TraitClass = typeof(MentalityTrait_Revolutionary),
                Commonness = 5,
                ForbiddenMentalityTraits = new List<string>()
                {
                    STABLE,
                    REBELLIOUS,
                },
            },
        };
    }
}
