using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictInfo : MonoBehaviour
    {
        public UI_ElectionTactics UI;

        public Text AttributePrefab;
        public Font GraphFont;

        public Button BackButton;
        public Text TitleText;
        public Text PopulationText;
        public Text SeatsText;

        public GameObject GeographyPanel;
        public Text DensityText;
        public Text AgeText;
        public Text LanguageText;
        public Text ReligionText;
        public GameObject EconomyPanel;
        public GameObject CulturePanel;
        public WindowGraph ElectionGraph;

        // Start is called before the first frame update
        void Start()
        {
            BackButton.onClick.AddListener(() => UI.SelectTab(Tab.DistrictList));
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(District d)
        {
            ClearAllPanels();

            // Header
            TitleText.text = d.Name;
            PopulationText.text = d.Population.ToString("N0");
            SeatsText.text = d.Seats.ToString();

            // Geography
            foreach (GeographyTrait gt in d.Geography)
            {
                Text text = Instantiate(AttributePrefab, GeographyPanel.transform);
                text.text = EnumHelper.GetDescription(gt);
            }

            // Demography
            DensityText.text = EnumHelper.GetDescription(d.Density);
            AgeText.text = EnumHelper.GetDescription(d.AgeGroup);
            LanguageText.text = EnumHelper.GetDescription(d.Language);
            ReligionText.text = EnumHelper.GetDescription(d.Religion);

            // Economy
            Text eco1Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco1Text.text = "1. " + EnumHelper.GetDescription(d.Economy1);
            Text eco2Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco2Text.text = "2. " + EnumHelper.GetDescription(d.Economy2);
            Text eco3Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco3Text.text = "3. " + EnumHelper.GetDescription(d.Economy3);

            // Culture
            foreach (Mentality ct in d.Mentalities)
            {
                Text text = Instantiate(AttributePrefab, CulturePanel.transform);
                text.text = ct.ToString();
                text.alignment = TextAnchor.MiddleRight;
            }

            // Election Result
            if (d.LastElectionResult != null)
            {
                List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
                foreach (KeyValuePair<Party, float> kvp in d.LastElectionResult.VoteShare)
                    dataPoints.Add(new GraphDataPoint(kvp.Key.Acronym, kvp.Value, kvp.Key.Color));
                int yMax = (((int)d.LastElectionResult.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;
                ElectionGraph.ShowAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, GraphFont, 0.25f);
            }
            else
            {
                ElectionGraph.ClearGraph();
            }
        }

        private void ClearAllPanels()
        {
            for (int i = 1; i < GeographyPanel.transform.childCount; i++) Destroy(GeographyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < EconomyPanel.transform.childCount; i++) Destroy(EconomyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < CulturePanel.transform.childCount; i++) Destroy(CulturePanel.transform.GetChild(i).gameObject);
        }
    }
}
