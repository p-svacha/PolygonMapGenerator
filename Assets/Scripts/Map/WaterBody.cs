using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterBody
{
    public string Name;
    public List<Region> Regions = new List<Region>();
    public List<Region> BorderingLandRegions = new List<Region>();

    public float Area;
    public bool SaltWater;

    public WaterBody(string name, List<Region> regions, bool saltWater)
    {
        Name = name;
        Regions = regions;
        foreach(Region r in regions)
        {
            foreach (Region n in r.AdjacentRegions.Where(x => !x.IsWater))
                if (!BorderingLandRegions.Contains(n)) BorderingLandRegions.Add(n);
        }

        Area = regions.Sum(x => x.Area);
    }
    
}
