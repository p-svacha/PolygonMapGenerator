using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class BorderBattle : Battle
    {
        public bool IsWaterBattle;

        public Territory WinnerSourceTerritory;
        public Territory WinnerTargetTerritory;

        public BorderBattle(VisualArmy armyPrefab, int turn, List<Army> armies) : base(armyPrefab, turn, armies)
        {
            SupportOnly = armies.All(x => x.Type == ArmyType.Support);

            if (armies.Count != 2) throw new System.Exception("Border battles can only be fought with exactly 2 armies attack each other");
            Type = BattleType.BorderBattle;

            Vector2 fightPosition;
            float fightAngleArmy0;
            float fightAngleArmy1;
            float approachDistance = ParriskGame.ArmyApproachDistance;
            if (Armies[0].IsWaterArmy)
            {
                IsWaterBattle = true;
                WaterConnection wc = Armies[0].SourceTerritory.Region.GetWaterConnectionTo(Armies[0].TargetTerritory.Region);
                System.Tuple<Vector2, float> center = new System.Tuple<Vector2, float>(wc.Center, wc.FromRegion == Armies[0].SourceTerritory.Region ? wc.Angle - 90 : wc.Angle + 90);
                fightPosition = center.Item1;
                fightAngleArmy0 = center.Item2;
                fightAngleArmy1 = center.Item2 + 180;
                approachDistance = wc.Length / 2f;
            }
            else
            {
                IsWaterBattle = false;
                System.Tuple<Vector2, float> center = Armies[0].SourceTerritory.Region.GetBorderCenterPositionTo(Armies[0].TargetTerritory.Region);
                fightPosition = center.Item1;
                fightAngleArmy0 = center.Item2;
                fightAngleArmy1 = center.Item2 + 180;
            }

            VisualArmy army0 = GameObject.Instantiate(armyPrefab);
            float xSource0 = Mathf.Sin(Mathf.Deg2Rad * fightAngleArmy0) * approachDistance;
            float ySource0 = Mathf.Cos(Mathf.Deg2Rad * fightAngleArmy0) * approachDistance;
            Vector2 sourcePos0 = fightPosition + new Vector2(xSource0, ySource0);
            army0.Init(Armies[0], sourcePos0, fightPosition, ParriskGame.ArmyApproachTime);

            VisualArmy army1 = GameObject.Instantiate(armyPrefab);
            float xSource1 = Mathf.Sin(Mathf.Deg2Rad * fightAngleArmy1) * approachDistance;
            float ySource1 = Mathf.Cos(Mathf.Deg2Rad * fightAngleArmy1) * approachDistance;
            Vector2 sourcePos1 = fightPosition + new Vector2(xSource1, ySource1);
            army1.Init(Armies[1], sourcePos1, fightPosition, ParriskGame.ArmyApproachTime);
        }

        public override void ResolveBattle()
        {
            // Calculate amount of troops for each player
            Dictionary<Player, int> remainingTroops = new Dictionary<Player, int>();
            foreach (Army army in Armies)
            {
                if (!remainingTroops.ContainsKey(army.SourcePlayer)) remainingTroops.Add(army.SourcePlayer, army.NumTroops);
                else remainingTroops[army.SourcePlayer] += army.NumTroops;
            }
            List<Player> remainingPlayers = remainingTroops.Where(x => x.Value > 0).Select(x => x.Key).ToList();

            // Calculate strength of each player (weighted average of all armies of that player)
            Dictionary<Player, float> playerStrength = new Dictionary<Player, float>();
            foreach (Player p in remainingPlayers)
            {
                Casualties.Add(p, 0);
                List<Army> playerArmies = Armies.Where(x => x.SourcePlayer == p).ToList();
                int totalTroops = remainingTroops[p];
                float totalStrength = 0;
                foreach (Army army in playerArmies) totalStrength += (army.NumTroops * army.Strength);
                float strength = totalStrength / totalTroops;
                playerStrength.Add(p, strength);
                //Debug.Log("Player " + p.Name + " Strength: " + strength);
            }

            // Fight until only one player has armies left
            while (remainingPlayers.Count > 1)
            {
                float totalStrength = 0f;
                foreach (Player p in remainingPlayers) totalStrength += playerStrength[p];
                float rng = Random.Range(0, totalStrength);
                float tmp = 0f;
                int c = 0;
                Player winner = null;
                while (winner == null)
                {
                    tmp += playerStrength[remainingPlayers[c]];
                    if (tmp >= rng) winner = remainingPlayers[c];
                    c++;
                }

                foreach (Player p in remainingPlayers)
                {
                    if (p != winner)
                    {
                        remainingTroops[p]--;
                        Casualties[p]++;
                    }
                }
                remainingPlayers = remainingTroops.Where(x => x.Value > 0).Select(x => x.Key).ToList();
            }

            // Set post-battle values
            Winner = remainingPlayers[0];
            Losers = Players.Where(x => x != Winner).ToList();
            WinnerSourceTerritory = Armies.First(x => x.SourcePlayer == Winner).SourceTerritory;
            WinnerTargetTerritory = Armies.First(x => x.SourcePlayer == Winner).TargetTerritory;
            WinnerTroopsRemaining = remainingTroops[Winner];
        }
    }
}
