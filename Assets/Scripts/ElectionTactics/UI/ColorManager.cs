using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ColorManager : MonoBehaviour
    {
        [Header("Map Colors")]
        public Color WaterColor;
        public Color ActiveDistrictColor;
        public Color InactiveDistrictColor;
        public Color SelectedDistrictColor;

        [Header("Map Overlays")]
        public Color HighImpactColor;
        public Color MediumImpactColor;
        public Color LowImpactColor;
        public Color NoImpactColor;
        public List<Color> LegendColors;

        [Header("UI")]
        public Color UiHeaderColor;
        public Color UiMainColor;
        public Color UiMainColorLighter1;
        public Color UiMainColorLighter2;
        public Color UiSpecialColor;
        public Color TextColor;

        public static ColorManager Colors
        {
            get
            {
                return GameObject.Find("ColorManager").GetComponent<ColorManager>();
            }
        }
    }
}
