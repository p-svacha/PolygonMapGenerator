﻿using System.Collections;
using System.Collections.Generic;
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

        public TabButton NoOverlayButton;
        public TabButton PoliticalOverlayButton;
        public TabButton AgeOverlayButton;
        public TabButton LanguageOverlayButton;
        public TabButton ReligionOverlayButton;
        [HideInInspector]
        public TabButton ActiveTabButton;

        [Header("Legend")]
        public GameObject LegendContainer;
        public Text LegendTitleText;
        private Dictionary<Color, string> Legend = new Dictionary<Color, string>();

        private MapDisplayMode DisplayMode;

        private Dictionary<MapDisplayMode, TabButton> ModeButtons = new Dictionary<MapDisplayMode, TabButton>();

        private Dictionary<Color, string> NoOverlayLegend = new Dictionary<Color, string>();
        private Dictionary<Color, string> ImpactLegend = new Dictionary<Color, string>();

        private void Start()
        {
            ModeButtons.Add(MapDisplayMode.NoOverlay, NoOverlayButton);
            ModeButtons.Add(MapDisplayMode.Political, PoliticalOverlayButton);
            ModeButtons.Add(MapDisplayMode.Age, AgeOverlayButton);
            ModeButtons.Add(MapDisplayMode.Language, LanguageOverlayButton);
            ModeButtons.Add(MapDisplayMode.Religion, ReligionOverlayButton);

            NoOverlayLegend.Add(ColorManager.Singleton.InactiveDistrictColor, "District");
            ImpactLegend.Add(ColorManager.Singleton.HighImpactColor, "High");
            ImpactLegend.Add(ColorManager.Singleton.MediumImpactColor, "Medium");
            ImpactLegend.Add(ColorManager.Singleton.LowImpactColor, "Low");
            ImpactLegend.Add(ColorManager.Singleton.NoImpactColor, "None");

            foreach (KeyValuePair<MapDisplayMode, TabButton> kvp in ModeButtons)
                kvp.Value.Button.onClick.AddListener(() => SetMapDisplayMode(kvp.Key));
        }

        public void Init(ElectionTacticsGame game, MapDisplayMode mode)
        {
            Game = game;
            Map = game.Map;
            SetMapDisplayMode(mode);
        }

        #region Display Modes

        public void SetMapDisplayMode(MapDisplayMode mode)
        {
            // Change tab button color
            if (ActiveTabButton != null) ActiveTabButton.Background.color = ColorManager.Singleton.UiMain;
            ActiveTabButton = ModeButtons[mode];
            ActiveTabButton.Background.color = ColorManager.Singleton.UiMain;

            // Clear legend
            for (int i = 1; i < LegendContainer.transform.childCount; i++) Destroy(LegendContainer.transform.GetChild(i).gameObject);
            Legend.Clear();

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

                case MapDisplayMode.Political:
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

                case MapDisplayMode.Age:
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
            }

            // Generate legend
            foreach(KeyValuePair<Color, string> kvp in Legend)
            {
                LegendEntry entry = Instantiate(LegendEntryPrefab, LegendContainer.transform);
                entry.Init(kvp.Key, kvp.Value);
            }
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

        public void RefreshMapDisplay()
        {
            SetMapDisplayMode(DisplayMode);
        }

        #endregion

        #region Overlays

        public void ShowGeographyOverlay(GeographyTraitType t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                GeographyTrait trait = d.Geography.FirstOrDefault(x => x.Type == t);
                if(trait == null) r.SetColor(ColorManager.Singleton.NoImpactColor);
                else if(trait.Category == 3) r.SetColor(ColorManager.Singleton.HighImpactColor);
                else if(trait.Category == 2) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else if(trait.Category == 1) r.SetColor(ColorManager.Singleton.LowImpactColor);
            }
        }
        public void ShowEconomyOverlay(EconomyTrait t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                if (t == d.Economy1) r.SetColor(ColorManager.Singleton.HighImpactColor);
                else if (t == d.Economy2) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else if (t == d.Economy3) r.SetColor(ColorManager.Singleton.LowImpactColor);
                else r.SetColor(ColorManager.Singleton.NoImpactColor);
            }
        }
        public void ShowDensityOverlay(Density t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                if (d.Density == t) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else r.SetColor(ColorManager.Singleton.NoImpactColor);
            }
        }
        public void ShowAgeOverlay(AgeGroup t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                if (d.AgeGroup == t) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else r.SetColor(ColorManager.Singleton.NoImpactColor);
            }
        }
        public void ShowLanguageOverlay(Language t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                if (d.Language == t) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else r.SetColor(ColorManager.Singleton.NoImpactColor);
            }
        }
        public void ShowReligionOverlay(Religion t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.VisibleDistricts.ContainsKey(x)))
            {
                District d = Game.VisibleDistricts[r];
                if (d.Religion == t) r.SetColor(ColorManager.Singleton.MediumImpactColor);
                else r.SetColor(ColorManager.Singleton.NoImpactColor);
            }
        }

        private void ShowOverlayLegend(string title)
        {
            for (int i = 1; i < LegendContainer.transform.childCount; i++) Destroy(LegendContainer.transform.GetChild(i).gameObject);
            Legend.Clear();
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
            SetMapDisplayMode(DisplayMode);
        }

        #endregion
    }

    public enum MapDisplayMode
    {
        NoOverlay,
        Political,
        Age,
        Language,
        Religion
    }
}
