using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class River : MonoBehaviour
{
    public string Name;

    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> Regions = new List<Region>();

    public float Length;

    public void Init(List<BorderPoint> bps, List<Border> borders, List<Region> regions)
    {
        Name = "River XYZ";

        BorderPoints = bps;
        Borders = borders;
        Regions = regions;

        foreach (BorderPoint bp in bps) bp.River = this;
        foreach (Border b in borders) b.River = this;
        foreach (Region r in Regions) r.Rivers.Add(this);

        Length = Borders.Sum(x => x.Length);

        GetComponent<Renderer>().material = MapDisplayResources.Singleton.DefaultMaterial;
    }

    public void SetColor(Color c)
    {
        GetComponent<MeshRenderer>().material.color = c;
    }

    public void SetTexture(Texture2D texture)
    {
        if (texture != null) GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
        else GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture2D.whiteTexture);
    }
}
