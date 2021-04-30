using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public class TroopMovementArrow : MonoBehaviour
    {
        public TroopMovement TroopMovement;
        public Vector2 MidPoint;
        public TextMesh Label;

        public void Init(TroopMovement tm, TextMesh label, Vector2 midPoint)
        {
            TroopMovement = tm;
            Label = label;
            MidPoint = midPoint;
            gameObject.AddComponent<MeshCollider>();
        }
    }
}
