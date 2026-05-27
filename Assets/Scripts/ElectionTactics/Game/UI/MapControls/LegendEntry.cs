using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class LegendEntry : MonoBehaviour
    {
        public Image ColorKnob;
        public TextMeshProUGUI Label;

        public void Init(Color c, string text)
        {
            ColorKnob.color = c;
            Label.text = text;
            Canvas.ForceUpdateCanvases();
            GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        }
    }
}
