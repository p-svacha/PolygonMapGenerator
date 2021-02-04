using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    public GraphPolygon Polygon;

    // Attributes
    public string Name;
    public float Area;
    public float Jaggedness;
    public float TotalBorderLength;
    public float InlandBorderLength;
    public float InlandBorderRatio; // The ratio is defined as [border type] / total border length
    public float CoastLength;
    public float CoastRatio;
    public float OceanCoastLength;
    public float OceanCoastRatio;
    public float LakeCoastLength;
    public float LakeCoastRatio;

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
    public Vector2 Centroid; // Mathematical average center of polygon
    public Vector2 CenterPoi; // Point of inaccessability - this is the point with the biggest distance to any edge

    // Topography
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> AdjacentRegions = new List<Region>();
    public List<Region> Neighbours = new List<Region>(); // Neighbours are land regions that have a connection to this region
    public List<Region> LandNeighbours = new List<Region>(); // Land neighbours are land regions that are adjacent to this region
    public List<Region> WaterNeighbours = new List<Region>(); // Water neighbours are land regions that share an adjacent water to this region
    public Dictionary<Region, List<Border>> RegionBorders = new Dictionary<Region, List<Border>>(); // This dictionary contains all borders to a specific region
    public Landmass Landmass;
    public WaterBody WaterBody;
    public List<River> Rivers = new List<River>();
    public int DistanceFromNearestWater;

    // Game
    public Nation Nation { get; private set; }

    // Display mode
    private MapDisplayMode DisplayMode;
    private Color Color;
    private bool IsHighlighted;
    private Color HighlightColor; // Highlight color always has top priority when Highlighted is true
    public bool IsBlinking;
    private bool ShowRegionBorders;
    public Texture2D RegionBorderMaskTexture;

    public void Init(GraphPolygon p)
    {
        Polygon = p;
        BorderPoints = p.Nodes.Select(x => x.BorderPoint).ToList();
        Borders = p.Connections.Select(x => x.Border).ToList();
        AdjacentRegions = p.AdjacentPolygons.Select(x => x.Region).ToList();
        LandNeighbours = p.LandNeighbours.Select(x => x.Region).ToList();
        WaterNeighbours = p.WaterNeighbours.Select(x => x.Region).ToList();
        Neighbours = LandNeighbours.Concat(WaterNeighbours).ToList();
        SetRegionBorders();

        Width = p.Width;
        Height = p.Height;
        XPos = p.Nodes.Min(x => x.Vertex.x);
        YPos = p.Nodes.Min(x => x.Vertex.y);
        Centroid = new Vector2(p.Nodes.Average(x => x.Vertex.x), p.Nodes.Average(x => x.Vertex.y));
        CenterPoi = p.CenterPoi;

        Area = p.Area;
        Jaggedness = p.Jaggedness;
        TotalBorderLength = p.TotalBorderLength;
        InlandBorderLength = p.InlandBorderLength;
        InlandBorderRatio = InlandBorderLength / TotalBorderLength;
        CoastLength = p.Coastline;
        CoastRatio = CoastLength / TotalBorderLength;

        IsWater = p.IsWater;
        IsNextToWater = p.IsNextToWater;
        IsEdgeRegion = p.IsEdgePolygon;
        IsOuterOcean = p.IsOuterPolygon;
        DistanceFromNearestWater = p.DistanceFromNearestWater;
    }

    /// <summary>
    /// In here everything can be initialized that relies on additional info from map initialization, such as landmass and waterbody info
    /// </summary>
    public void InitAdditionalInfo()
    {
        OceanCoastLength = RegionBorders.Where(x => x.Key.WaterBody != null && !x.Key.WaterBody.IsLake).Sum(x => x.Value.Sum(y => y.Length));
        OceanCoastRatio = OceanCoastLength / TotalBorderLength;
        LakeCoastLength = RegionBorders.Where(x => x.Key.WaterBody != null && x.Key.WaterBody.IsLake).Sum(x => x.Value.Sum(y => y.Length));
        LakeCoastRatio = LakeCoastLength / TotalBorderLength;
    }

    private void SetRegionBorders()
    {
        RegionBorders.Clear();
        foreach (Region r in AdjacentRegions) RegionBorders.Add(r, new List<Border>());
        foreach(Border b in Borders)
        {
            foreach (Region r in AdjacentRegions)
            {
                if (b.Regions.Contains(r))
                {
                    RegionBorders[r].Add(b);
                    break;
                }
            }
        }
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

        if (IsHighlighted) GetComponent<Renderer>().material.color = HighlightColor;
        else GetComponent<Renderer>().material.color = Color;

        if (ShowRegionBorders) GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", RegionBorderMaskTexture);
        else GetComponent<MeshRenderer>().material.SetTexture("_BorderMask", Texture2D.blackTexture);

        GetComponent<MeshRenderer>().material.SetFloat("_Blink", IsBlinking ? 1 : 0);
    }

    public void SetBlinking(bool b)
    {
        IsBlinking = b;
        UpdateDisplayMode();
    }

    public void Highlight(Color highlightColor)
    {
        IsHighlighted = true;
        HighlightColor = highlightColor;
        UpdateDisplayMode();
    }

    public void Unhighlight()
    {
        IsHighlighted = false;
        UpdateDisplayMode();
    }

    public void SetColor(Color c)
    {
        Color = c;
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
