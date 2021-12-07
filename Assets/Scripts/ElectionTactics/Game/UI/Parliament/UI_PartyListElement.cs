using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PartyListElement : MonoBehaviour
    {
        public Image Background;
        public Text NameText;
        public Text ValueText;
        public Party Party;

        public void Init(Party p, string value)
        {
            Party = p;
            NameText.text = p.Name;
            NameText.color = p.Color;
            ValueText.text = value;
            Background.color = ColorManager.Colors.UiMainColorLighter1;
        }

        public void UpdateValue(string value)
        {
            ValueText.text = value;
        }
    }
}
