using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplayResources : MonoBehaviour
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

    [Header("Biome Textures")]
    public Texture2D WaterTexture;
    public Texture2D IceTexture;
    public Texture2D TundraTexture;
    public Texture2D TaigaTexture;
    public Texture2D ShrublandTexture;
    public Texture2D GrasslandTexture;
    public Texture2D TemperateRainforestTexture;
    public Texture2D TemperateTexture;
    public Texture2D TropicalRainforestTexture;
    public Texture2D TropicalTexture;
    public Texture2D SavannaTexture;
    public Texture2D DesertTexture;

    [Header("Special Colors")]
    public Color HighlightColor;
    public Color GreyedOutColor;

    [Header("Textures")]
    public Texture2D WaterBackgroundTexture;
    public Texture2D MinorNoiseTexture;

    public static MapDisplayResources Singleton
    {
        get
        {
            return GameObject.Find("MapDisplayResources").GetComponent<MapDisplayResources>();
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

    public Texture2D GetBiomeTexture(Biome b)
    {
        if (b == Biome.Ice) return IceTexture;
        if (b == Biome.Tundra) return TundraTexture;
        if (b == Biome.TropicalRainForest) return TropicalRainforestTexture;
        if (b == Biome.Tropical) return TropicalTexture;
        if (b == Biome.TemperateRainForest) return TemperateRainforestTexture;
        if (b == Biome.Temperate) return TemperateTexture;
        if (b == Biome.BorealForest) return TaigaTexture;
        if (b == Biome.Shrubland) return ShrublandTexture;
        if (b == Biome.Savanna) return SavannaTexture;
        if (b == Biome.Grassland) return GrasslandTexture;
        if (b == Biome.Desert) return DesertTexture;

        throw new System.Exception("Texture not found for biome " + b.ToString());
    }
}