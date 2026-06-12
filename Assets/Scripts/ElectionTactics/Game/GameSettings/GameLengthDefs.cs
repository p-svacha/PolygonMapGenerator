using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GameLengthDef : Def
    {
        public float ModifierFactor { get; init; }
    }

    public class GameLengthDefs
    {
        public static List<GameLengthDef> Defs => new List<GameLengthDef>()
        {
            new GameLengthDef()
            {
                DefName = "VeryShort",
                Label = "Very Short (x0.5)",
                ModifierFactor = 0.5f,
            },

            new GameLengthDef()
            {
                DefName = "Short",
                Label = "Short (x0.75)",
                ModifierFactor = 0.75f,
            },

            new GameLengthDef()
            {
                DefName = "Standard",
                Label = "Standard",
                ModifierFactor = 1f,
            },

            new GameLengthDef()
            {
                DefName = "Long",
                Label = "Long (x1.5)",
                ModifierFactor = 1.5f,
            },

            new GameLengthDef()
            {
                DefName = "VeryLong",
                Label = "Very Long (x2)",
                ModifierFactor = 2f,
            },

            new GameLengthDef()
            {
                DefName = "Marathon",
                Label = "Marathon (x3)",
                ModifierFactor = 3f,
            },
        };
    }

    [DefOf]
    public static class GameLengthDefOf
    {
        public static GameLengthDef VeryShort;
        public static GameLengthDef Short;
        public static GameLengthDef Standard;
        public static GameLengthDef Long;
        public static GameLengthDef VeryLong;
        public static GameLengthDef Marathon;
    }
}
