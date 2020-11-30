using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictAttribute : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Text MainText;
        public GameObject Tooltip;
        public Text TooltipText;

        public bool HasTooltip;

        public void Init(string text, bool hasTooltip = false, string tooltipText = "")
        {
            MainText.text = text;
            HasTooltip = hasTooltip;
            TooltipText.text = tooltipText;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HasTooltip) Tooltip.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HasTooltip) Tooltip.SetActive(false);
        }
    }
}
