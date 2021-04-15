using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nation
{
    public string Name;
    public Sprite Flag;
    public Color PrimaryColor;
    public Color SecondaryColor;
    public Region Capital;

    public List<Region> Regions = new List<Region>();
    public List<List<Region>> Clusters = new List<List<Region>>();
    public float Area;

    public List<GameObject> Borders = new List<GameObject>();
    public List<GameObject> RegionLabels = new List<GameObject>();

    public void AddRegion(Region region, bool doUpdate = true)
    {
        if (region.Nation != null) region.Nation.RemoveRegion(region);
        Regions.Add(region);
        region.SetNation(this);
        region.SetColor(PrimaryColor);
        if (doUpdate) UpdateProperties();
    }

    public void RemoveRegion(Region region, bool doUpdate = true)
    {
        Regions.Remove(region);
        region.SetNation(null);
        if(doUpdate) UpdateProperties();
    }

    public void UpdateProperties()
    {
        DestroyAllObjects();
        if (Regions.Count == 0) return;

        // Border
        Area = Regions.Sum(x => x.Area);
        Clusters = PolygonMapFunctions.FindClusters(Regions);
        foreach (List<Region> cluster in Clusters)
        {
            List<GameObject> clusterBorders = MeshGenerator.CreatePolygonGroupBorder(cluster.Select(x => x.Polygon).ToList(), PolygonMapGenerator.DefaultCoastBorderWidth, SecondaryColor, onOutside: false, height: 0.0002f);
            foreach (GameObject clusterBorder in clusterBorders) Borders.Add(clusterBorder);
        }

        // Label
        Label();
    }

    public void DestroyAllObjects()
    {
        foreach (GameObject border in Borders) GameObject.Destroy(border);
        foreach (GameObject regionLabel in RegionLabels) GameObject.Destroy(regionLabel);
    }

    public void Label()
    {
        foreach(List<Region> cluster in Clusters)
        {
            List<List<GraphNode>> clusterBorders = PolygonMapFunctions.FindOutsideNodes(cluster.Select(x => x.Polygon).ToList());
            List<GraphNode> outsideBorder = clusterBorders.OrderByDescending(x => x.Count).First();
            RegionLabels.Add(CenterlineLabler.LabelPolygon(outsideBorder, Name));
        }
    }
}
