using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    /// <summary>
    /// A battle represents a fight of multiple armies (of at least two different players) in a specific border/territory at a specific time. There is always exactly one player that wins a battle.
    /// </summary>
    public abstract class Battle
    {
        public int Turn;
        public BattleType Type;
        public List<Army> Armies = new List<Army>();
        public List<Territory> Territories = new List<Territory>();
        public List<Player> Players = new List<Player>();

        public Dictionary<Player, int> Casualties = new Dictionary<Player, int>();
        public Player Winner;
        public int WinnerTroopsRemaining;
        public List<Player> Losers = new List<Player>();

        public bool SupportOnly; // If this is true, only armies from one player are involved

        public Battle(VisualArmy armyPrefab, int turn, List<Army> armies)
        {
            Turn = turn;
            Armies = armies;

            foreach(Army army in armies)
            {
                if (!Territories.Contains(army.TargetTerritory)) Territories.Add(army.TargetTerritory);
                if (!Players.Contains(army.SourcePlayer)) Players.Add(army.SourcePlayer);
                if (!Players.Contains(army.TargetPlayer)) Players.Add(army.TargetPlayer);
            }
        }

        public abstract void ResolveBattle();
    }

    public enum BattleType
    {
        BorderBattle, // Two territories attack each other
        TerritoryBattle // A territory gets attacked by one or more armies and has to defend itself
    }
}
