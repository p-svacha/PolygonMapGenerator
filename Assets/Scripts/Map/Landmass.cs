using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Landmass : MonoBehaviour
{
    public List<Region> Regions;
    public string Name;

    public List<GameObject> Borders;

    public void Init(List<Region> regions)
    {
        Name = "Landmass XYZ";
        Regions = regions;
    }

    public void ShowBorders(bool show)
    {
        foreach (GameObject border in Borders) border.SetActive(show);
    }

    public int Size
    {
        get
        {
            return Regions.Count;
        }
    }

    public float Area
    {
        get
        {
            return Regions.Sum(x => x.Area);
        }
    }
}
