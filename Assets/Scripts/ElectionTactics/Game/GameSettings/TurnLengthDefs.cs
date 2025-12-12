using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnLengthDef : Def
{
    /// <summary>
    /// The base turn length.
    /// </summary>
    public int BaseTurnTime { get; init; }

    /// <summary>
    /// How much the turn timer increases each turn.
    /// </summary>
    public int TurnLengthIncreasePerTurn { get; init; }
}

public static class TurnLengthDefs
{
    public static List<TurnLengthDef> Defs => new List<TurnLengthDef>()
    {
        new TurnLengthDef()
        {
            DefName = "Slow",
            Label = "slow",
            BaseTurnTime = 160,
            TurnLengthIncreasePerTurn = 20,
        },

        new TurnLengthDef()
        {
            DefName = "Medium",
            Label = "medium",
            BaseTurnTime = 75,
            TurnLengthIncreasePerTurn = 16,
        },

        new TurnLengthDef()
        {
            DefName = "Fast",
            Label = "fast",
            BaseTurnTime = 50,
            TurnLengthIncreasePerTurn = 12,
        },
    };
}

[DefOf]
public static class TurnLengthDefOf
{
    public static TurnLengthDef Fast;
    public static TurnLengthDef Medium;
    public static TurnLengthDef Slow;
}
