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
            if (m.Type == ModifierType.Positive) TypeImage.sprite = IconManager.Icons.ModifierPositiveIcon;
            else if (m.Type == ModifierType.Negative) TypeImage.sprite = IconManager.Icons.ModifierNegativeIcon;
            else if (m.Type == ModifierType.Exclusion) TypeImage.sprite = IconManager.Icons.ModifierExclusionIcon;
            else throw new System.Exception("Modifier Image not found for type " + m.Type.ToString());

            PartyText.text = m.Party.Acronym;
            PartyText.color = m.Party.Color;
            DescriptionText.text = m.Description;
            SourceText.text = m.Source;
            LengthText.text = m.RemainingLength.ToString();
        }
    }
}
