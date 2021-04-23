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

        public TextMesh TroopsLabel;
        

        public Territory(Region region, string name)
        {
            Region = region;
            Name = name;

            // Init label
            GameObject labelObject = new GameObject("Label");
            labelObject.AddComponent<MeshRenderer>();
            TroopsLabel = labelObject.AddComponent<TextMesh>();
            TroopsLabel.transform.position = new Vector3(Region.CenterPoi.x, 0.01f, Region.CenterPoi.y);
            TroopsLabel.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            TroopsLabel.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            TroopsLabel.color = Color.black;
            TroopsLabel.fontSize = 100;
            TroopsLabel.anchor = TextAnchor.MiddleCenter;
        }
        

        public void AddTroops(int troops)
        {
            Troops += troops;
            TroopsLabel.text = Troops.ToString();
        }
    }
}
