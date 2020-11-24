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

    public List<Region> Provinces = new List<Region>();
    public float Area;

    public void AddProvince(Region region)
    {
        if (region.Nation != null) region.Nation.RemoveProvince(region);
        Provinces.Add(region);
        region.SetNation(this);
        CalculateArea();
    }

    public void RemoveProvince(Region region)
    {
        Provinces.Remove(region);
        region.SetNation(null);
        CalculateArea();
    }

    private void CalculateArea()
    {
        Area = Provinces.Sum(x => x.Area);
    }
}
