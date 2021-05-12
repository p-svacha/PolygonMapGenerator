using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public class Territory
    {
        public Region Region;
        public Player Player;

        public string Name;
        public int Troops;
        public int UnplannedTroops; // Amount of troops that can still be moved during planning phase
        public int DistributedTroops; // Amount of troops that will be added at the end of distribution phase

        private const int TroopsLabelSize = 80;
        public TextMesh TroopsLabel;
        

        public Territory(Region region, string name)
        {
            Region = region;
            Name = name;

            TroopsLabel = MeshGenerator.DrawTextMesh(Region.CenterPoi, 0.01f, "", TroopsLabelSize);
        }
        

        public void AddTroops(int numTroops)
        {
            Troops += numTroops;
            UpdateLabel();
        }

        public void SetTroops(int numTroops)
        {
            Troops = numTroops;
            UpdateLabel();
        }

        public void ResetPlannedTroops()
        {
            UnplannedTroops = Troops;
            UpdateLabel();
        }

        public void PlanTroops(int numTroops)
        {
            UnplannedTroops -= numTroops;
            UpdateLabel();
        }

        public void DistributeTroop()
        {
            DistributedTroops++;
            UpdateLabel();
        }

        public void UndistributeTroop()
        {
            DistributedTroops--;
            UpdateLabel();
        }

        public void LockInDistributedTroops()
        {
            Troops += DistributedTroops;
            DistributedTroops = 0;
            UpdateLabel();
        }

        public void EndPlanningPhase()
        {
            Troops = UnplannedTroops;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (Player == null) TroopsLabel.text = "";
            if (DistributedTroops != 0) TroopsLabel.text = Troops.ToString() + "+" + DistributedTroops.ToString();
            else if (Troops == UnplannedTroops) TroopsLabel.text = Troops.ToString();
            else TroopsLabel.text = UnplannedTroops + "/" + Troops;
        }
    }
}
