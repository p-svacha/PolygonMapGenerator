using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PopulationGrowthIndicator : MonoBehaviour
    {
        [Header("Elements")]
        public Image IndicatorIcon;

        public void Init(float growthRate)
        {
            string iconPath = "ElectionTactics/Icons/PopulationGrowth_";

            if (growthRate < -3.5f) iconPath += "Decrease3";
            else if (growthRate < -1.5f) iconPath += "Decrease2";
            else if (growthRate < -0.5f) iconPath += "Decrease1";
            else if (growthRate < 0.5f) iconPath += "Stagnant";
            else if (growthRate < 1.5f) iconPath += "Increase1";
            else if (growthRate < 3.5f) iconPath += "Increase2";
            else iconPath += "Increase3";

            IndicatorIcon.sprite = ResourceManager.LoadSprite(iconPath);
        }
    }
}
