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

    // Climate
    public int Temperature;
    public int Precipitation;
    public Biome Biome;

    // Topography
    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> AdjacentRegions = new List<Region>();
    public List<Region> Neighbours = new List<Region>(); // Neighbours are land regions that have a connection to this region
    public List<Region> LandNeighbours = new List<Region>(); // Land neighbours are land regions that are adjacent to this region
    public List<Region> WaterNeighbours = new List<Region>(); // Water neighbours are land regions that share an adjacent water to this region
    public Dictionary<Region, List<Border>> RegionBorders = new Dictionary<Region, List<Border>>(); // This dictionary contains all borders to a specific region
    public List<WaterConnection> WaterConnections = new List<WaterConnection>(); // List containing all water connection objects (1 per water neighbour)

    public Continent Continent;
    public Landmass Landmass;
    public WaterBody WaterBody;
    public List<River> Rivers = new List<River>();

    public int DistanceFromNearestWater;

    // Game
    public Nation Nation { get; private set; }

    // Display
    private Color Color;
    private Texture2D Texture;

    private bool IsHighlighted;
    private Color HighlightColor; // HighlightColor overrides Color if IsHighlighted is true
    public bool IsBlinking;
    public bool IsAnimatedHighlighted;
    private bool ShowRegionBorders;
    public GameObject Border;

    private List<GameObject> ConnectionOverlays;

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
        Centroid = p.Centroid;
        CenterPoi = p.CenterPoi;

        Area = p.Area;
        Jaggedness = p.Jaggedness;
        TotalBorderLength = p.TotalBorderLength;
        InlandBorderLength = p.InlandBorderLength;
        InlandBorderRatio = InlandBorderLength / TotalBorderLength;
        CoastLength = p.Coastline;
        CoastRatio = CoastLength / TotalBorderLength;

        Temperature = p.Temperature;
        Precipitation = p.Precipitation;
        Biome = p.Biome;

        IsWater = p.IsWater;
        IsNextToWater = p.IsNextToWater;
        IsEdgeRegion = p.IsEdgePolygon;
        IsOuterOcean = p.IsOuterPolygon;
        DistanceFromNearestWater = p.DistanceFromNearestWater;

        GetComponent<Renderer>().material = MapDisplaySettings.Settings.DefaultMaterial;

        // Coast
        OceanCoastLength = RegionBorders.Where(x => x.Key.WaterBody != null && !x.Key.WaterBody.IsLake).Sum(x => x.Value.Sum(y => y.Length));
        OceanCoastRatio = OceanCoastLength / TotalBorderLength;
        LakeCoastLength = RegionBorders.Where(x => x.Key.WaterBody != null && x.Key.WaterBody.IsLake).Sum(x => x.Value.Sum(y => y.Length));
        LakeCoastRatio = LakeCoastLength / TotalBorderLength;

        // Border surrounding the region
        Border = MeshGenerator.CreateSinglePolygonBorder(p.Nodes, PolygonMapGenerator.DefaultRegionBorderWidth, Color.black, layer: PolygonMapGenerator.LAYER_REGION_BORDER);
        Border.transform.SetParent(transform);

        // Connection overlays (lines to neighbouring regions)
        ConnectionOverlays = InitConnectionOverlays();
        ShowConnectionOverlays(false);
    }

    private List<GameObject> InitConnectionOverlays()
    {
        List<GameObject> connections = new List<GameObject>();
        foreach (GraphPolygon neighbour in Polygon.LandNeighbours)
        {
            GameObject line = MeshGenerator.DrawLine(CenterPoi, neighbour.CenterPoi, 0.015f, Color.red);
            line.transform.SetParent(transform);
            connections.Add(line);
        }
        foreach (GraphPolygon neighbour in Polygon.WaterNeighbours)
        {
            GameObject line = MeshGenerator.DrawLine(CenterPoi, neighbour.CenterPoi, 0.015f, new Color(0.5f, 0.1f, 0.8f));
            line.transform.SetParent(transform);
            connections.Add(line);
        }
        return connections;
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
        UpdateDisplay();
    }

    public void TurnToWater()
    {
        IsWater = true;
    }

    public void UpdateDisplay()
    {
        if (IsHighlighted) GetComponent<MeshRenderer>().material.SetColor("_Color", HighlightColor);
        else GetComponent<MeshRenderer>().material.SetColor("_Color", Color);

        if(Texture != null) GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture);
        else GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture2D.whiteTexture);


        Border.SetActive(ShowRegionBorders);

        GetComponent<MeshRenderer>().material.SetFloat("_Blink", IsBlinking ? 1 : 0);
        GetComponent<MeshRenderer>().material.SetFloat("_AnimatedHighlight", IsAnimatedHighlighted ? 1 : 0);
    }

    public void SetBlinking(bool b)
    {
        IsBlinking = b;
        UpdateDisplay();
    }

    public void SetAnimatedHighlight(bool b)
    {
        IsAnimatedHighlighted = b;
        UpdateDisplay();
    }

    public void Highlight(Color highlightColor)
    {
        IsHighlighted = true;
        HighlightColor = highlightColor;
        UpdateDisplay();
    }

    public void Unhighlight()
    {
        IsHighlighted = false;
        UpdateDisplay();
    }

    public void SetColor(Color c)
    {
        Color = c;
        UpdateDisplay();
    }

    public void SetTexture(Texture2D texture)
    {
        Texture = texture;
        UpdateDisplay();
    }

    public void SetShowRegionBorders(bool b)
    {
        ShowRegionBorders = b;
        UpdateDisplay();
    }

    public void ShowConnectionOverlays(bool show)
    {
        foreach (GameObject overlay in ConnectionOverlays) overlay.SetActive(show);
    }

    #region Getters

    /// <summary>
    /// Returns the middle point and angle of a border to an adjacent region. The angle always points to the direction of this region.
    /// </summary>
    public Tuple<Vector2, float> GetBorderCenterPositionTo(Region otherRegion)
    {
        if (!AdjacentRegions.Contains(otherRegion)) throw new Exception("Other region is not adjacent to this region");

        // Find all borders between the two regions
        List<Border> borders = Borders.Where(x => x.Regions.Contains(otherRegion)).ToList();

        // Split borders into clusters and take longest cluster
        List<List<Border>> clusters = PolygonMapFunctions.FindBorderClusters(borders);
        List<Border> longestCluster = clusters.OrderByDescending(x => x.Sum(y => y.Length)).First();

        // Find center of longest cluster
        Tuple<Vector2, float> center = PolygonMapFunctions.FindBorderCenter(longestCluster);

        // Swap angle if the border was reversed
        Vector2 testPoint = center.Item1 + new Vector2(Mathf.Sin(Mathf.Deg2Rad * center.Item2) * 0.01f, Mathf.Cos(Mathf.Deg2Rad * center.Item2) * 0.01f);
        if (!GeometryFunctions.IsPointInPolygon4(Polygon.Nodes.Select(x => x.Vertex).ToList(), testPoint)) center = new Tuple<Vector2, float>(center.Item1, center.Item2 + 180);

        return center;
    }

    public WaterConnection GetWaterConnectionTo(Region otherRegion)
    {
        return WaterConnections.Where(x => x.FromRegion == otherRegion || x.ToRegion == otherRegion).First();
    }

    public float MinWorldX { get { return BorderPoints.Min(x => x.Position.x); } }
    public float MinWorldY { get { return BorderPoints.Min(x => x.Position.y); } }
    public float MaxWorldX { get { return BorderPoints.Max(x => x.Position.x); } }
    public float MaxWorldY { get { return BorderPoints.Max(x => x.Position.y); } }

    #endregion
}
