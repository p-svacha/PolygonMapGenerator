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

        public Sprite GetModifierIcon(ModifierType type)
        {
            if (type == ModifierType.Positive) return ModifierPositiveIcon;
            if (type == ModifierType.Negative) return Singleton.ModifierNegativeIcon;
            if (type == ModifierType.Exclusion) return Singleton.ModifierExclusionIcon;
            throw new System.Exception("Modifier image not found for type " + type.ToString());
        } 

        public static IconManager Singleton
        {
            get
            {
                return GameObject.Find("IconManager").GetComponent<IconManager>();
            }
        }
    }
}
