using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class LegendEntry : MonoBehaviour
    {
        public Image ColorKnob;
        public Text Label;

        public void Init(Color c, string text)
        {
            ColorKnob.color = c;
            Label.text = text;
            Canvas.ForceUpdateCanvases();
            GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        }
    }
}
