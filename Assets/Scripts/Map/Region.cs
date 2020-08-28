using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    public string Name;

   
    public float Area;
    public float Jaggedness;

    public bool IsEdgeRegion;
    public bool IsOuterOcean;
    public bool IsWater;
    public bool IsNextToWater;
    public RegionType Type;

    // Bounds
    public float XPos;
    public float YPos;
    public float Width;
    public float Height;

    // Topography
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> NeighbouringRegions = new List<Region>();
    public Landmass Landmass;
    public WaterBody WaterBody;
    public List<River> Rivers = new List<River>();

    // Game
    public Nation Nation { get; private set; }

    public Material SatelliteWaterMaterial;
    public Material PoliticalWaterMaterial;
    public Material SatelliteLandMaterial;
    public Material PoliticalLandMaterial;
    public Color UnownedLandColor;

    public void Init(GraphPolygon p, Material politicalWater, Material satelliteWater, Material landMaterial, Material politicalMaterial, Color unownedColor)
    {
        BorderPoints = p.Nodes.Select(x => x.BorderPoint).ToList();
        Borders = p.Connections.Select(x => x.Border).ToList();
        NeighbouringRegions = p.Neighbours.Select(x => x.Region).ToList();
        Width = p.Width;
        Height = p.Height;
        XPos = p.Nodes.Min(x => x.Vertex.x);
        YPos = p.Nodes.Min(x => x.Vertex.y);
        Area = p.Area;
        Jaggedness = p.Jaggedness;
        IsWater = p.IsWater;
        IsNextToWater = p.IsNextToWater;
        IsEdgeRegion = p.IsEdgePolygon;
        IsOuterOcean = p.IsOuterPolygon;

        SatelliteLandMaterial = landMaterial;
        SatelliteWaterMaterial = satelliteWater;
        PoliticalLandMaterial = politicalMaterial;
        PoliticalWaterMaterial = politicalWater;
        UnownedLandColor = unownedColor;
    }

    public void SetNation(Nation n)
    {
        Nation = n;
        GetComponent<MeshRenderer>().material.color = n == null ? UnownedLandColor : n.PrimaryColor;
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

    // Returns the camera position that it is zoomed to this region
    public Vector3 GetCameraPosition()
    {
        return new Vector3(XPos + Width / 2f, Math.Max(Width, Height) * 1.5f, YPos + Height / 2f);
    }
}
