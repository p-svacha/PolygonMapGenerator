using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
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
        public TMP_Dropdown OverlayDropdown;

        [Header("Legend")]
        public TextMeshProUGUI TitleLabel;
        public GameObject LegendContainer;
        public TextMeshProUGUI LegendTitleText;

        public GameObject LinearScaleLegend;
        public Image LinearScaleLegendImage;
        public TextMeshProUGUI PopularityLegendTopLimitText;
        public TextMeshProUGUI PopularityLegendBotLimitText;

        private List<LegendEntryData> Legend = new List<LegendEntryData>();

        private MapDisplayMode DisplayMode;
        private DistrictLabelMode DistrictLabelMode;

        // Specific Legends
        private List<LegendEntryData> PolicyImpactLegend;

        private void Start()
        {
            // Static legends initialization
            PolicyImpactLegend  = new List<LegendEntryData>()
            {
                new LegendEntryData("40", ColorManager.Instance.VeryHighImpactColor, "Very High", 40),
                new LegendEntryData("30", ColorManager.Instance.HighImpactColor, "High", 30),
                new LegendEntryData("20", ColorManager.Instance.MediumImpactColor, "Medium", 20),
                new LegendEntryData("10", ColorManager.Instance.LowImpactColor, "Low", 10),
                new LegendEntryData("0", ColorManager.Instance.NoImpactColor, "None", 0),
                new LegendEntryData("-10", ColorManager.Instance.NegativeImpactColor, "Negative", -10),
            };

            // Dropdown
            List<string> options = new List<string>();
            foreach(MapDisplayMode displayMode in Enum.GetValues(typeof(MapDisplayMode)))
            {
                options.Add(EnumHelper.GetDescription<MapDisplayMode>(displayMode));
            }
            OverlayDropdown.ClearOptions();
            OverlayDropdown.AddOptions(options);

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

            // Label
            // TitleLabel.text = OverlayDropdown.options[OverlayDropdown.value].text;

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
                    Legend.Add(new LegendEntryData("0", ColorManager.Instance.InactiveDistrictColor, "District"));
                    foreach(Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r)) r.SetColor(ColorManager.Instance.NoImpactColor);
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.LastElection:
                    LegendTitleText.text = "Current Winner";
                    Legend.Add(new LegendEntryData("none", ColorManager.Instance.NoImpactColor, "None", 1));
                    foreach (Party party in Game.Parties) Legend.Add(new LegendEntryData(party.Id.ToString(), party.Color, party.Acronym));
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
                    LinearScaleLegend.gameObject.SetActive(true);
                    LinearScaleLegendImage.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/legendGradient");
                    LegendTitleText.text = "Popularity";

                    // Take current max popularity as max
                    int maxPopularity = Game.ActiveDistricts.Max(d => d.GetPartyPopularity(Game.LocalPlayerParty));

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

                case MapDisplayMode.SeatsWon:
                    LinearScaleLegend.gameObject.SetActive(true);
                    LinearScaleLegendImage.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/legendGradient_WhiteToGreen");
                    LegendTitleText.text = "Seats Won";

                    // Take current max popularity as max
                    PopularityLegendBotLimitText.text = "- 0%";
                    PopularityLegendTopLimitText.text = "- 100%";

                    if (ElectionTacticsGame.Instance.ElectionCycle == 1) // Cannot show anything before first election
                    {
                        foreach (Region r in Map.LandRegions)
                        {
                            if (Game.VisibleDistricts.ContainsKey(r)) r.SetColor(Color.white);
                            else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                        }
                    }

                    else
                    {
                        GeneralElectionResult result = Game.GetLatestElectionResult();
                        foreach (Region r in Map.LandRegions)
                        {
                            if (Game.VisibleDistricts.ContainsKey(r))
                            {
                                District d = Game.GetDistrict(r);
                                DistrictElectionResult districtResult = result.GetDistrictResult(d);

                                if (districtResult == null)
                                {
                                    r.SetColor(new Color(0.9f, 0.9f, 0.9f));
                                }
                                else
                                {
                                    int seatsWon = districtResult.SeatsWon[Game.LocalPlayerParty];
                                    int totalSeats = result.GetDistrictResult(d).Seats;

                                    float f = seatsWon / (float)totalSeats;
                                    if (f > 0f)
                                    {
                                        r.SetColor(new Color(0.7f - (0.7f * f), 0.9f - (0.45f * f), 0.7f - (0.7f * f)));
                                    }
                                    else r.SetColor(new Color(0.9f, 0.9f, 0.9f));
                                }
                            }
                            else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                        }
                    }
                    break;

                case MapDisplayMode.Language:
                    LegendTitleText.text = "Languages";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            LanguageDef language = Game.VisibleDistricts[r].Language;
                            if (!HasLegendEntry(language.DefName)) Legend.Add(new LegendEntryData(language.DefName, language.Color, language.LabelCapWord));
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
                            if (!HasLegendEntry(religion.DefName)) Legend.Add(new LegendEntryData(religion.DefName, religion.Color, religion.LabelCapWord, religion.SortingOrder));
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
                            if (!HasLegendEntry(ageGroup.DefName)) Legend.Add(new LegendEntryData(ageGroup.DefName, ageGroup.Color, ageGroup.Label, ageGroup.SortingOrder));
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
                            if (!HasLegendEntry(density.DefName)) Legend.Add(new LegendEntryData(density.DefName, density.Color, density.Label, density.SortingOrder));
                            r.SetColor(density.Color);
                        }
                        else r.SetColor(ColorManager.Instance.InactiveDistrictColor);
                    }
                    break;
            }

            // Finalize legend
            DisplayLegend(Legend);

            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Clears all legend entries except for fixed ones (like title, popularity legend)
        /// </summary>
        private void ClearLegend()
        {
            LinearScaleLegend.gameObject.SetActive(false);
            for (int i = 3; i < LegendContainer.transform.childCount; i++) Destroy(LegendContainer.transform.GetChild(i).gameObject);
            Legend.Clear();
        }

        public void RefreshMapDisplay()
        {
            DoSetMapDisplayMode(DisplayMode, DistrictLabelMode);
        }

        private bool HasLegendEntry(string id) => Legend.Any(e => e.Id == id);

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

        public void ShowGeographyOverlay(GeographyTraitDef def)
        {
            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }
        public void ShowEconomicSectorOverlay(EconomicSectorDef def)
        {
            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }
        public void ShowDensityOverlay(DensityDef def)
        {
            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }
        public void ShowAgeOverlay(AgeGroupDef def)
        {
            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }
        public void ShowLanguageOverlay(LanguageDef def)
        {
            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }
        public void ShowReligionOverlay(ReligionDef def)
        {
            if (def == ReligionDefOf.None) return;

            Policy policy = Game.LocalPlayerParty.GetPolicy(def);
            ShowPolicyImpact(policy);
        }

        private void ColorDistrictByPolicyImpact(Region region, Policy policy)
        {
            District district = Game.VisibleDistricts[region];
            int policyImpact = policy.GetSinglePointPopularityDelta(district);
            Color impactColor = ColorManager.Instance.GetImpactColor(policyImpact);
            region.SetColor(impactColor);
        }

        private void ShowOverlayLegend(string title)
        {
            ClearLegend();

            LegendTitleText.text = "Impact " + title;
            DisplayLegend(PolicyImpactLegend);
        }

        private void DisplayLegend(List<LegendEntryData> legend)
        {
            legend = legend.OrderByDescending(e => e.SortingOrder).ThenBy(e => e.Label).ToList();
            foreach (LegendEntryData data in legend)
            {
                LegendEntry entry = Instantiate(LegendEntryPrefab, LegendContainer.transform);
                entry.Init(data);
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
        [Description("Seats Won")] SeatsWon = 3,
        Language = 4,
        Religion = 5,
        [Description("Age Group")] AgeGroup = 6,
        Density = 7,
    }

    public enum DistrictLabelMode
    {
        Default, // Current values are shown, background represents last election winner
        InElection // Values before last election are shown, background is white
    }
}
