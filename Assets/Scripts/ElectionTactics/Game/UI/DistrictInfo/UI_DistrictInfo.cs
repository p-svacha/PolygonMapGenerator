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
        public Text TitleText;
        public UI_InfoTableRow PopulationInfo;
        public UI_InfoTableRow SeatsInfo;
        public UI_InfoTableRow PopularityInfo;

        public GameObject PopularityBreakdown;
        public UI_InfoTable PopularityBreakdownTable;

        [Header("Traits")]
        public UI_InfoTableRow LanguageInfo;
        public UI_InfoTableRow ReligionInfo;
        public UI_InfoTableRow DensityInfo;
        public UI_InfoTableRow AgeGroupInfo;

        public UI_InfoTableRow Economy1Info;
        public UI_InfoTableRow Economy2Info;
        public UI_InfoTableRow Economy3Info;

        public UI_TraitContainer GeographyPanel;
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
        }

        public void Init(District district)
        {
            ClearAllPanels();

            CurrentDistrict = district;

            // Header
            TitleText.text = district.Name;

            PopulationInfo.SetValue(district.Population.ToString("N0"));

            SeatsInfo.SetValue(district.Seats.ToString());
            PopularityInfo.SetValue(district.GetPartyPopularity(UI.Game.LocalPlayerParty).ToString());

            // Geography
            GeographyPanel.InitGeographyTraits(district);

            // Demography
            DensityInfo.SetValue(district.Density.Label);
            DensityInfo.SetHoverAction(() => { UI.MapControls.ShowDensityOverlay(district.Density); });
            DensityInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            AgeGroupInfo.SetValue(district.AgeGroup.Label);
            AgeGroupInfo.SetHoverAction(() => { UI.MapControls.ShowAgeOverlay(district.AgeGroup); });
            AgeGroupInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            ReligionInfo.SetValue(district.Religion.Label);
            ReligionInfo.SetHoverAction(() => { UI.MapControls.ShowReligionOverlay(district.Religion); });
            ReligionInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            LanguageInfo.SetValue(district.Language.Label);
            LanguageInfo.SetHoverAction(() => { UI.MapControls.ShowLanguageOverlay(district.Language); });
            LanguageInfo.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            // Economy
            Economy1Info.SetValue(district.Economy1.Label);
            Economy1Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(district.Economy1); });
            Economy1Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            Economy2Info.SetValue(district.Economy2.Label);
            Economy2Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(district.Economy2); });
            Economy2Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

            Economy3Info.SetValue(district.Economy3.Label);
            Economy3Info.SetHoverAction(() => { UI.MapControls.ShowEconomicSectorOverlay(district.Economy3); });
            Economy3Info.SetUnhoverAction(() => { UI.MapControls.ClearOverlay(); });

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

        private void ClearAllPanels()
        {
            for (int i = 0; i < ModifierContent.transform.childCount; i++) Destroy(ModifierContent.transform.GetChild(i).gameObject);
        }
    }
}
