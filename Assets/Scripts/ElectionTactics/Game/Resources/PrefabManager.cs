using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class PrefabManager : MonoBehaviour
    {
        public Tooltip Tooltip;
        public Font GraphFont;

        public static PrefabManager Singleton
        {
            get
            {
                return GameObject.Find("PrefabManager").GetComponent<PrefabManager>();
            }
        }
    }
}
