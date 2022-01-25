using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ModifierListElement : MonoBehaviour
    {
        public Image TypeImage;
        public Text PartyText;
        public Text DescriptionText;
        public Text SourceText;
        public Text LengthText;

        public void Init(Modifier m)
        {
            TypeImage.sprite = IconManager.Singleton.GetModifierIcon(m.Type);

            PartyText.text = m.Party.Acronym;
            PartyText.color = m.Party.Color;
            DescriptionText.text = m.Description;
            SourceText.text = m.Source;
            LengthText.text = m.RemainingLength.ToString();
        }
    }
}
