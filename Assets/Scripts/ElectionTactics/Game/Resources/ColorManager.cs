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

        /// <summary>
        /// Returns the fitting color for a popularity impact value.
        /// </summary>
        public Color GetImpactColor(int value)
        {
            if (value < 0) return ColorManager.Singleton.NegativeImpactColor;
            if (value == 0) return ColorManager.Singleton.NoImpactColor;
            if (value <= 3) return ColorManager.Singleton.LowImpactColor;
            if (value <= 6) return ColorManager.Singleton.MediumImpactColor;
            if (value <= 9) return ColorManager.Singleton.HighImpactColor;
            else return ColorManager.Singleton.VeryHighImpactColor;
        }

        public Color Lighter(Color c)
        {
            return new Color(c.r + 0.5f * (1f - c.r), c.g + 0.5f * (1f - c.g), c.b + 0.5f * (1f - c.b));
        }

        public static ColorManager Singleton
        {
            get
            {
                return GameObject.Find("ColorManager").GetComponent<ColorManager>();
            }
        }
    }
}
