using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmass
{
    public List<Region> Regions;
    public string Name;

    public Landmass(List<Region> regions, string name)
    {
        Regions = regions;
        Name = name;
    }

    public int Size
    {
        get
        {
            return Regions.Count;
        }
    }
}
