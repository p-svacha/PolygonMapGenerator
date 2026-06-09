using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ElectionTactics
{
    public class UI_DistrictInfo : MonoBehaviour
    {
        public UI_ElectionTactics UI;

        public UI_ModifierListElement ModifierListElementPrefab;

        private District CurrentDistrict;

        [Header("Header")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;

        public UI_SeatNumber SeatsInfo;
        public UI_SeatNumber PopularityInfo;

        public GameObject PopularityBreakdown;
        public UI_InfoTable PopularityBreakdownTable;

        [Header("Traits")]
        public UI_InfoTableRow PopulationInfo;
        public UI_PopulationGrowthIndicator PopulationGrowthIndicator;
        public GameObject PopulationGrowthInfo;
        public TextMeshProUGUI PopulationGrowthValueText;
        public TextMeshProUGUI SeatChangeText;

        public UI_InfoTableRow LanguageInfo;
        public UI_InfoTableRow ReligionInfo;
        public UI_InfoTableRow DensityInfo;
        public UI_InfoTableRow AgeGroupInfo;

        public UI_InfoTableRow Economy1Info;
        public UI_InfoTableRow Economy2Info;
        public UI_InfoTableRow Economy3Info;

        public GameObject GeographyRowIII;
        public GameObject GeographyRowII;
        public GameObject GeographyRowI;
        public UI_Trait GeographicTraitPrefab;

        public UI_TraitContainer CulturalTraitsPanel;

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
            PopularityInfo.SetHoverAction(() => { PopularityBreakdown.gameObject.SetActive(true); PopularityBreakdownTable.InitPopularityBreakdown(CurrentDistrict, UI.Game.LocalPlayerParty); });
            PopularityInfo.SetUnhoverAction(() => PopularityBreakdown.gameObject.SetActive(false));

            // Population
            PopulationInfo.SetHoverAction(ShowPopulationGrowthInfo);
            PopulationInfo.SetUnhoverAction(HidePopulationGrowthInfo);
            PopulationGrowthInfo.gameObject.SetActive(false);

            // Demography
            PopulationInfo.LabelTooltipTarget.Init("Population", "Determines the number of seats this district holds in parliament.\n\nGrows or shrinks each cycle based on the district's growth rate.");

            DensityInfo.LabelTooltipTarget.Init("Density", "Population density, derived from the district's size and population.\n\nUpdates each cycle as population changes.\n\nSmaller districts tend toward higher density.");
            DensityInfo.SetHoverAction(() => { UI.MapControls.ShowDensityOverlay(CurrentDistrict.Density); });
            DensityInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            AgeGroupInfo.LabelTooltipTarget.Init("Primary Age Group", "The dominant generation of the population.\n\nSlightly modifies the district's natural population growth rate.");
            AgeGroupInfo.SetHoverAction(() => { UI.MapControls.ShowAgeOverlay(CurrentDistrict.AgeGroup); });
            AgeGroupInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            ReligionInfo.LabelTooltipTarget.Init("Religion", "The dominant religion practiced in this district.\n\nSpreads along land and water connections when new districts form.\n\nNever changes once established.");
            ReligionInfo.SetHoverAction(() => { UI.MapControls.ShowReligionOverlay(CurrentDistrict.Religion); });
            ReligionInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            LanguageInfo.LabelTooltipTarget.Init("Language", "The dominant language spoken in this district.\n\nSpreads along land borders when new districts form.\n\nNever changes once established.");
            LanguageInfo.SetHoverAction(() => { UI.MapControls.ShowLanguageOverlay(CurrentDistrict.Language); });
            LanguageInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            // Economy
            Economy1Info.LabelTooltipTarget.Init("Dominant Industry", "The district's primary and most important economic sector.\n\nSupporting this policy gives +7 popularity here.");
            Economy1Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(CurrentDistrict.Economy1); });
            Economy1Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            Economy2Info.LabelTooltipTarget.Init("Major Industry", "The district's second most important economic sector.\n\nSupporting this policy gives +5 popularity here.");
            Economy2Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(CurrentDistrict.Economy2); });
            Economy2Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            Economy3Info.LabelTooltipTarget.Init("Minor Industry", "The districts third most important economic sector.\n\nSupporting this policy gives +3 popularity here.");
            Economy3Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(CurrentDistrict.Economy3); });
            Economy3Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });
        }

        public void Init(District district)
        {
            ClearAllPanels();

            CurrentDistrict = district;

            // Header
            TitleText.text = district.Name;

            SeatsInfo.InitDistrictSeats(district.Seats, district.GetSeatAllocationMethod(), darkMode: false);
            PopularityInfo.SetValue(district.GetPartyPopularity(UI.Game.LocalPlayerParty).ToString());

            // Population
            PopulationInfo.SetValue((Mathf.Round(district.Population / 1000f) * 1000).ToString("N0")); // Rounded to the thousands

            float growthRate = district.GetPopulationGrowthRate();
            PopulationGrowthIndicator.Init(growthRate);
            PopulationGrowthValueText.text = growthRate.ToString("+0.##;-0.##;0") + "% / cycle";
            int seatChangeCountdown = district.GetSeatChangeCountdown();
            if (seatChangeCountdown == -1) SeatChangeText.text = "Seat amount will not change.";
            else
            {
                if (growthRate > 0f) SeatChangeText.text = $"Gains a seat in {seatChangeCountdown} {"cycle".Pluralize(seatChangeCountdown)}";
                else SeatChangeText.text = $"Loses a seat in {seatChangeCountdown} {"cycle".Pluralize(seatChangeCountdown)}";
            }

            // Geography
            InitGeographyPanel();

            // Demography
            DensityInfo.SetValue(district.Density.Label);
            AgeGroupInfo.SetValue(district.AgeGroup.Label);
            ReligionInfo.SetValue(district.Religion.Label);
            LanguageInfo.SetValue(district.Language.Label);

            // Economy
            Economy1Info.SetValue(district.Economy1.Label);
            Economy2Info.SetValue(district.Economy2.Label);
            Economy3Info.SetValue(district.Economy3.Label);

            // Cultural Traits
            CulturalTraitsPanel.InitCulturalTraits(district);

            // Modifiers
            if (district.Modifiers.Count > 0)
            {
                ModifierContainer.SetActive(true);
                foreach (Modifier m in district.Modifiers)
                {
                    UI_ModifierListElement modElem = Instantiate(ModifierListElementPrefab, ModifierContent.transform);
                    modElem.Init(m);
                }
            }
            else ModifierContainer.SetActive(false);

            // Election Result
            if (district.ElectionResults.Count > 0)
            {
                ElectionGraph.gameObject.SetActive(true);
                ElectionGraph.Init(district.ElectionResults);
            }
            else ElectionGraph.gameObject.SetActive(false);
        }

        private void InitGeographyPanel()
        {
            InitGeographyRow(GeographyRowIII, 3);
            InitGeographyRow(GeographyRowII, 2);
            InitGeographyRow(GeographyRowI, 1);
        }

        private void InitGeographyRow(GameObject row, int category)
        {
            HelperFunctions.DestroyAllChildredImmediately(row, skipElements: 1);
            List<GeographyTrait> traits = CurrentDistrict.Geography.Where(t => t.Category == category).OrderByDescending(t => t.Label).ToList();

            foreach (GeographyTrait trait in traits)
            {
                UI_Trait elem = Instantiate(GeographicTraitPrefab, row.transform);
                elem.InitGeographyTrait(trait);
            }
        }

        private void ClearAllPanels()
        {
            for (int i = 0; i < ModifierContent.transform.childCount; i++) Destroy(ModifierContent.transform.GetChild(i).gameObject);
        }

        private void ShowPopulationGrowthInfo()
        {
            PopulationGrowthInfo.gameObject.SetActive(true);
        }
        private void HidePopulationGrowthInfo()
        {
            PopulationGrowthInfo.gameObject.SetActive(false);
        }
    }
}
