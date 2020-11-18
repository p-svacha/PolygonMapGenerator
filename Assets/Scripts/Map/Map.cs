using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map
{
    public MaterialHandler MaterialHandler;
    public Texture2D RegionBorderMaskTexture;

    public List<Border> Borders = new List<Border>();
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> EdgeBorders = new List<Border>();
    public List<Region> Regions = new List<Region>();
    public List<Landmass> Landmasses = new List<Landmass>();
    public List<WaterBody> WaterBodies = new List<WaterBody>();
    public List<River> Rivers = new List<River>();

    // Display Mode
    public MapDisplayMode DisplayMode;
    public bool IsShowingRegionBorders;

    public int Width;
    public int Height;

    public Map(PolygonMapGenerator PMG)
    {
        MaterialHandler = GameObject.Find("MaterialHandler").GetComponent<MaterialHandler>();

        Width = PMG.Width;
        Height = PMG.Height;

        MaterialHandler.PoliticalLandMaterial.SetTexture("_RiverMask", TextureGenerator.CreateRiverMaskTexture(PMG));
        MaterialHandler.TopographicLandMaterial.SetTexture("_RiverMask", TextureGenerator.CreateRiverMaskTexture(PMG));
        RegionBorderMaskTexture = TextureGenerator.CreateRegionBorderMaskTexture(PMG);
    }

    public void InitializeMap(PolygonMapGenerator PMG)
    {
        InitRivers(PMG);
        IdentifyLandmasses();
        IdentifyWaterBodies();
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
                foreach (Region neighbourRegion in regionToAdd.NeighbouringRegions.Where(x => !x.IsWater))
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
                foreach (Region neighbourRegion in regionToAdd.NeighbouringRegions.Where(x => x.IsWater && !x.IsOuterOcean))
                    if (!waterBodyRegions.Contains(neighbourRegion) && !regionsToAdd.Contains(neighbourRegion))
                        regionsToAdd.Enqueue(neighbourRegion);
            }
            string name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
            bool saltWater = false;
            if(waterBodyRegions.Any(x => x.IsEdgeRegion))
            {
                name += " Ocean";
                saltWater = true;
            }
            else name = "Lake " + name;
            WaterBody newWaterBody = new WaterBody(name, waterBodyRegions, saltWater);

            // Add outer ocean to ocean
            if(saltWater)
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

    public void HandleInputs()
    {
        // S - Satellite display
        if(Input.GetKeyDown(KeyCode.S))
            SetDisplayMode(MapDisplayMode.Satellite);

        // P - Political display
        if (Input.GetKeyDown(KeyCode.P))
            SetDisplayMode(MapDisplayMode.Political);

        // R - Show region borders
        if (Input.GetKeyDown(KeyCode.R) && (DisplayMode == MapDisplayMode.Political || DisplayMode == MapDisplayMode.Topographic))
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
        foreach (River r in Rivers) r.SetDisplayMode(mode);
    }

    public void ToggleRegionBorders()
    {
        if(IsShowingRegionBorders)
        {
            IsShowingRegionBorders = false;
            foreach(Region r in Regions.Where(x => !x.IsWater)) r.GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", Texture2D.blackTexture);

        }
        else
        {
            IsShowingRegionBorders = true;
            foreach (Region r in Regions.Where(x => !x.IsWater)) r.GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", RegionBorderMaskTexture);
        }
    }
}
