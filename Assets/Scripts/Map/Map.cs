using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map
{
    public Texture2D RegionBorderMaskTexture;

    public GameObject RootObject;
    public List<Border> Borders = new List<Border>();
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> EdgeBorders = new List<Border>();
    public List<Region> Regions = new List<Region>();
    public List<Landmass> Landmasses = new List<Landmass>();
    public List<WaterBody> WaterBodies = new List<WaterBody>();
    public List<River> Rivers = new List<River>();

    // Display Mode
    public bool IsShowingRegionBorders;

    public int Width;
    public int Height;
    public int Area;

    public Map(PolygonMapGenerator PMG)
    {
        Width = PMG.Width;
        Height = PMG.Height;
        Area = Width * Height;

        //MaterialHandler.PoliticalLandMaterial.SetTexture("_RiverMask", TextureGenerator.CreateRiverMaskTexture(PMG));
        //MaterialHandler.TopographicLandMaterial.SetTexture("_RiverMask", TextureGenerator.CreateRiverMaskTexture(PMG));
        //RegionBorderMaskTexture = TextureGenerator.CreateRegionBorderMaskTexture(PMG);
    }

    public void InitializeMap(PolygonMapGenerator PMG, bool showRegionBorders, Color landColor, Color waterColor)
    {
        InitRivers(PMG);
        IdentifyLandmasses();
        IdentifyWaterBodies();
        InitAdditionalRegionInfo();
        FocusMapInEditor();
        InitDisplay(showRegionBorders, landColor, waterColor);
    }

    private void InitDisplay(bool showRegionBorders, Color landColor, Color waterColor)
    {
        foreach (Region r in Regions)
        {
            if (!r.IsWater) r.SetColor(landColor);
            else r.SetColor(waterColor);
        }
        ShowRegionBorders(showRegionBorders);
    }

    private void InitRivers(PolygonMapGenerator PMG)
    {
        foreach (GraphPath r in PMG.RiverPaths)
        {
            Rivers.Add(RiverCreator.CreateRiverObject(r, PMG));
        }
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
        foreach (Border b in Borders) b.gameObject.SetActive(!b.gameObject.activeSelf);
        foreach (Border b in EdgeBorders) b.gameObject.SetActive(!b.gameObject.activeSelf);
    }

    public void ToggleHideBorderPoints()
    {
        foreach (BorderPoint bp in BorderPoints) bp.gameObject.SetActive(!bp.gameObject.activeSelf);
    }

    public void UpdateDisplay()
    {
        foreach (Region r in Regions) r.UpdateDisplay();
        //foreach (River r in Rivers) r.SetDisplayMode(mode);
    }

    public void ShowRegionBorders(bool show)
    {
        IsShowingRegionBorders = show;
        foreach (Region r in Regions.Where(x => !x.IsWater)) r.SetShowRegionBorders(show);
    }

    private void FocusMapCentered()
    {
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.position = new Vector3(Width / 2f, Height, Height / 2f);
    }

    private void FocusMapInEditor()
    {
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.position = new Vector3(Width * 0.7f, Height * 0.9f, Height * 0.5f);
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
