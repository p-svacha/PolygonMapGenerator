using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame {
    public class TroopMovement
    {
        public int Turn;

        public Territory SourceTerritory;
        public Territory TargetTerritory;
        public Player SourcePlayer;
        public Player TargetPlayer;

        public int NumTroops;

        public TroopMovement(int turn, Territory sourceTerritory, Territory targetTerritory, Player sourcePlayer, Player targetPlayer, int numTroops)
        {
            Turn = turn;
            SourceTerritory = sourceTerritory;
            TargetTerritory = targetTerritory;
            SourcePlayer = sourcePlayer;
            TargetPlayer = targetPlayer;
            NumTroops = numTroops;
        }
    }
}
