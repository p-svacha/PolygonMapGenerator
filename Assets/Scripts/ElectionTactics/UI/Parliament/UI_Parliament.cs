using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Parliament : MonoBehaviour
    {
        [Header("Parliament")]
        public UI_PartyList ParliamentPartyList;

        [Header("Standings")]
        public List<Party> Parties;
        public GameObject StandingsContainer;
        public Dropdown StandingsDropdown;
        public UI_PartyList StandingsPartyList;
        private const int STANDINGS_ELECTIONS = 0;
        private const int STANDINGS_SEATS = 1;
        private const int STANDINGS_DISTRICTS = 2;
        private const int STANDINGS_VOTES = 3;

        [Header("Current District Election")]
        public UI_ModifierSliderContainer ModifierSliderContainer;
        public GameObject CurrentElectionContainer;
        public Text CurrentElectionTitle;
        public Text CurrentElectionMarginText;
        public Image LastElectionWinnerKnob;
        public Text CurrentElectionSeatsText;
        public Image CurrentElectionSeatsIcon;
        public WindowGraph CurrentElectionGraph;

        private void Start()
        {
            StandingsDropdown.onValueChanged.AddListener(UpdateList);
        }

        public void Init(ElectionTacticsGame game, List<Party> parties)
        {
            // Parlialment party list
            Parties = parties;
            ParliamentPartyList.Init(parties.OrderByDescending(x => x.Seats).ToList(), Parties.OrderByDescending(x => x.Seats).Select(x => x.Seats.ToString()).ToList(), dynamic: true);
            
            // Standings party list
            StandingsContainer.SetActive(true);
            if(game.Constitution.WinCondition.Type == WinConditionType.TotalElectionsWon) StandingsDropdown.value = STANDINGS_ELECTIONS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalSeatsWon) StandingsDropdown.value = STANDINGS_SEATS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalDistrictsWon) StandingsDropdown.value = STANDINGS_DISTRICTS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalVotes) StandingsDropdown.value = STANDINGS_VOTES;

            UpdateList(StandingsDropdown.value);
        }

        private void UpdateList(int value)
        {
            if(value == STANDINGS_ELECTIONS) StandingsPartyList.Init(Parties.OrderByDescending(x => x.TotalElectionsWon).ToList(), Parties.OrderByDescending(x => x.TotalElectionsWon).Select(x => x.TotalElectionsWon.ToString()).ToList(), dynamic: false);
            else if(value == STANDINGS_SEATS) StandingsPartyList.Init(Parties.OrderByDescending(x => x.TotalSeatsWon).ToList(), (Parties.OrderByDescending(x => x.TotalSeatsWon).Select(x => x.TotalSeatsWon.ToString()).ToList()), dynamic: false);
            else if(value == STANDINGS_DISTRICTS) StandingsPartyList.Init(Parties.OrderByDescending(x => x.TotalDistrictsWon).ToList(), Parties.OrderByDescending(x => x.TotalDistrictsWon).Select(x => x.TotalDistrictsWon.ToString()).ToList(), dynamic: false);
            else if(value == STANDINGS_VOTES) StandingsPartyList.Init(Parties.OrderByDescending(x => x.TotalVotes).ToList(), Parties.OrderByDescending(x => x.TotalVotes).Select(x => x.TotalVotes.ToString("N0")).ToList(), dynamic: false);
        }

    }
}
