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
        public WindowGraph ElectionGraph;

        public List<DistrictElectionResult> ElectionResults;
        public int CurrentIndex;

        public void Init(List<DistrictElectionResult> results)
        {
            ElectionResults = results;
            CurrentIndex = results.Count - 1;
            DisplayGraph();
        }

        private void DisplayGraph(bool fullRefesh = true)
        {
            DistrictElectionResult result = ElectionResults[CurrentIndex];
            YearText.text = result.Year.ToString();
            if (result != null)
            {
                List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
                foreach (KeyValuePair<Party, float> kvp in result.VoteShare)
                {
                    List<Sprite> modifierIcons = new List<Sprite>();
                    List<string> iconTooltipTitles = new List<string>();
                    List<string> iconTooltipTexts = new List<string>();
                    foreach (Modifier m in result.Modifiers.Where(x => x.Party == kvp.Key))
                    {
                        modifierIcons.Add(IconManager.Icons.GetModifierIcon(m.Type));
                        iconTooltipTitles.Add(m.Type.ToString());
                        iconTooltipTexts.Add(m.Description + "\n\nSource: " + m.Source);
                    }
                    string label = GlobalSettings.DebugMode ? kvp.Key.Acronym + " / " + result.PartyPoints[kvp.Key] : kvp.Key.Acronym; // Show party points when in debug mode
                    dataPoints.Add(new GraphDataPoint(label, kvp.Value, kvp.Key.Color, modifierIcons, iconTooltipTitles, iconTooltipTexts));
                }
                int yMax = (((int)result.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;
                if (fullRefesh) ElectionGraph.InitAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, PrefabManager.Prefabs.GraphFont, 0.25f, startAnimation: true);
                else ElectionGraph.UpdateAnimatedBarGraph(dataPoints, yMax, 0.25f);
            }
        }

        private void GoToFirstYear()
        {
            if (CurrentIndex == 0) return;
            CurrentIndex = 0;
            DisplayGraph(false);
        }
        private void GoToPreviousYear()
        {
            if (CurrentIndex == 0) return;
            CurrentIndex--;
            DisplayGraph(false);
        }
        private void GoToNextYear()
        {
            if (CurrentIndex == ElectionResults.Count - 1) return;
            CurrentIndex++;
            DisplayGraph(false);
        }
        private void GoToLastYear()
        {
            if (CurrentIndex == ElectionResults.Count - 1) return;
            CurrentIndex = ElectionResults.Count - 1;
            DisplayGraph(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            FirstYearButton.onClick.AddListener(GoToFirstYear);   
            PrevYearButton.onClick.AddListener(GoToPreviousYear);   
            NextYearButton.onClick.AddListener(GoToNextYear);   
            LastYearButton.onClick.AddListener(GoToLastYear);
        }
    }
}
