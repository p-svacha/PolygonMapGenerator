using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map
{
    public Material SatelliteWaterMaterial;
    public Material PoliticalWaterMaterial;
    public Material SatelliteLandMaterial;
    public Material PoliticalLandMaterial;
    public Texture RegionBorderMaskTexture;

    public MarkovChainWordGenerator NameGenerator;

    public List<Border> Borders = new List<Border>();
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> EdgeBorders = new List<Border>();
    public List<Region> Regions = new List<Region>();
    public List<Landmass> Landmasses = new List<Landmass>();

    // Display Mode
    public MapDisplayMode DisplayMode;
    public bool IsShowingRegionBorders;

    public int Width;
    public int Height;

    public Map(PolygonMapGenerator PMG)
    {
        Width = PMG.Width;
        Height = PMG.Height;

        SatelliteLandMaterial = PMG.SatelliteLandMaterial;
        PoliticalLandMaterial = PMG.PoliticalLandMaterial;

        SatelliteWaterMaterial = PMG.SatelliteWaterMaterial;
        PoliticalWaterMaterial = PMG.PoliticalWaterMaterial;

        NameGenerator = new MarkovChainWordGenerator();

        RegionBorderMaskTexture = TextureGenerator.CreateRegionBorderTexture(PMG);
    }

    public void InitializeMap()
    {
        IdentifyLandmasses();
    }

    private void IdentifyLandmasses()
    {
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
                foreach (Region neighbourRegion in regionToAdd.NeighbouringRegions.Where(x => !x.IsWater))
                    if (!landmassRegions.Contains(neighbourRegion) && !regionsToAdd.Contains(neighbourRegion))
                        regionsToAdd.Enqueue(neighbourRegion);
            }
            string name = GetRandomProvinceName();
            if (landmassRegions.Count < 5) name += " Island";
            Landmass newLandmass = new Landmass(landmassRegions, name);
            Landmasses.Add(newLandmass);
            foreach (Region r in landmassRegions)
            {
                r.Landmass = newLandmass;
                regionsWithoutLandmass.Remove(r);
            }
        }
    }

    public void HandleInputs()
    {
        // S - Satellite display
        if(Input.GetKeyDown(KeyCode.S))
            SetDisplayMode(MapDisplayMode.Satellite);

        // P - Political display
        if (Input.GetKeyDown(KeyCode.P))
            SetDisplayMode(MapDisplayMode.Political);

        // R - Show region borders
        if (Input.GetKeyDown(KeyCode.R) && DisplayMode == MapDisplayMode.Political)
            ToggleRegionBorders();
    }

    public void DestroyAllGameObjects()
    {
        foreach (Border b in Borders) GameObject.Destroy(b.gameObject);
        foreach (Border b in EdgeBorders) GameObject.Destroy(b.gameObject);
        foreach (BorderPoint bp in BorderPoints) GameObject.Destroy(bp.gameObject);
        foreach (Region r in Regions) GameObject.Destroy(r.gameObject);
    }

    public void ToggleHideBorders()
    {
        foreach (Border b in Borders) b.GetComponent<Renderer>().enabled = !b.GetComponent<Renderer>().enabled;
        foreach (Border b in EdgeBorders) b.GetComponent<Renderer>().enabled = !b.GetComponent<Renderer>().enabled;
    }

    public void ToggleHideBorderPoints()
    {
        foreach (BorderPoint bp in BorderPoints) bp.GetComponent<Renderer>().enabled = !bp.GetComponent<Renderer>().enabled;
    }

    public void SetDisplayMode(MapDisplayMode mode)
    {
        DisplayMode = mode;
        foreach (Region r in Regions) r.SetDisplayMode(mode);
    }

    public void ToggleRegionBorders()
    {
        if(IsShowingRegionBorders)
        {
            IsShowingRegionBorders = false;
            PoliticalLandMaterial.SetTexture("_Mask", Texture2D.blackTexture);
        }
        else
        {
            IsShowingRegionBorders = true;
            PoliticalLandMaterial.SetTexture("_Mask", RegionBorderMaskTexture);
        }
    }

    private string GetRandomProvinceName(int maxLength = 16)
    {
        string name = NameGenerator.GenerateWord("Province", 4);
        while(name.Length > maxLength) name = NameGenerator.GenerateWord("Province", 4);
        return name;
    }
}
