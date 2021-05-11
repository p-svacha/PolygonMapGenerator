using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame {
    /// <summary>
    /// An army represents a group of troops that move from one territory to another at a specific time.
    /// </summary>
    public class Army
    {
        public int Turn;
        public int NumTroops;
        public float Strength = ParriskGame.BaseOffenseStrength;

        public Territory SourceTerritory;
        public Territory TargetTerritory;
        public Player SourcePlayer;
        public Player TargetPlayer;

        public ArmyType Type;

        public bool IsWaterArmy;

        public VisualArmy VisualArmy;

        public Army(int turn, Territory sourceTerritory, Territory targetTerritory, Player sourcePlayer, Player targetPlayer, int numTroops, bool isWaterMovement)
        {
            Turn = turn;
            SourceTerritory = sourceTerritory;
            TargetTerritory = targetTerritory;
            SourcePlayer = sourcePlayer;
            TargetPlayer = targetPlayer;
            Type = sourcePlayer == targetPlayer ? ArmyType.Support : ArmyType.Attack;
            NumTroops = numTroops;
            IsWaterArmy = isWaterMovement;
        }
    }

    public enum ArmyType
    {
        Attack,
        Support
    }
}
