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
        private static string GEOCENTRIC = "Geocentric";
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
        private static string HISTORICAL_LEGACY = "HistoricalLegacy";

        private static string CAPITAL = "Capital";
        private static string IMPRESSIONABLE = "Impressionable";
        private static string INFLUENTAL = "Influental";
        private static string IMMIGRATION_HUB = "ImmigrationHub";
        private static string PARTNER_DISTRICT = "PartnerDistrict";
        private static string RIVAL_DISTRICT = "RivalDistrict";
        private static string IMPATIENT = "Impatient";
        private static string IMPERIALISTIC = "Imperialistic";
        private static string FOCAL_POINT = "FocalPoint";

        // Note: The labels and descriptions defined here are often overriden by the trait class itself with more instance-specific details. Here they are very general. "Impact" refers to the popularity gain by the policy.

        public static List<CulturalTraitDef> Defs => new List<CulturalTraitDef>()
        {
            #region Policy Impact

            // Religion
            new CulturalTraitDef()
            {
                DefName = RELIGIOUS,
                Label = "Religious",
                Adjective = "religious",
                Describer = "with a focus on religion",
                Description = "Base religion policy impact in this district is doubled.",
                TraitClass = typeof(CT_Religious),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
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
                Adjective = "secular",
                Description = "Base religion policy impact in this district is halved.",
                TraitClass = typeof(CT_Secular),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
                Commonness = 10,
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
                Adjective = "fanatically religious",
                Describer = "with a fanatic religious community",
                Description = "+10 impact of religion policy of the districts religion. All other religion policies give -3.",
                TraitClass = typeof(CT_Fanatics),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
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
                Adjective = "linguistic",
                Describer = "with a focus on language",
                Description = "Base language policy impact in this district is doubled.",
                TraitClass = typeof(CT_Linguistic),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
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
                Adjective = "non-linguistic",
                Description = "Base language policy impact in this district is halved.",
                TraitClass = typeof(CT_NonLinguistic),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
                Commonness = 10,
                ForbiddenCulturalTraits = new List<string>()
                {
                    LINGUISTIC,
                },
            },

            // Geography
            new CulturalTraitDef()
            {
                DefName = GEOCENTRIC,
                Label = "Geocentric",
                Adjective = "geocentric",
                Describer = "that highly values its geography",
                Description = "Base geography policy impact in this district is doubled.",
                TraitClass = typeof(CT_Geocentric),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
                Commonness = 100,
            },

            // Economy
            new CulturalTraitDef()
            {
                DefName = STRONG_ECONOMY,
                Label = "Strong Economy",
                Adjective = "economy-focussed",
                Description = "Base economy policy impact in this district is doubled.",
                TraitClass = typeof(CT_StrongEconomy),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
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
                Describer = "with a weak economy",
                Description = "Base economy policy impact in this district is halved.",
                TraitClass = typeof(CT_WeakEconomy),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
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
                Description = "Policy for dominant industry in this district has +3 impact and also gives +3 popularity in all adjacent districts.",
                TraitClass = typeof(CT_Exporter),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
                Commonness = 100,
                ForbiddenCulturalTraits = new List<string>()
                {
                    WEAK_ECONOMY,
                },
            },

            // District
            new CulturalTraitDef()
            {
                DefName = PATRIOTIC,
                Label = "Patriotic",
                Adjective = "patriotic",
                Description = "Base district policy impact of this district is doubled.",
                TraitClass = typeof(CT_Patriotic),
                Category = CulturalTraitCategoryDefOf.PolicyImpact,
                Commonness = 100,
            },

            #endregion

            #region Population Development

            // Population
            new CulturalTraitDef()
            {
                DefName = GROWING_POPULATION,
                Label = "Growing Population",
                Adjective = "rapidly growing",
                Describer = "with a rapidly growing population,",
                Description = "The population of this district grows quickly, leading to it being worth more seats over time.",
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
                Adjective = "rapidly declining",
                Describer = "with a rapidly declining population,",
                Description = "The population of this district is in quick decline, leading to it being fewer more seats over time.",
                Category = CulturalTraitCategoryDefOf.PopulationDevelopment,
                Commonness = 50,
                PopulationGrowthRateModifier = -5.0f,
                ForbiddenCulturalTraits = new List<string>()
                {
                    GROWING_POPULATION
                },
            },

            new CulturalTraitDef()
            {
                DefName = IMMIGRATION_HUB,
                Label = "Immigration Hub",
                Describer = ", acting as local immigration hub,",
                Description = "+5% to population growth in this district, but -2% population growth every adjacent district.",
                Category = CulturalTraitCategoryDefOf.PopulationDevelopment,
                Commonness = 80,
                PopulationGrowthRateModifier = +5.0f,
                NeighbourPopulationGrowthModifier = -2.0f,
            },

            new CulturalTraitDef()
            {
                DefName = IMPERIALISTIC,
                Label = "Imperialistic",
                Adjective = "imperialistic",
                Description = "Newly added districts adjacent to this will have guaranteed the same language/religion.",
                TraitClass = typeof(CT_Imperialistic),
                Category = CulturalTraitCategoryDefOf.PopulationDevelopment,
                Commonness = 80,
            },

            #endregion

            #region Popularity

            new CulturalTraitDef()
            {
                DefName = IMPRESSIONABLE,
                Label = "Impressionable",
                Adjective = "impressionable",
                Describer = ", which is easily influenced by its neighbours,",
                Description = "Each party gains a +10 popularity bonus for each adjacent district where they won the last election.",
                TraitClass = typeof(CT_Impressionable),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 80,
            },

            new CulturalTraitDef()
            {
                DefName = FOCAL_POINT,
                Label = "Focal Point",
                Description = "This district has a strong preference for [Language/Religion/etc.] policy. If you have invested at least 5 points into the policy, you gain +30 popularity here.",
                TraitClass = typeof(CT_FocalPoint),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 100,
            },

            // Modifiers
            new CulturalTraitDef()
            {
                DefName = STABLE,
                Label = "Stable",
                Adjective = "stable",
                Description = "The party that won the last election will get a +30 popularity bonus for the next one.",
                TraitClass = typeof(CT_Stable),
                Category = CulturalTraitCategoryDefOf.Popularity,
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
                Adjective = "rebellious",
                Description = "The party that won the last election will get a -30 popularity penalty for the next one.",
                TraitClass = typeof(CT_Rebellious),
                Category = CulturalTraitCategoryDefOf.Popularity,
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
                Adjective = "revolutionary",
                Description = "The party that won the last election will be excluded for the next one.",
                TraitClass = typeof(CT_Revolutionary),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 10,
                ForbiddenCulturalTraits = new List<string>()
                {
                    STABLE,
                    REBELLIOUS,
                },
            },

            new CulturalTraitDef()
            {
                DefName = IMPATIENT,
                Label = "Impatient",
                Adjective = "impatient",
                Description = $"Parties that don't win any seat in {CT_Impatient.THRESHOLD} consecutive cycles get permanently excluded from competing in elections.",
                TraitClass = typeof(CT_Impatient),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 50,
            },

            new CulturalTraitDef()
            {
                DefName = HISTORICAL_LEGACY,
                Label = "Historical Legacy",
                Describer = "with a strong historical legacy",
                Description = "The most popular party gets a permanent popularity bonus.",
                TraitClass = typeof(CT_HistoricalLegacy),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 70,
            },


            // Voting
            new CulturalTraitDef()
            {
                DefName = SWING_DISTRICT,
                Label = "Swing District",
                Adjective = "swing",
                Describer = ", known for its politically undecided population,",
                Description = "Election outcomes in this district are less predictable. Results may not fully accurately represent party popularities.",
                TraitClass = typeof(CT_SwingDistrict),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 50,
            },

            // Other district influences
            new CulturalTraitDef()
            {
                DefName = PARTNER_DISTRICT,
                Label = "Partner District",
                Description = "This district has a partnership with <PARTNER>. 20% of your popularity in <PARTNER> is applied as a popularity bonus here, and vice versa.",
                TraitClass = typeof(CT_PartnerDistrict),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 50,
                RequiresMultipleDistricts = true,
            },

            new CulturalTraitDef()
            {
                DefName = RIVAL_DISTRICT,
                Label = "Rival District",
                Description = "This district has a rivalry with <PARTNER>. 20% of your popularity in <PARTNER> is applied as a popularity penalty here, and vice versa.",
                TraitClass = typeof(CT_RivalDistrict),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 50,
                RequiresMultipleDistricts = true,
            },

            new CulturalTraitDef()
            {
                DefName = INFLUENTAL,
                Label = "Influental",
                Adjective = "influental",
                Describer = ", known to have an impact on its neighbours,",
                Description = "10% of your popularity in this district is applied as a popularity bonus in all adjacent districts.",
                TraitClass = typeof(CT_Influental),
                Category = CulturalTraitCategoryDefOf.Popularity,
                Commonness = 80,
            },

            #endregion

            #region Seats

            new CulturalTraitDef()
            {
                DefName = PROPORTIONAL_REPRESENTATION,
                Label = "Proportional Representation",
                Describer = ", that distributes its seats with proportional representation,",
                Description = "Seats in this district are divided as closely as possible to each party’s vote share.\n\nReplaces the Winner-Takes-All seat distribution system.",
                Category = CulturalTraitCategoryDefOf.Seats,
                Commonness = 0, // applied separately through seat distribution method choice
                IsSeatDistributionTrait = true,
            },

            new CulturalTraitDef()
            {
                DefName = MAJORITY_BONUS,
                Label = "Majority Representation",
                Describer = ", that distributes its seat with a majority representation,",
                Description = "Seats in this district are distributed proportionally, but larger parties gain a slight advantage.\n\nReplaces the Winner-Takes-All seat distribution system.",
                Category = CulturalTraitCategoryDefOf.Seats,
                Commonness = 0, // applied separately through seat distribution method choice
                IsSeatDistributionTrait = true,
            },

            // Seats
            new CulturalTraitDef()
            {
                DefName = CAPITAL,
                Label = "Capital",
                Adjective = "capital",
                Describer = ", that acts the country's capital,",
                Description = "This district is the nation's capital and holds 3 additional seats in parliament.",
                Category = CulturalTraitCategoryDefOf.Seats,
                Commonness = 0, // applied separately
                SeatModifier = +3,
            }

            #endregion
        };
    }
}
