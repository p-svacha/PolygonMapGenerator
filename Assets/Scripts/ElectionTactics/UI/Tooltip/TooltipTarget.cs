using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            IsFocussed = false;
            CurrentDelay = 0;
            if (Tooltip != null)
            {
                GameObject.Destroy(Tooltip.gameObject);
                Tooltip = null;
            }
        }

        private void Update()
        {
            if(IsFocussed && Tooltip == null)
            {
                if(CurrentDelay < Delay)
                {
                    CurrentDelay += Time.deltaTime;
                }
                else
                {
                    Tooltip = Instantiate(PrefabManager.Prefabs.Tooltip, GameObject.Find("Overlays").transform);
                    Tooltip.Title.text = Title;
                    Tooltip.Text.text = Text;
                    Vector2 mouse = Input.mousePosition;
                    float width = Mathf.Max(Tooltip.Title.preferredWidth, Tooltip.Text.preferredWidth);
                    if (mouse.x + width > Screen.width) Tooltip.transform.position = new Vector2(Screen.width - width, mouse.y);
                    else Tooltip.transform.position = mouse;
                }
            }    
        }
    }
}
