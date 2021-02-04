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

        public UI_DistrictAttribute AttributePrefab;
        public UI_ModifierListElement ModifierListElementPrefab;

        [Header("Header")]
        public Button BackButton;
        public Text TitleText;
        public Text PopulationText;
        public Text SeatsText;

        [Header("Traits")]
        public GameObject GeographyPanel;
        public GameObject DensityContainer;
        public GameObject AgeGroupContainer;
        public GameObject ReligionContainer;
        public GameObject LanguageContainer;
        public GameObject EconomyPanel;
        public GameObject MentalityPanel;

        [Header("Modifiers")]
        public GameObject ModifierContainer;
        public GameObject ModifierContent;

        [Header("Election Graph")]
        public UI_DistrictElectionGraph ElectionGraph;

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
                UI_DistrictAttribute geoAtt = Instantiate(AttributePrefab, GeographyPanel.transform);
                geoAtt.Init(UI, gt);
            }

            // Demography
            UI_DistrictAttribute densityAtt = Instantiate(AttributePrefab, DensityContainer.transform);
            densityAtt.Init(UI, d.Density);
            densityAtt.transform.SetAsFirstSibling();
            UI_DistrictAttribute ageAtt = Instantiate(AttributePrefab, AgeGroupContainer.transform);
            ageAtt.Init(UI, d.AgeGroup);
            ageAtt.transform.SetAsFirstSibling();
            UI_DistrictAttribute religionAtt = Instantiate(AttributePrefab, ReligionContainer.transform);
            religionAtt.Init(UI, d.Religion);
            religionAtt.transform.SetAsFirstSibling();
            UI_DistrictAttribute languageAtt = Instantiate(AttributePrefab, LanguageContainer.transform);
            languageAtt.Init(UI, d.Language);
            languageAtt.transform.SetAsFirstSibling();

            // Economy
            UI_DistrictAttribute eco1Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco1Att.Init(UI, d.Economy1);
            UI_DistrictAttribute eco2Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco2Att.Init(UI, d.Economy2);
            UI_DistrictAttribute eco3Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco3Att.Init(UI, d.Economy3);

            // Mentality
            foreach (Mentality m in d.Mentalities)
            {
                UI_DistrictAttribute mentalityAtt = Instantiate(AttributePrefab, MentalityPanel.transform);
                mentalityAtt.Init(UI, m.Name, m.Name, m.Description);
                mentalityAtt.DisplayText.alignment = TextAnchor.MiddleRight;
            }

            // Modifiers
            if (d.Modifiers.Count > 0)
            {
                ModifierContainer.SetActive(true);
                foreach (Modifier m in d.Modifiers)
                {
                    UI_ModifierListElement modElem = Instantiate(ModifierListElementPrefab, ModifierContent.transform);
                    modElem.Init(m);
                }
            }
            else ModifierContainer.SetActive(false);

            // Election Result
            if (d.ElectionResults.Count > 0)
            {
                ElectionGraph.gameObject.SetActive(true);
                ElectionGraph.Init(d.ElectionResults);
            }
            else ElectionGraph.gameObject.SetActive(false);
        }

        private void ClearAllPanels()
        {
            for (int i = 1; i < GeographyPanel.transform.childCount; i++) Destroy(GeographyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < EconomyPanel.transform.childCount; i++) Destroy(EconomyPanel.transform.GetChild(i).gameObject);
            for (int i = 1; i < MentalityPanel.transform.childCount; i++) Destroy(MentalityPanel.transform.GetChild(i).gameObject);
            for (int i = 0; i < ModifierContent.transform.childCount; i++) Destroy(ModifierContent.transform.GetChild(i).gameObject);

            for (int i = 0; i < DensityContainer.transform.childCount - 1; i++) Destroy(DensityContainer.transform.GetChild(i).gameObject);
            for (int i = 0; i < AgeGroupContainer.transform.childCount - 1; i++) Destroy(AgeGroupContainer.transform.GetChild(i).gameObject);
            for (int i = 0; i < ReligionContainer.transform.childCount - 1; i++) Destroy(ReligionContainer.transform.GetChild(i).gameObject);
            for (int i = 0; i < LanguageContainer.transform.childCount - 1; i++) Destroy(LanguageContainer.transform.GetChild(i).gameObject);
        }
    }
}
