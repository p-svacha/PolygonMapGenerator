using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerationSettings
{
    public int Width;
    public int Height;
    public int Area;

    public float MinPolygonArea;
    public float MaxPolygonArea;
    public float AvgPolygonArea;

    public int MinContinentSize;
    public int MaxContinentSize;

    public MapType MapType;
    public float ContinentSizeScaleFactor;

    public MapGenerationSettings(int width = 10, int height = 10, float minPolygonArea = 0.08f, float maxPolygonArea = 1.5f, int minContinentSize = 5, int maxContinentSize = 30, MapType mapType = MapType.Island, float continentSizeScaleFactor = 1f)
    {
        Width = width;
        Height = height;
        Area = width * height;

        MinPolygonArea = minPolygonArea;
        MaxPolygonArea = maxPolygonArea;
        AvgPolygonArea = (minPolygonArea + maxPolygonArea) / 2f;

        MinContinentSize = minContinentSize;
        MaxContinentSize = maxContinentSize;

        MapType = mapType;
        ContinentSizeScaleFactor = continentSizeScaleFactor;
    }

}
