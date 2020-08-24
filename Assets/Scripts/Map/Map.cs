using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Map
{
    public Material SatelliteWaterMaterial;
    public Material PoliticalWaterMaterial;
    public Material SatelliteLandMaterial;
    public Material PoliticalLandMaterial;

    public List<Border> Borders = new List<Border>();
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> EdgeBorders = new List<Border>();
    public List<Region> Regions = new List<Region>();

    public MapDisplayMode DisplayMode;

    public int Width;
    public int Height;

    public Map(Material waterSat, Material waterPol, Material landSat, Material landPol)
    {
        Width = PolygonMapGenerator.MAP_WIDTH;
        Height = PolygonMapGenerator.MAP_HEIGHT;


        SatelliteLandMaterial = landSat;
        PoliticalLandMaterial = landPol;

        SatelliteWaterMaterial = waterSat;
        PoliticalWaterMaterial = waterPol;
    }

    public void HandleInputs()
    {
        // S - Satellite display
        if(Input.GetKeyDown(KeyCode.S))
            SetDisplayMode(MapDisplayMode.Satellite);

        // P - Political display
        if (Input.GetKeyDown(KeyCode.P))
            SetDisplayMode(MapDisplayMode.Political);
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
}
