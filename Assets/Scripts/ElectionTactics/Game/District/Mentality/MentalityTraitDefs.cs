using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public static class MentalityTraitDefs
    {
        // Defname constants
        private static string RELIGIOUS = "Religious";
        private static string SECULAR = "Secular";
        private static string LINGUISTIC = "Linguistic";
        private static string NON_LINGUISTIC = "NonLinguistic";
        private static string STABLE = "Stable";
        private static string REBELLIOUS = "Rebellious";
        private static string REVOLUTIONARY = "Revolutionary";

        public static List<MentalityTraitDef> Defs => new List<MentalityTraitDef>()
        {
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
                },
                RequiresReligion = true,
            },

            new MentalityTraitDef()
            {
                DefName = SECULAR,
                Label = "secular",
                Description = "Religion policy effectiveness in this district is halved.",
                TraitClass = typeof(MentalityTrait_Secular),
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    RELIGIOUS,
                },
                RequiresReligion = true,
            },

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
                Commonness = 100,
                ForbiddenMentalityTraits = new List<string>()
                {
                    LINGUISTIC,
                },
            },

            new MentalityTraitDef()
            {
                DefName = STABLE,
                Label = "stable",
                Description = "The party that won the last election will get a bonus for the next one.",
                TraitClass = typeof(MentalityTrait_Stable),
                Commonness = 60,
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
                Commonness = 40,
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
                Commonness = 20,
                ForbiddenMentalityTraits = new List<string>()
                {
                    STABLE,
                    REBELLIOUS,
                },
            },
        };
    }
}
