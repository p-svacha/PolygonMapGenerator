using System.Collections.Generic;
using UnityEngine;

public class BotDifficultyDef : Def
{
    /// <summary>
    /// Minimum amount of policy points a bot can receiver per turn on this difficulty.
    /// </summary>
    public int MinPolicyPointsPerCycle { get; init; }

    /// <summary>
    /// Maximum amount of policy points a bot can receiver per turn on this difficulty.
    /// </summary>
    public int MaxPolicyPointsPerCycle { get; init; }
}

public static class BotDifficultyDefs
{
    public static List<BotDifficultyDef> Defs => new List<BotDifficultyDef>()
    {
        new BotDifficultyDef()
        {
            DefName = "Easy",
            Label = "easy",
            MinPolicyPointsPerCycle = 2,
            MaxPolicyPointsPerCycle = 5,
        },

        new BotDifficultyDef()
        {
            DefName = "Standard",
            Label = "standard",
            MinPolicyPointsPerCycle = 3,
            MaxPolicyPointsPerCycle = 6,
        },

        new BotDifficultyDef()
        {
            DefName = "Hard",
            Label = "hard",
            MinPolicyPointsPerCycle = 4,
            MaxPolicyPointsPerCycle = 7,
        },

        new BotDifficultyDef()
        {
            DefName = "Extreme",
            Label = "extreme",
            MinPolicyPointsPerCycle = 5,
            MaxPolicyPointsPerCycle = 8,
        },
    };
}
