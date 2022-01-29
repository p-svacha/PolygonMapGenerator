using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterBody : MonoBehaviour
{
    public string Name;
    public List<Region> Regions = new List<Region>();
    public List<Region> BorderingLandRegions = new List<Region>();

    public float Area;
    public bool IsLake;

    public void Init(List<Region> regions)
    {
        Regions = regions;
        IsLake = regions.Count < 5;
        foreach(Region r in regions)
        {
            foreach (Region n in r.AdjacentRegions.Where(x => !x.IsWater))
                if (!BorderingLandRegions.Contains(n)) BorderingLandRegions.Add(n);
        }

        Name = IsLake ? "Lake XYZ" : "XYZ Ocean";

        Area = regions.Sum(x => x.Area);
    }
    
}
