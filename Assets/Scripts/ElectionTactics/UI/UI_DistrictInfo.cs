using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictInfo : MonoBehaviour
    {
        public Text TitleText;
        public Text AttributePrefab;

        public int Population;
        public int Votes;

        public GameObject GeographyPanel;
        public GameObject DemographyPanel;
        public GameObject EconomyPanel;
        public GameObject CulturePanel;
        public WindowGraph ElectionGraph;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetDistrict(District d)
        {
            ClearAllPanels();

            TitleText.text = d.Name;

            // Geography
            foreach (GeographyTrait gt in d.Geography)
            {
                Text text = Instantiate(AttributePrefab, GeographyPanel.transform);
                text.text = gt.ToString();
            }

            // Demography
            Text densityText = Instantiate(AttributePrefab, DemographyPanel.transform);
            densityText.text = "Density: " + d.Density.ToString();
            densityText.alignment = TextAnchor.MiddleRight;
            Text languageText = Instantiate(AttributePrefab, DemographyPanel.transform);
            languageText.text = "Language: " + d.Language.ToString();
            languageText.alignment = TextAnchor.MiddleRight;
            Text religionText = Instantiate(AttributePrefab, DemographyPanel.transform);
            religionText.text = "Religion: " +  d.Religion.ToString();
            religionText.alignment = TextAnchor.MiddleRight;
            Text ageText = Instantiate(AttributePrefab, DemographyPanel.transform);
            ageText.text = "Age: " + d.AgeGroup.ToString();
            ageText.alignment = TextAnchor.MiddleRight;

            // Economy
            Text eco1Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco1Text.text = "1. " + d.Economy1.ToString();
            Text eco2Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco2Text.text = "2. " + d.Economy2.ToString();
            Text eco3Text = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco3Text.text = "3. " + d.Economy3.ToString();

            // Culture
            foreach (CultureTrait ct in d.CultureTraits)
            {
                Text text = Instantiate(AttributePrefab, CulturePanel.transform);
                text.text = ct.ToString();
                text.alignment = TextAnchor.MiddleRight;
            }
        }

        private void ClearAllPanels()
        {
            for (int i = 1; i < GeographyPanel.transform.childCount; i++) Destroy(GeographyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < DemographyPanel.transform.childCount; i++) Destroy(DemographyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < EconomyPanel.transform.childCount; i++) Destroy(EconomyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < CulturePanel.transform.childCount; i++) Destroy(CulturePanel.transform.GetChild(i).gameObject);
        }
    }
}
