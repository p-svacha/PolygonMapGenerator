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

    // Display mode
    private MapDisplayMode DisplayMode;
    private MaterialHandler MaterialHandler;

    public void Init(string name, List<BorderPoint> bps, List<Border> borders, List<Region> regions)
    {
        MaterialHandler = GameObject.Find("MaterialHandler").GetComponent<MaterialHandler>();
        Name = name;

        BorderPoints = bps;
        Borders = borders;
        Regions = regions;

        foreach (BorderPoint bp in bps) bp.River = this;
        foreach (Border b in borders) b.River = this;
        foreach (Region r in Regions) r.Rivers.Add(this);

        Length = Borders.Sum(x => x.Length);
    }

    public void SetDisplayMode(MapDisplayMode mode)
    {
        DisplayMode = mode;
        UpdateDisplayMode();
    }

    private void UpdateDisplayMode()
    {
        switch (DisplayMode)
        {
            case MapDisplayMode.Topographic:
                GetComponent<Renderer>().material = MaterialHandler.TopographicWaterMaterial;
                break;

            case MapDisplayMode.Political:
                GetComponent<Renderer>().material = MaterialHandler.PoliticalWaterMaterial;
                break;

            case MapDisplayMode.Satellite:
                GetComponent<Renderer>().material = MaterialHandler.SatelliteWaterMaterial;
                break;
        }
    }
}
