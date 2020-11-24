using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ColorManager : MonoBehaviour
    {
        // Map
        public Color WaterColor;
        public Color ActiveDistrictColor;
        public Color InactiveDistrictColor;
        public Color SelectedDistrictColor;

        // UI
        public Color HeaderColor;
        public Color PanelColor;
        public Color ListElementColor;
        public Color HighlightedListElementColor;

        public static ColorManager Colors
        {
            get
            {
                return GameObject.Find("ColorManager").GetComponent<ColorManager>();
            }
        }
    }
}
