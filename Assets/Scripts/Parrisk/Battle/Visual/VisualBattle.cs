using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public class VisualBattle : MonoBehaviour
    {
        public Battle Battle;

        public Vector2 Position;
        public List<VisualArmy> Army = new List<VisualArmy>();

        public void Init(Battle battle)
        {
            Battle = battle;
        }
    }
}
