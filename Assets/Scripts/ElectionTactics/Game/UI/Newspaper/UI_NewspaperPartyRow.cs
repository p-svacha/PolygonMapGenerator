using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_NewspaperPartyRow : MonoBehaviour
    {
        [Header("Elements")]
        public Image Knob;
        public TextMeshProUGUI PartyText;
        public TextMeshProUGUI SeatsText;

        public void Init(Party p, int seats)
        {
            Knob.color = p.Color;
            PartyText.text = p.Acronym;
            SeatsText.text = seats.ToString();
        }
    }
}
