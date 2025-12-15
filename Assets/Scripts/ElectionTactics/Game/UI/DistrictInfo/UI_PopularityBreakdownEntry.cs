using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PopularityBreakdownEntry : MonoBehaviour
    {
        [Header("Elements")]
        public Text LabelText;
        public Text ValueText;

        public void Init(string label, int value)
        {
            LabelText.text = label;
            ValueText.text = value >= 0 ? "+" + value : value.ToString();
        }
    }
}
