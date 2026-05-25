using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class MapControls : UIElement
    {
        public LegendEntry LegendEntryPrefab;

        private ElectionTacticsGame Game;
        private Map Map;

        [Header("Footer Menubar")]
        public Dropdown OverlayDropdown;

        [Header("Legend")]
        public GameObject LegendContainer;
        public Text LegendTitleText;

        public GameObject PopularityLegend;
        public Text PopularityLegendTopLimitText;
        public Text PopularityLegendBotLimitText;

        private Dictionary<Color, string> Legend = new Dictionary<Color, string>();

        private MapDisplayMode DisplayMode;
        private DistrictLabelMode DistrictLabelMode;

        // Specific Legends
        private Dictionary<Color, string> NoOverlayLegend = new Dictionary<Color, string>();
        private Dictionary<Color, string> ImpactLegend = new Dictionary<Color, string>();

        private void Start()
        {
            // Create default legends
            NoOverlayLegend.Add(ColorManager.Instance.InactiveDistrictColor, "District");
            ImpactLegend.Add(ColorManager.Instance.VeryHighImpactColor, "Very High");
            ImpactLegend.Add(ColorManager.Instance.HighImpactColor, "High");
            ImpactLegend.Add(ColorManager.Instance.MediumImpactColor, "Medium");
            ImpactLegend.Add(ColorManager.Instance.LowImpactColor, "Low");
            ImpactLegend.Add(ColorManager.Instance.NoImpactColor, "None");
            ImpactLegend.Add(ColorManager.Instance.NegativeImpactColor, "Negative");

            foreach(MapDisplayMode displayMode in Enum.GetValues(typeof(MapDisplayMode)))
                OverlayDropdown.options.Add(new Dropdown.OptionData(EnumHelper.GetDescription<MapDisplayMode>(displayMode)));

            OverlayDropdown.onValueChanged.AddListener(OverlayDropdown_OnValueChanged);
            OverlayDropdown.onValueChanged.AddListener(_ => AudioManager.PlayStandardClickSound());
        }

        public void Init(ElectionTacticsGame game, MapDisplayMode mapDisplayMode, DistrictLabelMode districtLabelMode)
        {
            Game = game;
            Map = game.Map;
            SetMapDisplayMode(mapDisplayMode, districtLabelMode);
        }

        public void SetMapDisplayMode(MapDisplayMode mapDisplayMode, DistrictLabelMode districtLabelMode)
        {
            DistrictLabelMode = districtLabelMode;
            int newValue = (int)mapDisplayMode;
            if (newValue == OverlayDropdown.value) RefreshMapDisplay();
            else OverlayDropdown.value = (int)mapDisplayMode;
        }

        public void OverlayDropdown_OnValueChanged(int value)
        {
            DoSetMapDisplayMode((MapDisplayMode)value, DistrictLabelMode);
        }


        #region Display Modes

        private void DoSetMapDisplayMode(MapDisplayMode mapDisplayMode, DistrictLabelMode districtLabelMode)
        {
            ClearLegend();

            // Show region borders in active districts only
            foreach(Region r in Map.LandRegions) r.SetShowRegionBorders(Game.VisibleDistricts.ContainsKey(r));

            // Update district labels
            foreach (District d in Game.VisibleDistricts.Values)
            {
                // Reset temporary popularity impact values
                d.MapLabel.HidePolicyImpact();

                // Show district winners as district label background color
                d.MapLabel.Refresh(districtLabelMode);
            }

            DisplayMode = mapDisplayMode;
            switch(mapDisplayMode)
            {
                case MapDisplayMode.NoOverlay:
                    LegendTitleText.text = "Districts";
                    foreach (KeyValuePair<Color, string> kvp in NoOverlayLegend) Legend.Add(kvp.Key, kvp.Value);
                    foreach(Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r)) r.SetColor(ColorManager.Instance.NoImpactColor);
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.LastElection:
                    LegendTitleText.text = "Parties";
                    Legend.Add(ColorManager.Instance.NoImpactColor, "None");
                    foreach (Party party in Game.Parties) Legend.Add(party.Color, party.Acronym);
                    foreach (Region r in Map.LandRegions) 
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            Party displayedParty = Game.VisibleDistricts[r].CurrentWinnerParty;
                            if (displayedParty != null) r.SetColor(displayedParty.Color);
                            else r.SetColor(ColorManager.Instance.NoImpactColor);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Popularity:
                    PopularityLegend.gameObject.SetActive(true);
                    LegendTitleText.text = "Popularity";
                    int maxPopularity = 90 + 10 * Game.ElectionCycle;
                    PopularityLegendBotLimitText.text = "- 0";
                    PopularityLegendTopLimitText.text = "- " + maxPopularity;
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            int popularity = Game.VisibleDistricts[r].GetPartyPopularity(Game.LocalPlayerParty);
                            float f = popularity / (float)maxPopularity;
                            Mathf.Clamp(f, 0f, 1f);
                            r.SetColor(new Color(1f - f, f, 0f));
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Language:
                    LegendTitleText.text = "Languages";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            LanguageDef language = Game.VisibleDistricts[r].Language;
                            if (!Legend.ContainsValue(language.LabelCapWord)) Legend.Add(language.Color, language.LabelCapWord);
                            r.SetColor(language.Color);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Religion:
                    LegendTitleText.text = "Religions";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            ReligionDef religion = Game.VisibleDistricts[r].Religion;
                            if (!Legend.ContainsValue(religion.LabelCapWord)) Legend.Add(religion.Color, religion.LabelCapWord);
                            r.SetColor(religion.Color);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.AgeGroup:
                    LegendTitleText.text = "Age Groups";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            AgeGroupDef ageGroup = Game.VisibleDistricts[r].AgeGroup;
                            if (!Legend.ContainsValue(ageGroup.Label)) Legend.Add(ageGroup.Color, ageGroup.Label);
                            r.SetColor(ageGroup.Color);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Density:
                    LegendTitleText.text = "Density";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            DensityDef density = Game.VisibleDistricts[r].Density;
                            if (!Legend.ContainsValue(density.Label)) Legend.Add(density.Color, density.Label);
                            r.SetColor(density.Color);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;
            }

            foreach(KeyValuePair<Color, string> kvp in Legend)
            {
                LegendEntry entry = Instantiate(LegendEntryPrefab, LegendContainer.transform);
                entry.Init(kvp.Key, kvp.Value);
            }

            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Clears all legend entries except for fixed ones (like title, popularity legend)
        /// </summary>
        private void ClearLegend()
        {
            PopularityLegend.gameObject.SetActive(false);
            for (int i = 3; i < LegendContainer.transform.childCount; i++) Destroy(LegendContainer.transform.GetChild(i).gameObject);
            Legend.Clear();
        }

        public void RefreshMapDisplay()
        {
            DoSetMapDisplayMode(DisplayMode, DistrictLabelMode);
        }

        #endregion

        #region Policy Overlays

        /// <summary>
        /// Shows the impact of the specified policy on all districts by coloring the districts based on impact and also showing the exact popularity impact value in the district labels.
        /// </summary>
        public void ShowPolicyImpact(Policy policy)
        {
            ShowOverlayLegend("Policy " + policy.Name);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                ColorDistrictByPolicyImpact(region, policy); // Color districts
                Game.VisibleDistricts[region].MapLabel.ShowPolicyImpact(policy); // Show in map labels
            }
        }

        public void ShowGeographyOverlay(GeographyTraitType t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                ShowPolicyImpact(policy);
            }
        }
        public void ShowEconomicSectorOverlay(EconomicSectorDef def)
        {
            ShowOverlayLegend(def.Label);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(def);
                ShowPolicyImpact(policy);
            }
        }
        public void ShowDensityOverlay(DensityDef def)
        {
            ShowOverlayLegend(def.Label);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(def);
                ShowPolicyImpact(policy);
            }
        }
        public void ShowAgeOverlay(AgeGroupDef def)
        {
            ShowOverlayLegend(def.Label);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(def);
                ShowPolicyImpact(policy);
            }
        }
        public void ShowLanguageOverlay(LanguageDef def)
        {
            ShowOverlayLegend(def.Label);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(def);
                ShowPolicyImpact(policy);
            }
        }
        public void ShowReligionOverlay(ReligionDef def)
        {
            if (def == ReligionDefOf.None) return;

            ShowOverlayLegend(def.Label);
            foreach (Region region in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(def);
                ShowPolicyImpact(policy);
            }
        }

        private void ColorDistrictByPolicyImpact(Region region, Policy policy)
        {
            District district = Game.VisibleDistricts[region];
            int policyImpact = policy.GetSinglePointImpactOn(district);
            Color impactColor = ColorManager.Instance.GetImpactColor(policyImpact);
            region.SetColor(impactColor);
        }

        private void ShowOverlayLegend(string title)
        {
            ClearLegend();

            LegendTitleText.text = "Impact " + title;
            foreach (KeyValuePair<Color, string> kvp in ImpactLegend) Legend.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<Color, string> kvp in Legend)
            {
                LegendEntry entry = Instantiate(LegendEntryPrefab, LegendContainer.transform);
                entry.Init(kvp.Key, kvp.Value);
            }
        }

        public void ClearOverlay()
        {
            RefreshMapDisplay();
        }

        #endregion
    }

    public enum MapDisplayMode
    {
        [Description("No Overlay")] NoOverlay = 0,
        [Description("Last Election")] LastElection = 1,
        Popularity = 2,
        Language = 3,
        Religion = 4,
        [Description("Age Group")] AgeGroup = 5,
        Density = 6,
    }

    public enum DistrictLabelMode
    {
        Default, // Current values are shown, background represents last election winner
        InElection // Values before last election are shown, background is white
    }
}
