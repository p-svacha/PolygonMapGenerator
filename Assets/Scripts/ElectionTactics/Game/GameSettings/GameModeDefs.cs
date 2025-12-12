using System.Collections.Generic;
using UnityEngine;

public class GameModeDef : Def { }

public static class GameModeDefs
{
    public static List<GameModeDef> Defs => new List<GameModeDef>()
    {
        new GameModeDef()
        {
            DefName = "Classic",
            Label = "Classic",
            Description = "The classic gamemode where all parties compete until one has won 7 elections in total.",
        },

        new GameModeDef()
        {
            DefName = "BattleRoyale",
            Label = "Battle Royale",
            Description = "All parties start the game with 100 influence. Each election and seat will affect influence. A party is eliminated when influence goes below 0. The game ends when there is one party left."
        }
    };
}

[DefOf]
public static class GameModeDefOf
{
    public static GameModeDef Classic;
    public static GameModeDef BattleRoyale;
}
