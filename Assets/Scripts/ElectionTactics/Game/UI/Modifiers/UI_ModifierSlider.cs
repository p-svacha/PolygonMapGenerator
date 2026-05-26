using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ModifierSlider : MonoBehaviour
    {
        public RectTransform Container;
        public Image TypeImage;
        public Text PartyText;
        public TextMeshProUGUI DescriptionText;

        public void Init(Modifier m)
        {
            if (m.Type == ModifierType.Positive) TypeImage.sprite = IconManager.Singleton.ModifierPositiveIcon;
            else if (m.Type == ModifierType.Negative) TypeImage.sprite = IconManager.Singleton.ModifierNegativeIcon;
            else if (m.Type == ModifierType.Exclusion) TypeImage.sprite = IconManager.Singleton.ModifierExclusionIcon;
            else throw new System.Exception("Modifier Image not found for type " + m.Type.ToString());

            PartyText.text = m.Party.Acronym;
            PartyText.color = m.Party.Color;

            if (m.Type == ModifierType.Exclusion) DescriptionText.text = $"<b>EXCLUDED</b> {m.Description}";
            if (m.Type == ModifierType.Positive) DescriptionText.text = "<b>+" + m.Value + "</b> " + m.Description;
            if (m.Type == ModifierType.Negative) DescriptionText.text = "<b>-" + m.Value + "</b> " + m.Description;
        }
    }
}
