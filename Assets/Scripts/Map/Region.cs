using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    public string Name;

    public float Width;
    public float Height;
    public float Area;
    public float Jaggedness;

    public bool IsWater;
    public bool IsNextToWater;

    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> NeighbouringRegions = new List<Region>();
    public Landmass Landmass;
    public List<River> Rivers = new List<River>();

    public Material SatelliteWaterMaterial;
    public Material PoliticalWaterMaterial;
    public Material SatelliteLandMaterial;
    public Material PoliticalLandMaterial;

    public void Init(GraphPolygon p, Material politicalWater, Material satelliteWater, Material landMaterial, Material politicalMaterial)
    {
        BorderPoints = p.Nodes.Select(x => x.BorderPoint).ToList();
        Borders = p.Connections.Select(x => x.Border).ToList();
        NeighbouringRegions = p.Neighbours.Select(x => x.Region).ToList();
        Width = p.Width;
        Height = p.Height;
        Area = p.Area;
        Jaggedness = p.Jaggedness;
        IsWater = p.IsWater;
        IsNextToWater = p.IsNextToWater;

        SatelliteLandMaterial = landMaterial;
        SatelliteWaterMaterial = satelliteWater;
        PoliticalLandMaterial = politicalMaterial;
        PoliticalWaterMaterial = politicalWater;
    }

    public void TurnToWater()
    {
        IsWater = true;
    }

    public void SetDisplayMode(MapDisplayMode mode)
    {
        switch(mode)
        {
            case MapDisplayMode.Political:
                if(IsWater) GetComponent<Renderer>().material = PoliticalWaterMaterial;
                else GetComponent<Renderer>().material = PoliticalLandMaterial;
                break;

            case MapDisplayMode.Satellite:
                if (IsWater) GetComponent<Renderer>().material = SatelliteWaterMaterial;
                else GetComponent<Renderer>().material = SatelliteLandMaterial;
                break;
        }
    }
}
