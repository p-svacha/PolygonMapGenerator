using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nation
{
    public string Name;
    public Sprite Flag;
    public Color PrimaryColor;
    public Region Capital;

    public List<Region> Regions = new List<Region>();
    public float Area;

    public void AddRegion(Region region)
    {
        if (Regions.Contains(region)) return;

        Regions.Add(region);
        region.SetNation(this);
        CalculateArea();
    }

    private void CalculateArea()
    {
        Area = Regions.Sum(x => x.Area);
    }
}
