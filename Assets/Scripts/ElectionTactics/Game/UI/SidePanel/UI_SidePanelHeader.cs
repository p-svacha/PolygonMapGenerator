using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionTactics
{
    public class UI_SidePanelHeader : UIElement
    {
        [Header("Elements")]
        public TextMeshProUGUI YearText;
        public TextMeshProUGUI ElectionCycleText;
        public TextMeshProUGUI PPText;
        public TextMeshProUGUI CPText;

        public void UpdateValues(ElectionTacticsGame game)
        {
            YearText.text = game.Year.ToString();
            ElectionCycleText.text = game.ElectionCycle.ToString();
            PPText.text = game.LocalPlayerParty.PolicyPoints.ToString();
            CPText.text = game.LocalPlayerParty.CampaignPoints.ToString();
        }
    }
}
