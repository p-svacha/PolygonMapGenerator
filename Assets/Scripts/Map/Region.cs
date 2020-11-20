using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    public GraphPolygon Polygon;

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
    private bool Highlighted;
    private bool ShowRegionBorders;
    private bool IsGreyedOut;
    public Texture2D RegionBorderMaskTexture;

    public void Init(GraphPolygon p)
    {
        Polygon = p;
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
                    GetComponent<Renderer>().material = MaterialHandler.Materials.TopographicWaterMaterial;
                    GetComponent<MeshRenderer>().material.color = MaterialHandler.Materials.WaterColor;
                }
                else
                {
                    GetComponent<Renderer>().material = MaterialHandler.Materials.TopographicLandMaterial;
                    GetComponent<MeshRenderer>().material.color = MaterialHandler.Materials.LandColor;
                }
                break;

            case MapDisplayMode.Political:
                if (IsWater) GetComponent<Renderer>().material = MaterialHandler.Materials.PoliticalWaterMaterial;
                else
                {
                    GetComponent<Renderer>().material = MaterialHandler.Materials.PoliticalLandMaterial;
                    GetComponent<MeshRenderer>().material.color = Nation == null ? MaterialHandler.Materials.LandColor : Nation.PrimaryColor;
                }
                break;

            case MapDisplayMode.Satellite:
                if (IsWater) GetComponent<Renderer>().material = MaterialHandler.Materials.SatelliteWaterMaterial;
                else GetComponent<Renderer>().material = MaterialHandler.Materials.SatelliteLandMaterial;
                break;
        }
        if (IsGreyedOut) GetComponent<Renderer>().material.color = MaterialHandler.Materials.GreyedOutColor;
        if (Highlighted) GetComponent<Renderer>().material.color = MaterialHandler.Materials.HighlightColor;
        if (ShowRegionBorders) GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", RegionBorderMaskTexture);
        else GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", Texture2D.blackTexture);
    }

    public void SetHighlighted(bool b)
    {
        Highlighted = b;
        UpdateDisplayMode();
    }

    public void SetGreyedOut(bool b)
    {
        IsGreyedOut = b;
        UpdateDisplayMode();
    }

    public void SetShowRegionBorders(bool b)
    {
        ShowRegionBorders = b;
        UpdateDisplayMode();
    }

    #region Getters

    public float MinWorldX { get { return BorderPoints.Min(x => x.Position.x); } }
    public float MinWorldY { get { return BorderPoints.Min(x => x.Position.y); } }
    public float MaxWorldX { get { return BorderPoints.Max(x => x.Position.x); } }
    public float MaxWorldY { get { return BorderPoints.Max(x => x.Position.y); } }

    #endregion
}
