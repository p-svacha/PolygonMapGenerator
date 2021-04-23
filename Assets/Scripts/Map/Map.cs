using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map
{
    public MapGenerationSettings Attributes;

    public GameObject RootObject;
    public GameObject BorderPointContainer;
    public GameObject BorderContainer;
    public GameObject RegionContainer;
    public GameObject RiverContainer;
    public GameObject ContinentContainer;

    public List<Border> Borders;
    public List<BorderPoint> BorderPoints;
    public List<Border> EdgeBorders;

    public List<Region> Regions;
    public List<Landmass> Landmasses;
    public List<WaterBody> WaterBodies;
    public List<Continent> Continents;
    public List<River> Rivers;

    // Display
    public MapDrawMode DrawMode;
    public bool IsShowingRegionBorders;
    public bool IsShowingShorelineBorders;

    public Map(MapGenerationSettings settings)
    {
        Attributes = settings;
    }

    /// <summary>
    /// This function always has to be called after the map is received from the map generator
    /// </summary>
    public void InitializeMap(bool showRegionBorders, bool showShorelineBorders, MapDrawMode drawMode)
    {
        UpdateDrawMode(drawMode);
        ShowRegionBorders(showRegionBorders);
        ShowShorelineBorders(showShorelineBorders);
        FocusMapInEditor();
    }

    public void UpdateDrawMode(MapDrawMode drawMode)
    {
        DrawMode = drawMode;

        switch(DrawMode)
        {
            case MapDrawMode.Basic:
                foreach (Region r in Regions)
                {
                    if (r.IsWater) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                    else r.SetColor(MapDisplaySettings.Settings.LandColor);
                }
                foreach (River r in Rivers) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                break;

            case MapDrawMode.Biomes:
                foreach (Region r in Regions)
                {
                    if (r.IsWater) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                    else r.SetColor(MapDisplaySettings.Settings.GetBiomeColor(r.Biome));
                }
                foreach (River r in Rivers) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                break;

            case MapDrawMode.Continents:
                foreach (Region r in WaterRegions) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                foreach (River r in Rivers) r.SetColor(MapDisplaySettings.Settings.WaterColor);
                List<Color> continentColors = new List<Color>();
                foreach(Continent continent in Continents)
                {
                    Color continentColor = ColorManager.GetRandomDistinctColor(continentColors);
                    foreach (Region r in continent.Regions) r.SetColor(continentColor);
                    continentColors.Add(continentColor);
                }
                break;
        }
    }

    public void DestroyAllGameObjects()
    {
        GameObject.Destroy(RootObject.gameObject);
    }

    public void ToggleHideBorders()
    {
        BorderContainer.SetActive(!BorderContainer.gameObject.activeSelf);
    }

    public void ToggleHideBorderPoints()
    {
        BorderPointContainer.SetActive(!BorderPointContainer.gameObject.activeSelf);
    }

    public void ShowRegionBorders(bool show)
    {
        IsShowingRegionBorders = show;
        foreach (Region r in Regions.Where(x => !x.IsWater)) r.SetShowRegionBorders(show);
    }

    public void ShowShorelineBorders(bool show)
    {
        IsShowingShorelineBorders = show;
        foreach (Landmass landmass in Landmasses) landmass.ShowBorders(show);
    }

    public void FocusMapCentered()
    {
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.position = new Vector3(Attributes.Width / 2f, Attributes.Height, Attributes.Height / 2f);
    }

    private void FocusMapInEditor()
    {
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.position = new Vector3(Attributes.Width * 0.7f, Attributes.Height * 0.9f, Attributes.Height * 0.5f);
    }

    #region Getters

    public List<Region> LandRegions { get { return Regions.Where(x => !x.IsWater).ToList(); } }
    public List<Region> WaterRegions { get { return Regions.Where(x => x.IsWater).ToList(); } }

    public int NumLandRegions { get { return Regions.Where(x => !x.IsWater).Count(); } }
    public int NumWaterRegions { get { return Regions.Where(x => x.IsWater).Count(); } }
    public float LandArea { get { return Regions.Where(x => !x.IsWater).Sum(x => x.Area); } }
    public float WaterArea { get { return Regions.Where(x => x.IsWater && !x.IsOuterOcean).Sum(x => x.Area); } }
    public int NumLandmasses { get { return Landmasses.Count; } }
    public int NumWaterBodies { get { return WaterBodies.Count; } }

    #endregion
}
