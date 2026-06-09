using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ModifierListElement : MonoBehaviour
    {
        public Image TypeImage;
        public TextMeshProUGUI PartyText;
        public TextMeshProUGUI ValueText;
        public TextMeshProUGUI DescriptionText;
        public TextMeshProUGUI RemainingDurationText;
        public TooltipTarget TooltipTarget;

        public void Init(Modifier m)
        {
            TypeImage.sprite = IconManager.Singleton.GetModifierIcon(m.Type);

            PartyText.text = m.Party.Acronym;
            PartyText.color = m.Party.Color;
            ValueText.text = m.Type == ModifierType.Positive ? "+" + m.Value.ToString() : "-" + m.Value.ToString();
            if (m.Value == 0) ValueText.text = "";
            DescriptionText.text = m.Description;
            if (m.Type == ModifierType.Exclusion) DescriptionText.text = $"<b>EXCLUDED</b> {DescriptionText.text}";
            RemainingDurationText.text = $"{m.RemainingLength} {"cycle".Pluralize(m.RemainingLength)} remaining";

            TooltipTarget.Init("", $"Source: {m.Source}");
        }
    }
}
