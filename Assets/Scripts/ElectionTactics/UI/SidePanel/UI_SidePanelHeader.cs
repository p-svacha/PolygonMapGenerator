using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_SidePanelHeader : UIElement
    {
        [Header("Elements")]
        public Text YearText;
        public Text PPText;
        public Text CPText;

        public void UpdateValues(ElectionTacticsGame game)
        {
            YearText.text = game.Year.ToString();
            PPText.text = game.PlayerParty.PolicyPoints.ToString();
            CPText.text = game.PlayerParty.CampaignPoints.ToString();
        }
    }
}
