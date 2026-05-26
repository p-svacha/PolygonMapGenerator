using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictElectionGraph : MonoBehaviour
    {
        public Text YearText;
        public Button FirstYearButton;
        public Button PrevYearButton;
        public Button NextYearButton;
        public Button LastYearButton;
        public Toggle ShowSeatsToggle;
        public Toggle NonEliminatedToggle;
        public WindowGraph ElectionGraph;

        public List<DistrictElectionResult> ElectionResults;
        public int CurrentIndex;

        public void Init(List<DistrictElectionResult> results)
        {
            ElectionResults = results;
            CurrentIndex = results.Count - 1;
            DisplayGraph();
        }

        private void DisplayGraph(bool fullRefresh = true)
        {
            DistrictElectionResult result = ElectionResults[CurrentIndex];
            YearText.text = result.Year.ToString();
            if (result == null) return;

            bool showSeats = ShowSeatsToggle.isOn;

            List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
            foreach (KeyValuePair<Party, float> kvp in result.VoteShare)
            {
                if (NonEliminatedToggle.isOn && kvp.Key.IsEliminated) continue;

                List<Sprite> modifierIcons = new List<Sprite>();
                List<string> iconTooltipTitles = new List<string>();
                List<string> iconTooltipTexts = new List<string>();
                foreach (Modifier m in result.Modifiers.Where(x => x.Party == kvp.Key))
                {
                    modifierIcons.Add(IconManager.Singleton.GetModifierIcon(m.Type));
                    iconTooltipTitles.Add(m.Type.ToString());
                    iconTooltipTexts.Add(m.Description + "\n\nSource: " + m.Source);
                }

                string label = GlobalSettings.DebugMode ? kvp.Key.Acronym + "|" + result.PartyPopularities[kvp.Key] : kvp.Key.Acronym;
                float value = showSeats ? result.SeatsWon[kvp.Key] : kvp.Value;
                dataPoints.Add(new GraphDataPoint(label, value, kvp.Key.Color, modifierIcons, iconTooltipTitles, iconTooltipTexts));
            }

            if (showSeats)
            {
                int yMax = Mathf.Max(result.Seats, 1);
                float yStep = yMax <= 5 ? 1 : yMax <= 20 ? 5 : 10;
                if (fullRefresh) ElectionGraph.InitAnimatedBarGraph(dataPoints, yMax, yStep, 0.1f, Color.white, Color.grey, PrefabManager.Singleton.GraphFont, 0.25f, startAnimation: true);
                else ElectionGraph.UpdateAnimatedBarGraph(dataPoints, yMax, 0.25f);
            }
            else
            {
                int yMax = (((int)result.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;
                if (fullRefresh) ElectionGraph.InitAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, PrefabManager.Singleton.GraphFont, 0.25f, startAnimation: true);
                else ElectionGraph.UpdateAnimatedBarGraph(dataPoints, yMax, 0.25f);
            }

            // Hide eliminated toggle in classic mode
            NonEliminatedToggle.gameObject.SetActive(ElectionTacticsGame.Instance.IsBattleRoyale);
        }

        private void GoToFirstYear()
        {
            if (CurrentIndex == 0) return;
            CurrentIndex = 0;
            DisplayGraph(fullRefresh: false);
        }
        private void GoToPreviousYear()
        {
            if (CurrentIndex == 0) return;
            CurrentIndex--;
            DisplayGraph(fullRefresh: false);
        }
        private void GoToNextYear()
        {
            if (CurrentIndex == ElectionResults.Count - 1) return;
            CurrentIndex++;
            DisplayGraph(fullRefresh: false);
        }
        private void GoToLastYear()
        {
            if (CurrentIndex == ElectionResults.Count - 1) return;
            CurrentIndex = ElectionResults.Count - 1;
            DisplayGraph(fullRefresh: false);
        }

        private void NonEliminated_OnToggle(bool value)
        {
            DisplayGraph(fullRefresh: true);
        }

        private void ShowSeats_OnToggle(bool value)
        {
            DisplayGraph(fullRefresh: true);
        }

        // Start is called before the first frame update
        void Start()
        {
            FirstYearButton.onClick.AddListener(GoToFirstYear);
            FirstYearButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            PrevYearButton.onClick.AddListener(GoToPreviousYear);
            PrevYearButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            NextYearButton.onClick.AddListener(GoToNextYear);
            NextYearButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            LastYearButton.onClick.AddListener(GoToLastYear);
            LastYearButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            ShowSeatsToggle.onValueChanged.AddListener(ShowSeats_OnToggle);
            ShowSeatsToggle.onValueChanged.AddListener((value) => AudioManager.PlayStandardClickSound());

            NonEliminatedToggle.onValueChanged.AddListener(NonEliminated_OnToggle);
            NonEliminatedToggle.onValueChanged.AddListener((value) => AudioManager.PlayStandardClickSound());
        }
    }
}
