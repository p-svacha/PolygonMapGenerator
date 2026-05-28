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

        public void Init(LegendEntryData entry)
        {
            ColorKnob.color = entry.Color;
            Label.text = entry.Label;
            Canvas.ForceUpdateCanvases();
            GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
        }
    }
}
