using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Title;
        public string Text;

        public bool IsFocussed;
        private float Delay = 1f;
        public float CurrentDelay;

        public Tooltip Tooltip;

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsFocussed = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void Update()
        {
            if(IsFocussed && Tooltip == null)
            {
                if(CurrentDelay < Delay) CurrentDelay += Time.deltaTime;
                else ShowTooltip();
            }
        }

        private void ShowTooltip()
        {
            Tooltip = Instantiate(PrefabManager.Singleton.Tooltip, GameObject.Find("Overlays").transform);
            Tooltip.Initialize(Title, Text);
        }

        private void HideTooltip()
        {
            IsFocussed = false;
            CurrentDelay = 0;
            if (Tooltip != null)
            {
                Destroy(Tooltip.gameObject);
                Tooltip = null;
            }
        }
    }
}
