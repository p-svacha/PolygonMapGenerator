using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

namespace ElectionTactics
{
    public class UI_PartyListElement : MonoBehaviour
    {
        public Image Background;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ValueText;
        public Party Party;
        public UI_SeatNumber SeatNumber;
        public GameObject EliminationOverlay;
        public Image PlayerControlledIndicator;
        public TooltipTarget TooltipTarget;

        private bool IsUsingSeatIcons;

        public void Init(Party p, string value, bool useAcronym, bool useSeatIcons = false)
        {
            IsUsingSeatIcons = useSeatIcons;

            Party = p;
            NameText.text = useAcronym ? p.Acronym : p.Name;
            NameText.color = p.Color;

            // Value
            if (useSeatIcons)
            {
                SeatNumber.gameObject.SetActive(true);
                if (ValueText != null) ValueText.gameObject.SetActive(false);

                SeatNumber.InitPartySeats(p, value);
            }
            else
            {
                if (SeatNumber != null) SeatNumber.gameObject.SetActive(false);
                ValueText.gameObject.SetActive(true);

                ValueText.text = value;
            }


            Background.color = ColorManager.Instance.UiMainLighter1;
            if (EliminationOverlay != null) EliminationOverlay.SetActive(p.IsEliminated);
            if (PlayerControlledIndicator != null)
            {
                // PlayerControlledIndicator.color = p.Color;
                PlayerControlledIndicator.gameObject.SetActive(p.IsLocalPlayer);
            }
            RefreshTooltipContent();
        }

        public void SetSeats(int value)
        {
            if (SeatNumber != null)
            {
                SeatNumber.SetValue(value);
                SeatNumber.gameObject.SetActive(true);
            }
        }

        public void UpdateValue(string value)
        {
            if (IsUsingSeatIcons) SeatNumber.SetValue(value);
            else ValueText.text = value;
            RefreshTooltipContent();
        }

        private void RefreshTooltipContent()
        {
            if (TooltipTarget == null) return;

            string title = Party.Name;
            string description = "";
            if (ElectionTacticsGame.Instance.GameSettings.GameMode == GameModeDefOf.Classic)
            {
                if (ValueText.text == "") return; // safeguard

                int numElectionsWon = int.Parse(ValueText.text);
                description = $"{numElectionsWon} {"election".Pluralize(numElectionsWon)} won.";
            }
            else if (ElectionTacticsGame.Instance.GameSettings.GameMode == GameModeDefOf.BattleRoyale)
            {
                description = $"Legitimacy: {ValueText.text}.";
                if (Party.IsEliminated) description += " Eliminated.";
            }

            TooltipTarget.Init(title, description, Party.TextColor);
        }

        public Vector3 GetSeatInfoScreenPosition()
        {
            return SeatNumber.transform.position;
        }
    }
}
