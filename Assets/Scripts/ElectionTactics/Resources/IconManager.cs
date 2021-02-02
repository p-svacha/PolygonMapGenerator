using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class IconManager : MonoBehaviour
    {
        public Sprite ModifierNegativeIcon;
        public Sprite ModifierPositiveIcon;
        public Sprite ModifierExclusionIcon;

        public static IconManager Icons
        {
            get
            {
                return GameObject.Find("IconManager").GetComponent<IconManager>();
            }
        }
    }
}
