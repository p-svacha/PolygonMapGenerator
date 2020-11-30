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
        }

        public void UpdateValues()
        {
            SeatsText.text = Party.Seats.ToString();
        }
    }
}
