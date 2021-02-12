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

        public void SetSelected(bool active)
        {
            Background.color = active ? ColorManager.Colors.UiSpecialColor : ColorManager.Colors.UiHeaderColor;
        }
    }
}
