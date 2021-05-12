using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class TerritoryBattle : Battle
    {
        public TerritoryBattle(VisualArmy armyPrefab, int turn, List<Army> armies) : base(armyPrefab, turn, armies)
        {
            Type = BattleType.TerritoryBattle;
            SupportOnly = armies.All(x => x.Type == ArmyType.Support);

            // Check if all armies have the same target territory
            Army targetArmy = armies.FirstOrDefault();
            if (targetArmy != null && !armies.All(x => x.TargetTerritory == targetArmy.TargetTerritory)) throw new System.Exception("Not all armies have the same target territory!");

            foreach (Army army in Armies)
            {
                Vector2 borderCenter;
                float sourceAngle;
                float targetAngle;
                float approachDistance = ParriskGame.ArmyApproachDistance;
                if (army.IsWaterArmy)
                {
                    WaterConnection wc = army.SourceTerritory.Region.GetWaterConnectionTo(army.TargetTerritory.Region);
                    System.Tuple<Vector2, float> center = new System.Tuple<Vector2, float>(wc.Center, wc.FromRegion == army.SourceTerritory.Region ? wc.Angle - 90 : wc.Angle + 90);
                    borderCenter = center.Item1;
                    sourceAngle = center.Item2;
                    targetAngle = center.Item2 + 180;
                    approachDistance = wc.Length / 2f;
                }
                else
                {
                    System.Tuple<Vector2, float> center = army.SourceTerritory.Region.GetBorderCenterPositionTo(army.TargetTerritory.Region);
                    borderCenter = center.Item1;
                    sourceAngle = center.Item2;
                    targetAngle = center.Item2 + 180;
                }

                VisualArmy visualArmy = GameObject.Instantiate(armyPrefab);
                float xSource = Mathf.Sin(Mathf.Deg2Rad * sourceAngle) * approachDistance;
                float ySource = Mathf.Cos(Mathf.Deg2Rad * sourceAngle) * approachDistance;
                Vector2 sourcePos = borderCenter + new Vector2(xSource, ySource);
                float xTarget = Mathf.Sin(Mathf.Deg2Rad * targetAngle) * approachDistance;
                float yTarget = Mathf.Cos(Mathf.Deg2Rad * targetAngle) * approachDistance;
                Vector2 targetPos = borderCenter + new Vector2(xTarget, yTarget);
                visualArmy.Init(army, sourcePos, targetPos, ParriskGame.ArmyApproachTime * 2);
            }
        }

        public void AddArmyFromBorderBattle(VisualArmy armyPrefab, Army army)
        {
            Armies.Add(army);
            SupportOnly = Armies.All(x => x.Type == ArmyType.Support);

            if (!Territories.Contains(army.TargetTerritory)) Territories.Add(army.TargetTerritory);
            if (!Players.Contains(army.SourcePlayer)) Players.Add(army.SourcePlayer);
            if (!Players.Contains(army.TargetPlayer)) Players.Add(army.TargetPlayer);


            // Visual
            Vector2 borderCenter;
            float targetAngle;
            float approachDistance = ParriskGame.ArmyApproachDistance;
            if (army.IsWaterArmy)
            {
                WaterConnection wc = army.SourceTerritory.Region.GetWaterConnectionTo(army.TargetTerritory.Region);
                System.Tuple<Vector2, float> center = new System.Tuple<Vector2, float>(wc.Center, wc.FromRegion == army.SourceTerritory.Region ? wc.Angle - 90 : wc.Angle + 90);
                borderCenter = center.Item1;
                targetAngle = center.Item2 + 180;
                approachDistance = wc.Length / 2f;
            }
            else
            {
                System.Tuple<Vector2, float> center = army.SourceTerritory.Region.GetBorderCenterPositionTo(army.TargetTerritory.Region);
                borderCenter = center.Item1;
                targetAngle = center.Item2 + 180;
            }

            VisualArmy visualArmy = GameObject.Instantiate(armyPrefab);
            Vector2 sourcePos = borderCenter;
            float xTarget = Mathf.Sin(Mathf.Deg2Rad * targetAngle) * approachDistance;
            float yTarget = Mathf.Cos(Mathf.Deg2Rad * targetAngle) * approachDistance;
            Vector2 targetPos = borderCenter + new Vector2(xTarget, yTarget);
            visualArmy.Init(army, sourcePos, targetPos, ParriskGame.ArmyApproachTime);
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
            // Add troops that are stationed in the territory (only if territory belongs to someone)
            if (Territories[0].Player != null)
            {
                if (!remainingTroops.ContainsKey(Territories[0].Player)) remainingTroops.Add(Territories[0].Player, Territories[0].Troops);
                else remainingTroops[Territories[0].Player] += Territories[0].Troops;
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
                if (Territories[0].Player != null && p == Territories[0].Player) totalStrength += (Territories[0].Troops * ParriskGame.BaseDefenseStrength);
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
            WinnerTroopsRemaining = remainingTroops[Winner];
        }
    }
}
