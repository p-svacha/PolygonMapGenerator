using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ColorManager : MonoBehaviour
    {
        [Header("Map Colors")]
        public Color WaterColor;
        public Color InactiveDistrictColor;
        public Color SelectedDistrictColor;

        [Header("Map Overlays")]
        public Color VeryHighImpactColor;
        public Color HighImpactColor;
        public Color MediumImpactColor;
        public Color LowImpactColor;
        public Color NoImpactColor;
        public Color NegativeImpactColor;
        public List<Color> LegendColors;

        [Header("UI")]
        public Color UiMain;
        public Color UiMainLighter1;
        public Color UiMainLighter2;
        public Color UiInteractable;
        public Color UiInteractableDisabled;
        public Color UiText;

        public static ColorManager Singleton
        {
            get
            {
                return GameObject.Find("ColorManager").GetComponent<ColorManager>();
            }
        }
    }
}
