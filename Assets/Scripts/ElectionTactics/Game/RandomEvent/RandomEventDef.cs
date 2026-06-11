using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public abstract class RandomEventDef : Def
    {
        /// <summary>
        /// Class of the actual random event class that will be instantiated, containing the logic.
        /// </summary>
        public System.Type RandomEventClass { get; init; } = typeof(RandomEvent);

        /// <summary>
        /// How likely the event is to appear. Only relevant relative to each other. The general chance if ANY random event appears after an election is defined in ElectionTacticsGame.RANDOM_EVENT_CHANCE.
        /// </summary>
        public int Commonness { get; init; }

        public abstract bool CanExecute();
        public abstract void Execute();
    }

    public class RandomEventDefs
    {
        public static List<RandomEventDef> Defs => new List<RandomEventDef>()
        {
            new RE_Disaster()
            {
                DefName = "Disaster",
                Description = "A natural disaster hits a random district, instantly reducing the population by a big number.",
                Commonness = 100,
            },

            new RE_TraitGained()
            {
                DefName = "TraitGain",
                Description = "A random district gains a random new trait.",
                Commonness = 100,
            },

            new RE_TraitLost()
            {
                DefName = "TraitLoss",
                Description = "A random district loses a random trait",
                Commonness = 100,
            },

            new RE_ReligionChange()
            {
                DefName = "ReligionChange",
                Description = "A random district changes their religion to a random new one.",
                Commonness = 100,
            },

            new RE_LanguageChange()
            {
                DefName = "LanguageChange",
                Description = "A random district changes their language to a random new one.",
                Commonness = 100,
            },

            new RE_AgeGroupChange()
            {
                DefName = "AgeGroupChange",
                Description = "A random district changes their primary age group to a random new one.",
                Commonness = 100,
            },

            new RE_CapitalMove()
            {
                DefName = "CapitalMove",
                Description = "The capital cultural trait moves to a random other district. Only valid if capital is already visible.",
                Commonness = 100,
            },

            new RE_DistrictRemoved()
            {
                DefName = "DistrictRemoved",
                Description = "A random district gets removed from the game.",
                Commonness = 40,
            }
        };
    }
}
