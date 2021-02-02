using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Parliament : MonoBehaviour
    {
        public UI_PartyList PartyList;
        public UI_ModifierSliderContainer ModifierSliderContainer;

        public GameObject CurrentElectionContainer;
        public Text CurrentElectionTitle;
        public Text CurrentElectionMarginText;
        public Image LastElectionWinnerKnob;
        public Text CurrentElectionSeatsText;
        public Image CurrentElectionSeatsIcon;
        public WindowGraph CurrentElectionGraph;


        public void Init(List<Party> parties)
        {
            PartyList.Init(parties);
        }

    }
}
