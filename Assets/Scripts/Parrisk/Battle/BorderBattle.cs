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

            List<Vector2> army0Path = new List<Vector2>();
            List<Vector2> army1Path = new List<Vector2>();
            army0Path.Add(Armies[0].SourceTerritory.Region.CenterPoi);
            army1Path.Add(Armies[1].SourceTerritory.Region.CenterPoi);
            if (Armies[0].IsWaterArmy)
            {
                IsWaterBattle = true;
                WaterConnection wc = Armies[0].SourceTerritory.Region.GetWaterConnectionTo(Armies[0].TargetTerritory.Region);
                System.Tuple<Vector2, float> center = new System.Tuple<Vector2, float>(wc.Center, wc.FromRegion == Armies[0].SourceTerritory.Region ? wc.Angle - 90 : wc.Angle + 90);
                Vector2 fightPosition = center.Item1;
                float fightAngleArmy0 = center.Item2;
                float fightAngleArmy1 = center.Item2 + 180;
                float approachDistance = wc.Length / 2f;

                float xSource0 = Mathf.Sin(Mathf.Deg2Rad * fightAngleArmy0) * approachDistance;
                float ySource0 = Mathf.Cos(Mathf.Deg2Rad * fightAngleArmy0) * approachDistance;
                Vector2 coastPos0 = fightPosition + new Vector2(xSource0, ySource0);
                army0Path.Add(coastPos0);
                army0Path.Add(fightPosition);

                float xSource1 = Mathf.Sin(Mathf.Deg2Rad * fightAngleArmy1) * approachDistance;
                float ySource1 = Mathf.Cos(Mathf.Deg2Rad * fightAngleArmy1) * approachDistance;
                Vector2 coastPos1 = fightPosition + new Vector2(xSource1, ySource1);
                army1Path.Add(coastPos1);
                army1Path.Add(fightPosition);
            }
            else
            {
                IsWaterBattle = false;
                System.Tuple<Vector2, float> center = Armies[0].SourceTerritory.Region.GetBorderCenterPositionTo(Armies[0].TargetTerritory.Region);
                Vector2 fightPosition = center.Item1;
                army0Path.Add(fightPosition);
                army1Path.Add(fightPosition);
            }

            VisualArmy army0 = GameObject.Instantiate(armyPrefab);
            army0.Init(Armies[0], army0Path, ParriskGame.ArmyApproachTime);

            VisualArmy army1 = GameObject.Instantiate(armyPrefab);
            army1.Init(Armies[1], army1Path, ParriskGame.ArmyApproachTime);
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
