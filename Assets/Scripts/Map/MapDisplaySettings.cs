using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplaySettings : MonoBehaviour
{
    public Material DefaultMaterial;

    [Header("Basic Colors")]
    public Color LandColor;
    public Color WaterColor;

    [Header("Biome Colors")]
    public Color IceColor;
    public Color TundraColor;
    public Color TaigaColor;
    public Color ShrublandColor;
    public Color GrasslandColor;
    public Color TemperateRainforestColor;
    public Color TemperateColor;
    public Color TropicalRainforestColor;
    public Color TropicalColor;
    public Color SavannaColor;
    public Color DesertColor;

    [Header("Special Colors")]
    public Color HighlightColor;
    public Color GreyedOutColor;

    [Header("Textures")]
    public Texture2D WaterBackgroundTexture;

    public static MapDisplaySettings Settings
    {
        get
        {
            return GameObject.Find("DisplaySettings").GetComponent<MapDisplaySettings>();
        }
    }

    public Color GetBiomeColor(Biome b)
    {
        if (b == Biome.Ice) return IceColor;
        if (b == Biome.Tundra) return TundraColor;
        if (b == Biome.TropicalRainForest) return TropicalRainforestColor;
        if (b == Biome.Tropical) return TropicalColor;
        if (b == Biome.TemperateRainForest) return TemperateRainforestColor;
        if (b == Biome.Temperate) return TemperateColor;
        if (b == Biome.BorealForest) return TaigaColor;
        if (b == Biome.Shrubland) return ShrublandColor;
        if (b == Biome.Savanna) return SavannaColor;
        if (b == Biome.Grassland) return GrasslandColor;
        if (b == Biome.Desert) return DesertColor;

        throw new System.Exception("Color not found for biome " + b.ToString());
    }
}