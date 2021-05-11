using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class Player
    {
        public ParriskGame Game;
        public string Name;
        public Color Color;
        public bool IsNpc;

        public List<Territory> Territories = new List<Territory>();

        public Player(ParriskGame game, string name, Color color, bool isNpc)
        {
            Game = game;
            Name = name;
            Color = color;
            IsNpc = isNpc;
        }

        #region AI

        public void DoNpcTroopsMovements()
        {
            foreach(Territory t in Territories)
            {
                int remainingTroops = t.Troops;
                List<Territory> neighbourTerritories = t.Region.Neighbours.Select(x => Game.Territories[x]).ToList();

                while(remainingTroops > 0 && neighbourTerritories.Count > 0)
                {
                    Territory moveTerritory = neighbourTerritories[Random.Range(0, neighbourTerritories.Count)];
                    int moveTroops = Random.Range(0, remainingTroops + 1);
                    neighbourTerritories.Remove(moveTerritory);
                    if(moveTroops > 0)
                    {
                        Game.AddArmy(t, moveTerritory, moveTroops);
                        remainingTroops -= moveTroops;
                    }
                }
            }
        }

        #endregion
    }
}
