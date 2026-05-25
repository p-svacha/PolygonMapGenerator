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
            MinPolicyPointsPerCycle = 3,
            MaxPolicyPointsPerCycle = 6,
        },

        new BotDifficultyDef()
        {
            DefName = "Standard",
            Label = "standard",
            MinPolicyPointsPerCycle = 4,
            MaxPolicyPointsPerCycle = 7,
        },

        new BotDifficultyDef()
        {
            DefName = "Hard",
            Label = "hard",
            MinPolicyPointsPerCycle = 5,
            MaxPolicyPointsPerCycle = 8,
        },

        new BotDifficultyDef()
        {
            DefName = "Extreme",
            Label = "extreme",
            MinPolicyPointsPerCycle = 6,
            MaxPolicyPointsPerCycle = 9,
        },
    };
}

[DefOf]
public static class BotDifficultyDefOf
{
    public static BotDifficultyDef Easy;
    public static BotDifficultyDef Standard;
    public static BotDifficultyDef Hard;
    public static BotDifficultyDef Extreme;
}
