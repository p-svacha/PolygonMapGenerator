using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_StandingsElement : MonoBehaviour
    {
        public Text PartyText;
        public Text PointsText;

        public void Init(Party p)
        {
            PartyText.text = p.Acronym;
            PartyText.color = p.Color;
            PointsText.text = p.GamePoints.ToString();
            PointsText.color = p.Color;
        }
    }
}
