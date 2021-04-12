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
    public float Area;

    public List<GameObject> NationPolygons = new List<GameObject>();

    public void AddRegion(Region region)
    {
        if (region.Nation != null) region.Nation.RemoveRegion(region);
        Regions.Add(region);
        region.SetNation(this);
        region.SetColor(PrimaryColor);
        CalculateArea();
    }

    public void RemoveRegion(Region region)
    {
        Regions.Remove(region);
        region.SetNation(null);
        CalculateArea();
    }

    private void CalculateArea()
    {
        Area = Regions.Sum(x => x.Area);
    }
}
