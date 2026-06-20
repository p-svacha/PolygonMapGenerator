using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PostGameScreen : MonoBehaviour
    {
        [Header("Elements")]
        public GameObject Container;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI LeftStatsText;
        public TextMeshProUGUI RightStatsText;
        public Button QuitToDesktopButton;

        private static string BLUE = "#77aadd";

        void Start()
        {
            //QuitToDesktopButton.onClick.AddListener(() => Application.Quit());
            QuitToDesktopButton.onClick.AddListener(BackToMenuButton_OnClick);
        }

        private void BackToMenuButton_OnClick()
        {
            AudioManager.PlayStandardClickSound();
            Container.gameObject.SetActive(false);
            ElectionTacticsGame.Instance.MenuNavigator.SwitchToMainMenuScreen();
        }

        public void Init(ElectionTacticsGame game)
        {
            Container.SetActive(true);

            if (game.WinnerParty == game.LocalPlayerParty)
            {
                TitleText.text = "You have won the game!\n\nCongratulations!";
                AudioManager.PlaySound(AudioManager.Instance.WinGame);
            }
            else
            {
                TitleText.text = $"You have lost the game.\n\nBetter luck next time.";
                AudioManager.PlaySound(AudioManager.Instance.LoseGame);
            }

            // Left Stats
            int endYear = game.Year;
            int totalSeats = game.GetAllElectionResults().Sum(r => r.GetTotalSeats());
            int numParites = game.Parties.Count;
            int numDistricts = game.ActiveDistricts.Count;
            string leftStats = $"<b><color={BLUE}>Game Stats</color></b>\n\nEnd Year: <color={BLUE}>{endYear}</color>\n\nTotal Seats Distributed: <color={BLUE}>{totalSeats}</color>\n\nParties: <color={BLUE}>{numParites}</color>\n\nDistricts: <color={BLUE}>{numDistricts}</color>";
            LeftStatsText.text = leftStats;

            // Right Stats
            int electionsWon = game.LocalPlayerParty.TotalElectionsWon;
            int totalSeatsWon = game.LocalPlayerParty.TotalSeatsWon;
            int totalPopularity = game.ActiveDistricts.Sum(d => d.GetPartyPopularity(game.LocalPlayerParty));
            int avgPopularity = (int)(totalPopularity / numDistricts);
            int maxPopularity = game.ActiveDistricts.Max(d => d.GetPartyPopularity(game.LocalPlayerParty));
            int totalPPSpent = game.LocalPlayerParty.Policies.Sum(p => p.Value);
            string rightStats = $"<b><color={BLUE}>Party Stats</color></b>\n\nElections Won: <color={BLUE}>{electionsWon}</color>\n\nTotal Seats Won: <color={BLUE}>{totalSeatsWon}</color>\n\nTotal Popularity: <color={BLUE}>{totalPopularity} (avg: {avgPopularity}, max: {maxPopularity})</color>\n\nPolicy Points Spent: <color={BLUE}>{totalPPSpent}</color>";
            RightStatsText.text = rightStats;
        }
    }
}
