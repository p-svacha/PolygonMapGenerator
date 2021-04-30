using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Continent : MonoBehaviour
{
    public string Name;
    public List<Region> Regions;
    public int Size;
    public float Area;

    public List<GameObject> Borders = new List<GameObject>();

    public void Init(List<Region> regions)
    {
        Name = "Continent XYZ";
        Regions = regions;
        Size = regions.Count;
        Area = regions.Sum(x => x.Area);

        List<List<Region>> clusters = PolygonMapFunctions.FindClusters(regions);
        foreach (List<Region> cluster in clusters)
        {
            List<GameObject> clusterBorders = MeshGenerator.CreatePolygonGroupBorder(cluster.Select(x => x.Polygon).ToList(), PolygonMapGenerator.DefaulContinentBorderWidth, Color.black, onOutside: false, yPos: PolygonMapGenerator.LAYER_CONTINENT);
            foreach (GameObject clusterBorder in clusterBorders) Borders.Add(clusterBorder);
        }
        foreach (GameObject border in Borders) border.transform.SetParent(transform);
    }

    public void ShowBorders(bool show)
    {
        foreach (GameObject border in Borders) border.SetActive(show);
    }
}
