﻿using System.Collections;
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
        public Text DensityText;
        public Text AgeText;
        public Text LanguageText;
        public Text ReligionText;
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
                geoAtt.Init(gt.FullName, gt.BaseName, "");
            }

            // Demography
            DensityText.text = EnumHelper.GetDescription(d.Density);
            AgeText.text = EnumHelper.GetDescription(d.AgeGroup);
            LanguageText.text = EnumHelper.GetDescription(d.Language);
            ReligionText.text = EnumHelper.GetDescription(d.Religion);

            // Economy
            UI_DistrictAttribute eco1Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco1Att.Init("1. " + EnumHelper.GetDescription(d.Economy1), EnumHelper.GetDescription(d.Economy1), "");
            UI_DistrictAttribute eco2Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco2Att.Init("2. " + EnumHelper.GetDescription(d.Economy2), EnumHelper.GetDescription(d.Economy2), "");
            UI_DistrictAttribute eco3Att = Instantiate(AttributePrefab, EconomyPanel.transform);
            eco3Att.Init("3. " + EnumHelper.GetDescription(d.Economy3), EnumHelper.GetDescription(d.Economy3), "");

            // Mentality
            foreach (Mentality m in d.Mentalities)
            {
                UI_DistrictAttribute mentalityAtt = Instantiate(AttributePrefab, MentalityPanel.transform);
                mentalityAtt.Init(m.Name, m.Name, m.Description);
                mentalityAtt.MainText.alignment = TextAnchor.MiddleRight;
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
        }
    }
}