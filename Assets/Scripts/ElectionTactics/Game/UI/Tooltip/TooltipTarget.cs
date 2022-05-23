using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TooltipType Type;
        public string Title;
        public string Text;

        [HideInInspector] public bool IsFocussed;
        private float Delay = 1f;
        [HideInInspector] public float CurrentDelay;

        [HideInInspector] public Tooltip Tooltip;

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
            Tooltip = Instantiate(PrefabManager.Singleton.Tooltip, GameObject.Find("UiOverlays").transform);
            Tooltip.Initialize(Type, Title, Text);
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
