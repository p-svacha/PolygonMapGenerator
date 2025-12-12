using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        public TextMeshProUGUI StandingsTitle;
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
            StandingsDropdown.onValueChanged.AddListener(UpdateStandings);
        }

        /// <summary>
        /// Gets called every time this tab is selected.
        /// </summary>
        public void Init(ElectionTacticsGame game, List<Party> parties)
        {
            // Parlialment party list
            Parties = parties;
            Dictionary<Party, int> listValues = new Dictionary<Party, int>();
            foreach (Party p in parties.OrderByDescending(x => x.Seats)) listValues.Add(p, p.Seats);
            ParliamentPartyList.Init(listValues, dynamic: true);
            
            // Standings party list
            StandingsContainer.SetActive(true);
            if(game.Constitution == null) StandingsDropdown.value = STANDINGS_ELECTIONS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalElectionsWon) StandingsDropdown.value = STANDINGS_ELECTIONS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalSeatsWon) StandingsDropdown.value = STANDINGS_SEATS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalDistrictsWon) StandingsDropdown.value = STANDINGS_DISTRICTS;
            else if(game.Constitution.WinCondition.Type == WinConditionType.TotalVotes) StandingsDropdown.value = STANDINGS_VOTES;

            UpdateStandings(StandingsDropdown.value);
        }

        private void UpdateStandings(int value)
        {
            if (ElectionTacticsGame.Instance.GameSettings.GameMode == GameModeDefOf.BattleRoyale)
            {
                StandingsTitle.text = "Legitimacy";
                StandingsPartyList.Init(ElectionTacticsGame.Instance.GetCurrentStandings(), dynamic: false);
            }
            else if (ElectionTacticsGame.Instance.GameSettings.GameMode == GameModeDefOf.Classic)
            {
                StandingsTitle.text = "Elections Won";
                StandingsPartyList.Init(ElectionTacticsGame.Instance.GetCurrentStandings(), dynamic: false);
            }
            else
            {
                throw new System.Exception("Game mode not handled in settings");
            }

            /* Old logic showing standings based on a dropdown
            Dictionary<Party, int> listValues = new Dictionary<Party, int>();

            if (value == STANDINGS_ELECTIONS)
                foreach (Party p in Parties.OrderByDescending(x => x.TotalElectionsWon))
                    listValues.Add(p, p.TotalElectionsWon);

            if (value == STANDINGS_SEATS)
                foreach (Party p in Parties.OrderByDescending(x => x.TotalSeatsWon))
                    listValues.Add(p, p.Seats);

            if (value == STANDINGS_DISTRICTS)
                foreach (Party p in Parties.OrderByDescending(x => x.TotalDistrictsWon))
                    listValues.Add(p, p.TotalDistrictsWon);

            if (value == STANDINGS_VOTES)
                foreach (Party p in Parties.OrderByDescending(x => x.TotalVotes))
                    listValues.Add(p, p.TotalVotes);

            StandingsPartyList.Init(listValues, dynamic: false);
            */
        }

    }
}
