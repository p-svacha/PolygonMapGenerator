using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class RandomEventDef : Def
    {
        /// <summary>
        /// Class of the actual random event class that will be instantiated, containing the logic.
        /// </summary>
        public System.Type RandomEventClass { get; init; } = typeof(RandomEvent);

        /// <summary>
        /// How likely the event is to appear. Only relevant relative to each other. The general chance if ANY random event appears after an election is defined in ElectionTacticsGame.RANDOM_EVENT_CHANCE.
        /// </summary>
        public int Commonness { get; init; }
    }

    public class RandomEventDefs
    {
        public static List<RandomEventDef> Defs => new List<RandomEventDef>()
        {
            new RandomEventDef()
            {
                DefName = "Disaster",
                Description = "A natural disaster hits a random district, instantly reducing the population by a big number.",
                RandomEventClass = typeof(RE_Disaster),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "TraitGain",
                Description = "A random district gains a random new trait.",
                RandomEventClass = typeof(RE_TraitGained),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "TraitLoss",
                Description = "A random district loses a random trait",
                RandomEventClass = typeof(RE_TraitLost),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "ReligionChange",
                Description = "A random district changes their religion to a random new one.",
                RandomEventClass = typeof(RE_ReligionChange),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "LanguageChange",
                Description = "A random district changes their language to a random new one.",
                RandomEventClass = typeof(RE_LanguageChange),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "AgeGroupChange",
                Description = "A random district changes their primary age group to a random new one.",
                RandomEventClass = typeof(RE_AgeGroupChange),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "CapitalMove",
                Description = "The capital cultural trait moves to a random other district. Only valid if capital is already visible.",
                RandomEventClass = typeof(RE_CapitalMove),
                Commonness = 100,
            },

            new RandomEventDef()
            {
                DefName = "DistrictRemoved",
                Description = "A random district gets removed from the game.",
                RandomEventClass = typeof(RE_DistrictRemoved),
                Commonness = 40,
            }
        };
    }
}
