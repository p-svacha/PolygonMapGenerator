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
        public bool IsInitialized;
        private float Delay = 1f;
        private int MouseOffset = 5;
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
                    Tooltip.transform.position = Input.mousePosition + new Vector3(MouseOffset, -MouseOffset, 0);
                    float width = Mathf.Max(Tooltip.Title.preferredWidth, Tooltip.Text.preferredWidth);
                    float height = Tooltip.GetComponent<RectTransform>().rect.height;
                    if (Tooltip.transform.position.x + width > Screen.width) Tooltip.transform.position = new Vector2(Screen.width - width, Tooltip.transform.position.y);
                    if (Tooltip.transform.position.y - height < 0) Tooltip.transform.position = new Vector2(Tooltip.transform.position.x, height);
                    Canvas.ForceUpdateCanvases();
                    Tooltip.gameObject.SetActive(false);
                    IsInitialized = false;
                }
            }
            else if(IsFocussed && Tooltip != null && !IsInitialized) // Do not question this code, else it might stop working
            {
                    Tooltip.gameObject.SetActive(true);
                    Canvas.ForceUpdateCanvases();
                    Tooltip.gameObject.SetActive(false);
                    Tooltip.gameObject.SetActive(true);
                    IsInitialized = true;
            }
        }
    }
}
