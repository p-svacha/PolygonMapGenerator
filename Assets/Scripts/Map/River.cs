﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class River
{
    public string Name;

    public List<BorderPoint> BorderPoints = new List<BorderPoint>();
    public List<Border> Borders = new List<Border>();
    public List<Region> Regions = new List<Region>();

    public float Length;

    public River(string name, List<BorderPoint> bps, List<Border> borders, List<Region> regions)
    {
        Name = name;

        BorderPoints = bps;
        Borders = borders;
        Regions = regions;

        foreach (BorderPoint bp in bps) bp.River = this;
        foreach (Border b in borders) b.River = this;
        foreach (Region r in Regions) r.Rivers.Add(this);

        Length = Borders.Sum(x => x.Length);
    }
}