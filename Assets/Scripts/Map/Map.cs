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

    public List<Border> Borders = new List<Border>();
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> EdgeBorders = new List<Border>();
    public List<Region> Regions = new List<Region>();
    public List<Landmass> Landmasses = new List<Landmass>();
    public List<WaterBody> WaterBodies = new List<WaterBody>();
    public List<River> Rivers = new List<River>();

    // Display
    public MapDrawMode DrawMode;
    public List<GameObject> LandmassBorders = new List<GameObject>();
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
        IdentifyLandmasses();
        IdentifyWaterBodies();
        InitAdditionalRegionInfo();
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
        }
    }

    private void IdentifyLandmasses()
    {
        // Identify landmasses
        Landmasses.Clear();

        List<Region> regionsWithoutLandmass = new List<Region>();
        regionsWithoutLandmass.AddRange(Regions.Where(x => !x.IsWater));

        while(regionsWithoutLandmass.Count > 0)
        {
            List<Region> landmassRegions = new List<Region>();
            Queue<Region> regionsToAdd = new Queue<Region>();
            regionsToAdd.Enqueue(regionsWithoutLandmass[0]);
            while(regionsToAdd.Count > 0)
            {
                Region regionToAdd = regionsToAdd.Dequeue();
                landmassRegions.Add(regionToAdd);
                foreach (Region neighbourRegion in regionToAdd.AdjacentRegions.Where(x => !x.IsWater))
                    if (!landmassRegions.Contains(neighbourRegion) && !regionsToAdd.Contains(neighbourRegion))
                        regionsToAdd.Enqueue(neighbourRegion);
            }
            string name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
            if (landmassRegions.Count < 5) name += " Island";
            Landmass newLandmass = new Landmass(landmassRegions, name);
            Landmasses.Add(newLandmass);
            foreach (Region r in landmassRegions)
            {
                r.Landmass = newLandmass;
                regionsWithoutLandmass.Remove(r);
            }
        }

        // Create landmass coast border meshes
        GameObject landmassBorderContainer = new GameObject("Landmass Borders");
        landmassBorderContainer.transform.SetParent(RootObject.transform);

        foreach (Landmass lm in Landmasses)
        {
            List<GameObject> curLandmassBorders = MeshGenerator.CreatePolygonGroupBorder(lm.Regions.Select(x => x.Polygon).ToList(), PolygonMapGenerator.DefaultCoastBorderWidth, Color.black, onOutside: true, height: 0.0001f);
            foreach (GameObject border in curLandmassBorders)
            {
                border.transform.SetParent(landmassBorderContainer.transform);
                LandmassBorders.Add(border);
            }
        }
    }

    private void IdentifyWaterBodies()
    {
        WaterBodies.Clear();

        List<Region> regionsWithoutWaterBody = new List<Region>();
        regionsWithoutWaterBody.AddRange(Regions.Where(x => x.IsWater && !x.IsOuterOcean));

        while (regionsWithoutWaterBody.Count > 0)
        {
            List<Region> waterBodyRegions = new List<Region>();
            Queue<Region> regionsToAdd = new Queue<Region>();
            regionsToAdd.Enqueue(regionsWithoutWaterBody[0]);
            while (regionsToAdd.Count > 0)
            {
                Region regionToAdd = regionsToAdd.Dequeue();
                waterBodyRegions.Add(regionToAdd);
                foreach (Region neighbourRegion in regionToAdd.AdjacentRegions.Where(x => x.IsWater && !x.IsOuterOcean))
                    if (!waterBodyRegions.Contains(neighbourRegion) && !regionsToAdd.Contains(neighbourRegion))
                        regionsToAdd.Enqueue(neighbourRegion);
            }
            string name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
            bool isLake;
            if (waterBodyRegions.Count < 5)
            {
                name = "Lake " + name;
                isLake = true;
            }
            else
            {
                name += " Ocean";
                isLake = false;
            }
            WaterBody newWaterBody = new WaterBody(name, waterBodyRegions, isLake);

            // Add outer ocean to ocean
            if(!isLake)
            {
                foreach (Region r in Regions.Where(x => x.IsOuterOcean))
                {
                    newWaterBody.Regions.Add(r);
                    r.WaterBody = newWaterBody;
                }
            }

            WaterBodies.Add(newWaterBody);
            foreach (Region r in waterBodyRegions)
            {
                r.WaterBody = newWaterBody;
                regionsWithoutWaterBody.Remove(r);
            }
        }
    }

    private void InitAdditionalRegionInfo()
    {
        foreach (Region r in Regions) r.InitAdditionalInfo();
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

    public void UpdateDisplay()
    {
        foreach (Region r in Regions) r.UpdateDisplay();
    }

    public void ShowRegionBorders(bool show)
    {
        IsShowingRegionBorders = show;
        foreach (Region r in Regions.Where(x => !x.IsWater)) r.SetShowRegionBorders(show);
    }

    public void ShowShorelineBorders(bool show)
    {
        IsShowingShorelineBorders = show;
        foreach (GameObject landmassBorder in LandmassBorders) landmassBorder.SetActive(show);
    }

    private void FocusMapCentered()
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

    public int NumLandRegions { get { return Regions.Where(x => !x.IsWater).Count(); } }
    public int NumWaterRegions { get { return Regions.Where(x => x.IsWater).Count(); } }
    public float LandArea { get { return Regions.Where(x => !x.IsWater).Sum(x => x.Area); } }
    public float WaterArea { get { return Regions.Where(x => x.IsWater && !x.IsOuterOcean).Sum(x => x.Area); } }
    public int NumLandmasses { get { return Landmasses.Count; } }
    public int NumWaterBodies { get { return WaterBodies.Count; } }

    #endregion
}
