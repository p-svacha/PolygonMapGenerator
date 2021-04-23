using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Continent : MonoBehaviour
{
    public string Name;
    public List<Region> Regions;
    public int Size;

    public void Init(List<Region> regions)
    {
        Name = "Continent XYZ";
        Regions = regions;
        Size = regions.Count;
    }
}
