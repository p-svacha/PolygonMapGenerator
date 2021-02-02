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
        public Text SeatsText;
        public Party Party;

        public void Init(Party p)
        {
            Party = p;
            NameText.text = p.Name;
            NameText.color = p.Color;
            SeatsText.text = p.Seats.ToString();
            Background.color = ColorManager.Colors.UiMainColorLighter1;
        }

        public void UpdateValues()
        {
            SeatsText.text = Party.Seats.ToString();
        }
    }
}
