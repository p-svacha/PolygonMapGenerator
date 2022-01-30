using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class MapControls : MonoBehaviour
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

        // Specific Legends
        private Dictionary<Color, string> NoOverlayLegend = new Dictionary<Color, string>();
        private Dictionary<Color, string> ImpactLegend = new Dictionary<Color, string>();

        private void Start()
        {
            // Create default legends
            NoOverlayLegend.Add(ColorManager.Singleton.InactiveDistrictColor, "District");
            ImpactLegend.Add(ColorManager.Singleton.VeryHighImpactColor, "Very High");
            ImpactLegend.Add(ColorManager.Singleton.HighImpactColor, "High");
            ImpactLegend.Add(ColorManager.Singleton.MediumImpactColor, "Medium");
            ImpactLegend.Add(ColorManager.Singleton.LowImpactColor, "Low");
            ImpactLegend.Add(ColorManager.Singleton.NoImpactColor, "None");
            ImpactLegend.Add(ColorManager.Singleton.NegativeImpactColor, "Negative");

            foreach(MapDisplayMode displayMode in Enum.GetValues(typeof(MapDisplayMode)))
                OverlayDropdown.options.Add(new Dropdown.OptionData(EnumHelper.GetDescription<MapDisplayMode>(displayMode)));

            OverlayDropdown.onValueChanged.AddListener(OverlayDropdown_OnValueChanged);
        }

        public void Init(ElectionTacticsGame game, MapDisplayMode mode)
        {
            Game = game;
            Map = game.Map;
            SetMapDisplayMode(mode);
        }

        public void SetMapDisplayMode(MapDisplayMode mode)
        {
            int newValue = (int)mode;
            if (newValue == OverlayDropdown.value) RefreshMapDisplay();
            else OverlayDropdown.value = (int)mode;
        }

        public void OverlayDropdown_OnValueChanged(int value)
        {
            DoSetMapDisplayMode((MapDisplayMode)value);
        }


        #region Display Modes

        private void DoSetMapDisplayMode(MapDisplayMode mode)
        {
            ClearLegend();

            // Show region borders in active districts only
            foreach(Region r in Map.LandRegions) r.SetShowRegionBorders(Game.VisibleDistricts.ContainsKey(r));

            DisplayMode = mode;
            switch(mode)
            {
                case MapDisplayMode.NoOverlay:
                    LegendTitleText.text = "Districts";
                    foreach (KeyValuePair<Color, string> kvp in NoOverlayLegend) Legend.Add(kvp.Key, kvp.Value);
                    foreach(Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r)) r.SetColor(ColorManager.Singleton.NoImpactColor);
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.LastElection:
                    LegendTitleText.text = "Parties";
                    Legend.Add(ColorManager.Singleton.NoImpactColor, "None");
                    foreach(Party party in Game.Parties) Legend.Add(party.Color, party.Acronym);
                    foreach (Region r in Map.LandRegions) 
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            Party displayedParty = Game.VisibleDistricts[r].CurrentWinnerParty;
                            if (displayedParty != null) r.SetColor(displayedParty.Color);
                            else r.SetColor(ColorManager.Singleton.NoImpactColor);
                        }
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
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
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Language:
                    LegendTitleText.text = "Languages";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.VisibleDistricts[r].Language);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Religion:
                    LegendTitleText.text = "Religions";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.VisibleDistricts[r].Religion);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.AgeGroup:
                    LegendTitleText.text = "Age Groups";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.VisibleDistricts[r].AgeGroup);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Density:
                    LegendTitleText.text = "Density";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.VisibleDistricts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.VisibleDistricts[r].Density);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Singleton.InactiveDistrictColor);
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

        private void HandleLegendEntry(Region r, string label)
        {
            if (!Legend.ContainsValue(label))
            {
                Color c = ColorManager.Singleton.LegendColors[Legend.Count];
                Legend.Add(c, label);
                r.SetColor(c);
            }
            else
            {
                Color c = Legend.First(x => x.Value == label).Key;
                r.SetColor(c);
            }
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
            DoSetMapDisplayMode(DisplayMode);
        }

        #endregion

        #region Policy Overlays

        public void ShowGeographyOverlay(GeographyTraitType t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }
        public void ShowEconomyOverlay(EconomyTrait t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }
        public void ShowDensityOverlay(Density t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }
        public void ShowAgeOverlay(AgeGroup t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }
        public void ShowLanguageOverlay(Language t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }
        public void ShowReligionOverlay(Religion t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                Policy policy = Game.LocalPlayerParty.GetPolicy(t);
                int policyImpact = Game.VisibleDistricts[r].GetBaseImpact(policy);
                Color impactColor = GetImpactColor(policyImpact);
                r.SetColor(impactColor);
            }
        }

        /// <summary>
        /// Returns the fitting color for a popularity impact value.
        /// </summary>
        private Color GetImpactColor(int value)
        {
            if (value < 0) return ColorManager.Singleton.NegativeImpactColor;
            if (value == 0) return ColorManager.Singleton.NoImpactColor;
            if (value <= 3) return ColorManager.Singleton.LowImpactColor;
            if (value <= 6) return ColorManager.Singleton.MediumImpactColor;
            if (value <= 9) return ColorManager.Singleton.HighImpactColor;
            else return ColorManager.Singleton.VeryHighImpactColor;
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
}
