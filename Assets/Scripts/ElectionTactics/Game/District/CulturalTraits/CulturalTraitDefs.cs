using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public static class CulturalTraitDefs
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
        private static string STRONG_ECONOMY = "StrongEconomy";
        private static string WEAK_ECONOMY = "WeakEconomy";
        private static string EXPORTER = "Exporter";
        private static string GROWING_POPULATION = "GrowingPopulation";
        private static string DECLINING_POPULATION = "DecliningPopulation";
        private static string PATRIOTIC = "Patirotic";
        private static string PROPORTIONAL_REPRESENTATION = "ProportionalRepresentation";
        private static string MAJORITY_BONUS = "MajorityBonus";

        public static List<CulturalTraitDef> Defs => new List<CulturalTraitDef>()
        {
            // Religion
            new CulturalTraitDef()
            {
                DefName = RELIGIOUS,
                Label = "Religious",
                Description = "Religion policy effectiveness in this district is doubled.",
                TraitClass = typeof(CT_Religious),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    SECULAR,
                    FANATIC
                },
                RequiresReligion = true,
            },

            new CulturalTraitDef()
            {
                DefName = SECULAR,
                Label = "Secular",
                Description = "Religion policy effectiveness in this district is halved.",
                TraitClass = typeof(CT_Secular),
                Commonness = 20,
                ForbiddenCulturalTraits = new List<string>()
                {
                    RELIGIOUS,
                    FANATIC
                },
                RequiresReligion = true,
            },

            new CulturalTraitDef()
            {
                DefName = FANATIC,
                Label = "", // Set by trait class
                Description = "", // Set by trait class
                TraitClass = typeof(CT_Fanatics),
                Commonness = 30,
                ForbiddenCulturalTraits = new List<string>()
                {
                    RELIGIOUS,
                    SECULAR
                },
                RequiresReligion = true,
            },

            // Language 
            new CulturalTraitDef()
            {
                DefName = LINGUISTIC,
                Label = "Linguistic",
                Description = "Language policy effectiveness in this district is doubled.",
                TraitClass = typeof(CT_Linguistic),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    NON_LINGUISTIC,
                },
            },

            new CulturalTraitDef()
            {
                DefName = NON_LINGUISTIC,
                Label = "Non-Linguistic",
                Description = "Language policy effectiveness in this district is halved.",
                TraitClass = typeof(CT_NonLinguistic),
                Commonness = 50,
                ForbiddenCulturalTraits = new List<string>()
                {
                    LINGUISTIC,
                },
            },

            // Economy
            new CulturalTraitDef()
            {
                DefName = STRONG_ECONOMY,
                Label = "Strong Economy",
                Description = "",
                TraitClass = typeof(CT_StrongEconomy),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    WEAK_ECONOMY,
                },
            },
            new CulturalTraitDef()
            {
                DefName = WEAK_ECONOMY,
                Label = "Weak Economy",
                Description = "",
                TraitClass = typeof(CT_WeakEconomy),
                Commonness = 80,
                ForbiddenCulturalTraits = new List<string>()
                {
                    STRONG_ECONOMY,
                    EXPORTER
                },
            },
            new CulturalTraitDef()
            {
                DefName = EXPORTER,
                Label = "",
                Description = "",
                TraitClass = typeof(CT_Exporter),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    WEAK_ECONOMY,
                },
            },

            // Population
            new CulturalTraitDef()
            {
                DefName = GROWING_POPULATION,
                Label = "Growing Population",
                Description = "", // set by trait
                TraitClass = typeof(CT_GrowingPopulation),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    DECLINING_POPULATION
                },
            },

            new CulturalTraitDef()
            {
                DefName = DECLINING_POPULATION,
                Label = "Declining Population",
                Description = "", // set by trait
                TraitClass = typeof(CT_DecliningPopulation),
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    GROWING_POPULATION
                },
            },

            // Popularity General
            new CulturalTraitDef()
            {
                DefName = PATRIOTIC,
                Label = "Patriotic",
                Description = "District policy effectiveness of this district is doubled.",
                TraitClass = typeof(CT_Patriotic),
                Commonness = 100,
            },

            // Modifiers
            new CulturalTraitDef()
            {
                DefName = STABLE,
                Label = "Stable",
                Description = "The party that won the last election will get a bonus for the next one.",
                TraitClass = typeof(CT_Stable),
                Commonness = 70,
                ForbiddenCulturalTraits = new List<string>()
                {
                    REBELLIOUS,
                    REVOLUTIONARY,
                },
            },

            new CulturalTraitDef()
            {
                DefName = REBELLIOUS,
                Label = "Rebellious",
                Description = "The party that won the last election will get a malus for the next one.",
                TraitClass = typeof(CT_Rebellious),
                Commonness = 30,
                ForbiddenCulturalTraits = new List<string>()
                {
                    STABLE,
                    REVOLUTIONARY,
                },
            },

            new CulturalTraitDef()
            {
                DefName = REVOLUTIONARY,
                Label = "Revolutionary",
                Description = "The party that won the last election will be excluded for the next one.",
                TraitClass = typeof(CT_Revolutionary),
                Commonness = 10,
                ForbiddenCulturalTraits = new List<string>()
                {
                    STABLE,
                    REBELLIOUS,
                },
            },


            // Voting System
            new CulturalTraitDef()
            {
                DefName = PROPORTIONAL_REPRESENTATION,
                Label = "Proportional Representation",
                Description = "Seats in this district are awarded to all parties proportially according to their voter share.",
                TraitClass = typeof(CT_ProportionalRepresentation),
                Commonness = 65,
                IsSeatDistributionTrait = true,
            },

            new CulturalTraitDef()
            {
                DefName = MAJORITY_BONUS,
                Label = "Majority Bonus",
                Description = "Seats in this district are awarded in a way that the winning party gets the absolute majority, and the rest of the seats are distributed proportionally along the other parties according to their voter share.",
                TraitClass = typeof(CT_MajorityBonus),
                Commonness = 25,
                IsSeatDistributionTrait = true,
            },
        };
    }
}
