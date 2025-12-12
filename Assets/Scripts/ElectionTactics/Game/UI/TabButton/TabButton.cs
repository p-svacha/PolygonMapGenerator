using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class TabButton : MonoBehaviour
    {
        public Image Background;
        public Button Button;

        void Start()
        {
            ColorBlock colors = Button.colors;
            colors.highlightedColor = ColorManager.Instance.UiInteractable;
            Button.colors = colors;
        }

        public void SetSelected(bool active)
        {
            Button.GetComponent<Image>().color = active ? ColorManager.Instance.UiInteractable : ColorManager.Instance.UiText;
        }
    }
}
