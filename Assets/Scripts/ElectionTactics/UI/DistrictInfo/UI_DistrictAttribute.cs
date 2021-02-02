using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictAttribute : MonoBehaviour
    {
        public Text MainText;
        public HorizontalLayoutGroup HLG;
        public TooltipTarget TooltipTarget;

        public void Init(string displayText, string tooltipTitle, string tooltipDescription)
        {
            MainText.text = displayText;
            TooltipTarget = GetComponent<TooltipTarget>();
            TooltipTarget.Title = tooltipTitle;
            TooltipTarget.Text = tooltipDescription;
        }
    }
}
