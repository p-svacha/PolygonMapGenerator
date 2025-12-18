using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictInfo : MonoBehaviour
    {
        public UI_ElectionTactics UI;

        public UI_DistrictAttribute AttributePrefab;
        public UI_ModifierListElement ModifierListElementPrefab;

        private District CurrentDistrict;

        [Header("Header")]
        public Button BackButton;
        public Text TitleText;
        public Text PopulationText;
        public Text SeatsText;
        public Text PopularityText;
        public UI_PopularityBreakdown PopularityBreakdown;

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

            // Popularity breakdown triggers
            EventTrigger popularityTrigger = PopularityText.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry mouseEnterEntry = new EventTrigger.Entry();
            mouseEnterEntry.eventID = EventTriggerType.PointerEnter;
            mouseEnterEntry.callback.AddListener((x) => { PopularityBreakdown.gameObject.SetActive(true); PopularityBreakdown.Init(CurrentDistrict, UI.Game.LocalPlayerParty); });
            popularityTrigger.triggers.Add(mouseEnterEntry);

            EventTrigger.Entry mouseExitEntry = new EventTrigger.Entry();
            mouseExitEntry.eventID = EventTriggerType.PointerExit;
            mouseExitEntry.callback.AddListener((x) => PopularityBreakdown.gameObject.SetActive(false));
            popularityTrigger.triggers.Add(mouseExitEntry);
        }

        public void Init(District d)
        {
            ClearAllPanels();

            CurrentDistrict = d;

            // Header
            TitleText.text = d.Name;
            PopulationText.text = d.Population.ToString("N0");
            SeatsText.text = d.Seats.ToString();
            PopularityText.text = d.GetPartyPopularity(UI.Game.LocalPlayerParty).ToString();

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
            eco1Att.Init(UI, d.Economy1, rank: 1);
            UI_DistrictAttribute eco2Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco2Att.Init(UI, d.Economy2, rank: 2);
            UI_DistrictAttribute eco3Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco3Att.Init(UI, d.Economy3, rank: 3);

            // Mentality
            foreach (MentalityTrait m in d.MentalityTraits)
            {
                UI_DistrictAttribute mentalityAtt = Instantiate(AttributePrefab, MentalityPanel.transform);
                mentalityAtt.Init(UI, m.LabelCapWord, m.LabelCapWord, m.Description);
                mentalityAtt.DisplayText.alignment = TextAnchor.MiddleRight;
            }
            MentalityPanel.gameObject.SetActive(d.MentalityTraits.Count > 0);

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
