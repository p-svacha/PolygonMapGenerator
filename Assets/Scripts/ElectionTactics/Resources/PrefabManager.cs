using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class PrefabManager : MonoBehaviour
    {
        public Tooltip Tooltip;

        public static PrefabManager Prefabs
        {
            get
            {
                return GameObject.Find("PrefabManager").GetComponent<PrefabManager>();
            }
        }
    }
}
