using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class RandomEventFrequencyDef : Def
    {
        public float RandomEventChance { get; init; }
    }

    public class RandomEventFrequencyDefs
    {
        public static List<RandomEventFrequencyDef> Defs => new List<RandomEventFrequencyDef>()
        {
            new RandomEventFrequencyDef()
            {
                DefName = "Never",
                Label = "Never",
                RandomEventChance = 0f,
            },

            new RandomEventFrequencyDef()
            {
                DefName = "Rare",
                Label = "Rare (10%)",
                RandomEventChance = 0.10f,
            },

            new RandomEventFrequencyDef()
            {
                DefName = "Standard",
                Label = "Standard (25%)",
                RandomEventChance = 0.25f,
            },

            new RandomEventFrequencyDef()
            {
                DefName = "Often",
                Label = "Often (40%)",
                RandomEventChance = 0.40f,
            },

            new RandomEventFrequencyDef()
            {
                DefName = "VeryOften",
                Label = "Very Often (70%)",
                RandomEventChance = 0.70f,
            },

            new RandomEventFrequencyDef()
            {
                DefName = "Always",
                Label = "Always",
                RandomEventChance = 1f,
            },
        };
    }

    [DefOf]
    public static class RandomEventFrequencyDefOf
    {
        public static RandomEventFrequencyDef Never;
        public static RandomEventFrequencyDef Rare;
        public static RandomEventFrequencyDef Standard;
        public static RandomEventFrequencyDef Often;
        public static RandomEventFrequencyDef VeryOften;
        public static RandomEventFrequencyDef Always;
    }
}
