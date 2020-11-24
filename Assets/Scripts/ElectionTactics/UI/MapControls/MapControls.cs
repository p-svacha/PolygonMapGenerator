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

        public GameObject LegendContainer;
        public Text LegendTitleText;
        private Dictionary<Color, string> Legend = new Dictionary<Color, string>();

        private MapDisplayMode DisplayMode;

        private Dictionary<MapDisplayMode, TabButton> ModeButtons = new Dictionary<MapDisplayMode, TabButton>();

        private List<Color> LegendColors = new List<Color>()
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.gray,
            Color.cyan,
            Color.magenta
        };

        private Dictionary<Color, string> NoOverlayLegend = new Dictionary<Color, string>();
        private Dictionary<Color, string> ImpactLegend = new Dictionary<Color, string>();

        private void Start()
        {
            ModeButtons.Add(MapDisplayMode.NoOverlay, NoOverlayButton);
            ModeButtons.Add(MapDisplayMode.Political, PoliticalOverlayButton);
            ModeButtons.Add(MapDisplayMode.Age, AgeOverlayButton);
            ModeButtons.Add(MapDisplayMode.Language, LanguageOverlayButton);
            ModeButtons.Add(MapDisplayMode.Religion, ReligionOverlayButton);

            NoOverlayLegend.Add(ColorManager.Colors.ActiveDistrictColor, "District");
            ImpactLegend.Add(ColorManager.Colors.HighImpactColor, "High");
            ImpactLegend.Add(ColorManager.Colors.MediumImpactColor, "Medium");
            ImpactLegend.Add(ColorManager.Colors.LowImpactColor, "Low");
            ImpactLegend.Add(ColorManager.Colors.NoImpactColor, "None");

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
            if (ActiveTabButton != null) ActiveTabButton.Background.color = ColorManager.Colors.PanelColor;
            ActiveTabButton = ModeButtons[mode];
            ActiveTabButton.Background.color = ColorManager.Colors.HeaderColor;

            // Clear legend
            for (int i = 1; i < LegendContainer.transform.childCount; i++) Destroy(LegendContainer.transform.GetChild(i).gameObject);
            Legend.Clear();

            DisplayMode = mode;
            switch(mode)
            {
                case MapDisplayMode.NoOverlay:
                    LegendTitleText.text = "Districts";
                    foreach (KeyValuePair<Color, string> kvp in NoOverlayLegend) Legend.Add(kvp.Key, kvp.Value);
                    foreach(Region r in Map.LandRegions)
                    {
                        if (Game.Districts.ContainsKey(r)) r.SetColor(ColorManager.Colors.ActiveDistrictColor);
                        else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Political:
                    LegendTitleText.text = "Parties";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.Districts.ContainsKey(r))
                        {
                            if (Game.Districts[r].CurrentWinnerParty != null)
                            {
                                Party winnerParty = Game.Districts[r].CurrentWinnerParty;
                                r.SetColor(winnerParty.Color);
                                if (!Legend.ContainsKey(winnerParty.Color)) Legend.Add(winnerParty.Color, winnerParty.Name);
                            }
                            else r.SetColor(ColorManager.Colors.NoImpactColor);
                        }
                        else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Age:
                    LegendTitleText.text = "Age Groups";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.Districts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.Districts[r].AgeGroup);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Language:
                    LegendTitleText.text = "Languages";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.Districts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.Districts[r].Language);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
                    }
                    break;

                case MapDisplayMode.Religion:
                    LegendTitleText.text = "Religions";
                    foreach (Region r in Map.LandRegions)
                    {
                        if (Game.Districts.ContainsKey(r))
                        {
                            string label = EnumHelper.GetDescription(Game.Districts[r].Religion);
                            HandleLegendEntry(r, label);
                        }
                        else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
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
                Color c = LegendColors[Legend.Count];
                Legend.Add(c, label);
                r.SetColor(c);
            }
            else
            {
                Color c = Legend.First(x => x.Value == label).Key;
                r.SetColor(c);
            }
        }

        public void UpdateMapDisplay()
        {
            SetMapDisplayMode(DisplayMode);
        }

        #endregion

        #region Overlays

        public void ShowGeographyOverlay(GeographyTrait t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (d.Geography.Contains(t)) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
            }
        }
        public void ShowEconomyOverlay(EconomyTrait t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (t == d.Economy1) r.SetColor(ColorManager.Colors.HighImpactColor);
                else if (t == d.Economy2) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else if (t == d.Economy3) r.SetColor(ColorManager.Colors.LowImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
            }
        }
        public void ShowDensityOverlay(Density t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (d.Density == t) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
            }
        }
        public void ShowAgeOverlay(AgeGroup t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (d.AgeGroup == t) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
            }
        }
        public void ShowLanguageOverlay(Language t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (d.Language == t) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
            }
        }
        public void ShowReligionOverlay(Religion t)
        {
            ShowOverlayLegend(EnumHelper.GetDescription(t));
            foreach (Region r in Map.LandRegions.Where(x => Game.Districts.ContainsKey(x)))
            {
                District d = Game.Districts[r];
                if (d.Religion == t) r.SetColor(ColorManager.Colors.MediumImpactColor);
                else r.SetColor(ColorManager.Colors.NoImpactColor);
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