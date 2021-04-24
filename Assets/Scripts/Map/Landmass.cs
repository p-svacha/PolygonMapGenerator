using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Landmass : MonoBehaviour
{
    public string Name;
    public List<Region> Regions;
    public int Size;
    public float Area;

    public List<GameObject> Borders;

    public void Init(List<Region> regions)
    {
        Name = "Landmass XYZ";
        Regions = regions;
        Size = regions.Count;
        Area = regions.Sum(x => x.Area);

        Borders = MeshGenerator.CreatePolygonGroupBorder(regions.Select(x => x.Polygon).ToList(), PolygonMapGenerator.DefaultShorelineBorderWidth, Color.black, onOutside: true, height: 0.0001f);
        foreach (GameObject border in Borders) border.transform.SetParent(transform);
    }

    public void ShowBorders(bool show)
    {
        foreach (GameObject border in Borders) border.SetActive(show);
    }
}
