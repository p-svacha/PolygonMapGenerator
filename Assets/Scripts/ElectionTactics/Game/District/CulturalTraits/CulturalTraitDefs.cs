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
        private static string SWING_DISTRICT = "SwingDistrict";

        // Note: The labels and descriptions defined here are often overriden by the trait class itself with more instance-specific details. Here they are very general. "Impact" refers to the popularity gain by the policy.

        public static List<CulturalTraitDef> Defs => new List<CulturalTraitDef>()
        {
            // Religion
            new CulturalTraitDef()
            {
                DefName = RELIGIOUS,
                Label = "Religious",
                Description = "Base religion policy impact in this district is doubled.",
                TraitClass = typeof(CT_Religious),
                Category = CulturalTraitCategoryDefOf.Demographic,
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
                Description = "Base religion policy impact in this district is halved.",
                TraitClass = typeof(CT_Secular),
                Category = CulturalTraitCategoryDefOf.Demographic,
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
                Label = "Religion Fanatics",
                Description = "+10 impact of religion policy of the districts religion. All other religion policies give -3.",
                TraitClass = typeof(CT_Fanatics),
                Category = CulturalTraitCategoryDefOf.Demographic,
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
                Description = "Base language policy impact in this district is doubled.",
                TraitClass = typeof(CT_Linguistic),
                Category = CulturalTraitCategoryDefOf.Demographic,
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
                Description = "Base language policy impact in this district is halved.",
                TraitClass = typeof(CT_NonLinguistic),
                Category = CulturalTraitCategoryDefOf.Demographic,
                Commonness = 20,
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
                Description = "Base economy policy impact in this district is doubled.",
                TraitClass = typeof(CT_StrongEconomy),
                Category = CulturalTraitCategoryDefOf.Economic,
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
                Description = "Base economy policy impact in this district is halved.",
                TraitClass = typeof(CT_WeakEconomy),
                Category = CulturalTraitCategoryDefOf.Economic,
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
                Label = "Exporter",
                Description = "Policy for dominant industry in this district has +7 impact and also gives +3 popularity in all adjacent districts.",
                TraitClass = typeof(CT_Exporter),
                Category = CulturalTraitCategoryDefOf.Economic,
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
                Description = "The population of this district grows quickly, leading to it being worth more seats over time.",
                TraitClass = typeof(CT_GrowingPopulation),
                Category = CulturalTraitCategoryDefOf.PopulationDevelopment,
                Commonness = 90,
                PopulationGrowthRateModifier = +7.0f,
                ForbiddenCulturalTraits = new List<string>()
                {
                    DECLINING_POPULATION
                },
            },

            new CulturalTraitDef()
            {
                DefName = DECLINING_POPULATION,
                Label = "Declining Population",
                Description = "The population of this district is in quick decline, leading to it being fewer more seats over time.",
                TraitClass = typeof(CT_DecliningPopulation),
                Category = CulturalTraitCategoryDefOf.PopulationDevelopment,
                Commonness = 50,
                PopulationGrowthRateModifier = -5.0f,
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
                Description = "Base district policy impact of this district is doubled.",
                TraitClass = typeof(CT_Patriotic),
                Category = CulturalTraitCategoryDefOf.Demographic,
                Commonness = 100,
            },

            // Modifiers
            new CulturalTraitDef()
            {
                DefName = STABLE,
                Label = "Stable",
                Description = "The party that won the last election will get a +30 popularity bonus for the next one.",
                TraitClass = typeof(CT_Stable),
                Category = CulturalTraitCategoryDefOf.Political,
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
                Description = "The party that won the last election will get a -30 popularity penalty for the next one.",
                TraitClass = typeof(CT_Rebellious),
                Category = CulturalTraitCategoryDefOf.Political,
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
                Category = CulturalTraitCategoryDefOf.Political,
                Commonness = 10,
                ForbiddenCulturalTraits = new List<string>()
                {
                    STABLE,
                    REBELLIOUS,
                },
            },


            // Voting
            new CulturalTraitDef()
            {
                DefName = SWING_DISTRICT,
                Label = "Swing District",
                Description = "Election outcomes in this district are less predictable. Results may not fully accurately represent party popularities.",
                TraitClass = typeof(CT_SwingDistrict),
                Category = CulturalTraitCategoryDefOf.Political,
                Commonness = 50,
            },

            new CulturalTraitDef()
            {
                DefName = PROPORTIONAL_REPRESENTATION,
                Label = "Proportional Representation",
                Description = "Seats in this district are divided as closely as possible to each party’s vote share.\n\nReplaces the Winner-Takes-All seat distribution system.",
                TraitClass = typeof(CT_ProportionalRepresentation),
                Category = CulturalTraitCategoryDefOf.SeatDistribution,
                Commonness = 70,
                IsSeatDistributionTrait = true,
            },

            new CulturalTraitDef()
            {
                DefName = MAJORITY_BONUS,
                Label = "Majority Representation",
                Description = "Seats in this district are distributed proportionally, but larger parties gain a slight advantage.\n\nReplaces the Winner-Takes-All seat distribution system.",
                TraitClass = typeof(CT_MajorityBonus),
                Category = CulturalTraitCategoryDefOf.SeatDistribution,
                Commonness = 25,
                IsSeatDistributionTrait = true,
            },
        };
    }
}
