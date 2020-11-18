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
    public int DistanceFromNearestWater;

    // Game
    public Nation Nation { get; private set; }

    // Display mode
    private MapDisplayMode DisplayMode;
    private MaterialHandler MaterialHandler;

    public void Init(GraphPolygon p)
    {
        MaterialHandler = GameObject.Find("MaterialHandler").GetComponent<MaterialHandler>();

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
        DistanceFromNearestWater = p.DistanceFromNearestWater;
    }

    public void SetNation(Nation n)
    {
        Nation = n;
        UpdateDisplayMode();
    }

    public void TurnToWater()
    {
        IsWater = true;
    }

    public void SetDisplayMode(MapDisplayMode mode)
    {
        DisplayMode = mode;
        UpdateDisplayMode();
    }

    private void UpdateDisplayMode()
    {
        switch (DisplayMode)
        {
            case MapDisplayMode.Topographic:
                if (IsWater)
                {
                    GetComponent<Renderer>().material = MaterialHandler.TopographicWaterMaterial;
                    GetComponent<MeshRenderer>().material.color = MaterialHandler.WaterColor;
                }
                else
                {
                    GetComponent<Renderer>().material = MaterialHandler.TopographicLandMaterial;
                    GetComponent<MeshRenderer>().material.color = MaterialHandler.LandColor;
                }
                break;

            case MapDisplayMode.Political:
                if (IsWater) GetComponent<Renderer>().material = MaterialHandler.PoliticalWaterMaterial;
                else
                {
                    GetComponent<Renderer>().material = MaterialHandler.PoliticalLandMaterial;
                    GetComponent<MeshRenderer>().material.color = Nation == null ? MaterialHandler.LandColor : Nation.PrimaryColor;
                }
                break;

            case MapDisplayMode.Satellite:
                if (IsWater) GetComponent<Renderer>().material = MaterialHandler.SatelliteWaterMaterial;
                else GetComponent<Renderer>().material = MaterialHandler.SatelliteLandMaterial;
                break;
        }
    }
}
