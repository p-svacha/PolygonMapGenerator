using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionTactics
{
    public class UI_PartyListElement : MonoBehaviour
    {
        public Image Background;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ValueText;
        public Party Party;

        public void Init(Party p, string value, bool useAcronym)
        {
            Party = p;
            NameText.text = useAcronym ? p.Acronym : p.Name;
            NameText.color = p.Color;
            ValueText.text = value;
            Background.color = ColorManager.Instance.UiMainLighter1;
        }

        public void UpdateValue(string value)
        {
            ValueText.text = value;
        }
    }
}
