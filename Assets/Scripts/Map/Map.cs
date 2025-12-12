using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject WaterConnectionContainer;

    public List<Border> Borders;
    public List<BorderPoint> BorderPoints;
    public List<Border> EdgeBorders;

    public List<Region> Regions;
    public List<Landmass> Landmasses;
    public List<WaterBody> WaterBodies;
    public List<Continent> Continents;
    public List<River> Rivers;
    public List<WaterConnection> WaterConnections;

    // Display
    public MapColorMode ColorMode;
    public MapTextureMode TextureMode;
    public bool IsShowingRegionBorders;
    public bool IsShowingShorelineBorders;
    public bool IsShowingContinentBorders;
    public bool IsShowingWaterConnections;

    public Map(MapGenerationSettings settings)
    {
        Attributes = settings;
    }

    /// <summary>
    /// This function always has to be called after the map is received from the map generator
    /// </summary>
    public void InitializeMap(bool showRegionBorders, bool showShorelineBorders, bool showContinentBorders, bool showWaterConnections, MapColorMode drawMode, MapTextureMode textureMode)
    {
        UpdateColorMode(drawMode);
        UpdateTextureMode(textureMode);
        ShowRegionBorders(showRegionBorders);
        ShowShorelineBorders(showShorelineBorders);
        ShowContinentBorders(showContinentBorders);
        ShowWaterConnections(showWaterConnections);
        FocusMapInEditor();
    }

    public void DestroyAllGameObjects()
    {
        GameObject.Destroy(RootObject.gameObject);
    }

    #region Display

    /// <summary>
    /// Updates the colors of all regions according to the specified mode.
    /// </summary>
    public void UpdateColorMode(MapColorMode drawMode)
    {
        ColorMode = drawMode;

        switch(ColorMode)
        {
            case MapColorMode.White:
                foreach (Region r in Regions) r.SetColor(Color.white);
                foreach (River r in Rivers) r.SetColor(Color.white);
                break;

            case MapColorMode.Basic:
                foreach (Region r in Regions)
                {
                    if (r.IsWater) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                    else r.SetColor(MapDisplayResources.Singleton.LandColor);
                }
                foreach (River r in Rivers) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                break;

            case MapColorMode.Biomes:
                foreach (Region r in Regions)
                {
                    if (r.IsWater) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                    else r.SetColor(MapDisplayResources.Singleton.GetBiomeColor(r.Biome));
                }
                foreach (River r in Rivers) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                break;

            case MapColorMode.Continents:
                foreach (Region r in WaterRegions) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                foreach (River r in Rivers) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                List<Color> continentColors = new List<Color>();
                foreach(Continent continent in Continents)
                {
                    Color continentColor = ColorManager.GetRandomDistinctColor(continentColors);
                    foreach (Region r in continent.Regions) r.SetColor(continentColor);
                    continentColors.Add(continentColor);
                }
                break;

            case MapColorMode.ParriskBoard:
                foreach (Region r in Regions)
                {
                    if(r.IsOuterOcean)
                    {
                        r.SetColor(Color.black);
                    }
                    else if (r.IsWater)
                    {
                        r.SetColor(Color.white);
                        r.SetTexture(MapDisplayResources.Singleton.WaterBackgroundTexture);
                    }
                    else r.SetColor(Color.white);
                }
                foreach (River r in Rivers) r.SetColor(MapDisplayResources.Singleton.WaterColor);
                break;
        }
    }

    /// <summary>
    /// Updates the textures of all regions according to the specified mode.
    /// </summary>
    public void UpdateTextureMode(MapTextureMode mode)
    {
        TextureMode = mode;

        switch(TextureMode)
        {
            case MapTextureMode.None:
                foreach (Region r in Regions) r.SetTexture(null);
                foreach (River r in Rivers) r.SetTexture(null);
                break;

            case MapTextureMode.BiomeTextures:
                foreach (Region r in Regions)
                {
                    if (r.IsWater) r.SetTexture(MapDisplayResources.Singleton.WaterTexture);
                    else r.SetTexture(MapDisplayResources.Singleton.GetBiomeTexture(r.Biome));
                }
                foreach (River r in Rivers) r.SetColor(Color.white);
                break;

            case MapTextureMode.MinorNoise:
                foreach (Region r in Regions) r.SetTexture(MapDisplayResources.Singleton.MinorNoiseTexture);
                foreach (River r in Rivers) r.SetTexture(MapDisplayResources.Singleton.MinorNoiseTexture);
                break;
        }
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

    public void ShowContinentBorders(bool show)
    {
        IsShowingContinentBorders = show;
        foreach (Continent continent in Continents) continent.ShowBorders(show);
    }

    public void ShowWaterConnections(bool show)
    {
        IsShowingWaterConnections = show;
        foreach (WaterConnection wc in WaterConnections) wc.SetVisible(show);
    }

    #endregion

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
    public int NumContinents { get { return Continents.Count; } }

    #endregion
}
