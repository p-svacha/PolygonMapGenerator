using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public class ArmyArrow : MonoBehaviour
    {
        public Army TroopMovement;
        public Vector2 MidPoint;
        public TextMesh Label;

        public void Init(Army tm, TextMesh label, Vector2 midPoint)
        {
            TroopMovement = tm;
            Label = label;
            MidPoint = midPoint;
            gameObject.AddComponent<MeshCollider>();
        }
    }
}
