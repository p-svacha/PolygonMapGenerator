using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BiomeCreator
{
    public static void CreateBiomes(PolygonMapGenerator PMG)
    {
        GenerateTemperature(PMG);
        GeneratePrecipitation(PMG);
        foreach (GraphPolygon polygon in PMG.Polygons) polygon.Biome = GetBiome(polygon);
    }

    private static void GenerateTemperature(PolygonMapGenerator PMG)
    {
        float poleTemperature = -12f;
        float equatorTemperature = 35f;
        float temperatureModifyRange = 15f;
        TemperatureNoise noise = new TemperatureNoise(poleTemperature, equatorTemperature, temperatureModifyRange);

        foreach (GraphPolygon polygon in PMG.Polygons)
        {
            float noiseValue = noise.GetValue(polygon.CenterPoi.x, polygon.CenterPoi.y, PMG);
            polygon.Temperature = (int)noiseValue;
        }

        //if (Visualize) NoiseTester.DisplayNoise(noise, NoiseTester.TemperaturePlane, MapData, poleTemperature - temperatureModifyRange, equatorTemperature + temperatureModifyRange);
    }

    private static void GeneratePrecipitation(PolygonMapGenerator PMG)
    {
        int polePrecipitation = 0;
        int equatorPrecipitation = 5000;
        PrecipitationNoise noise = new PrecipitationNoise(polePrecipitation, equatorPrecipitation);

        foreach (GraphPolygon polygon in PMG.Polygons)
        {
            float noiseValue = noise.GetValue(polygon.CenterPoi.x, polygon.CenterPoi.y, PMG);
            polygon.Precipitation = (int)noiseValue;
        }

        //if (Visualize) NoiseTester.DisplayNoise(noise, NoiseTester.PrecipitationPlane, MapData, polePrecipitation, equatorPrecipitation);
    }

    public static Biome GetBiome(GraphPolygon polygon)
    {
        int Temperature = polygon.Temperature;
        int Precipitation = polygon.Precipitation;

        if (Temperature < -8) return Biome.Ice;
        else if (Temperature < -3) return Biome.Tundra;
        else if (Temperature < 1)
        {
            if (Precipitation < 200) return Biome.Grassland;
            else if (Precipitation < 300) return Biome.Shrubland;
            else return Biome.Tundra;
        }
        else if (Temperature < 8)
        {
            if (Precipitation < 300) return Biome.Grassland;
            else if (Precipitation < 400) return Biome.Shrubland;
            else return Biome.Taiga;
        }
        else if (Temperature < 14)
        {
            if (Precipitation < 500) return Biome.Grassland;
            else if (Precipitation < 800) return Biome.Shrubland;
            else if (Precipitation < 1700) return Biome.Temperate;
            else return Biome.TemperateRainForest;
        }
        else if (Temperature < 19)
        {
            if (Precipitation < 600) return Biome.Grassland;
            else if (Precipitation < 1100) return Biome.Shrubland;
            else if (Precipitation < 2000) return Biome.Temperate;
            else return Biome.TemperateRainForest;
        }
        else if (Temperature < 23)
        {
            if (Precipitation < 700) return Biome.Desert;
            else if (Precipitation < 1300) return Biome.Shrubland;
            else if (Precipitation < 2200) return Biome.Temperate;
            else return Biome.TemperateRainForest;
        }
        else
        {
            if (Precipitation < 800) return Biome.Desert;
            else if (Precipitation < 1400) return Biome.Savanna;
            else if (Precipitation < 2600) return Biome.Tropical;
            else return Biome.TropicalRainForest;
        }
    }
}
