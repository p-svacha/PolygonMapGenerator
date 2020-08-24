using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public static class SatelliteTextureGenerator
{
    public static void GenerateTexture(PolygonMapGenerator PMG)
    {
        // Land texture
        int perlinRange = 10000;
        Vector2 offset = new Vector2(UnityEngine.Random.Range(-perlinRange, perlinRange), UnityEngine.Random.Range(-perlinRange, perlinRange));
        float perlinScale = PolygonMapGenerator.MAP_WIDTH * 0.000365f;

        int texSize = 2048;
        Texture2D splatMap = new Texture2D(texSize, texSize);
        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float perlinValue = Mathf.PerlinNoise(offset.x + x * perlinScale, offset.y + y * perlinScale);
                float r = 0, g = 0, b = 0, a = 0;
                if (perlinValue < 1f / 7f) r = 1;
                else if (perlinValue < 2f / 7f)
                {
                    g = (perlinValue - 1f / 7f) / (1f / 7f);
                    r = 1 - g;
                }
                else if (perlinValue < 3f / 7f) g = 1;
                else if (perlinValue < 4f / 7f)
                {
                    b = (perlinValue - 3f / 7f) / (1f / 7f);
                    g = 1 - b;
                }
                else if (perlinValue < 5f / 7f) b = 1;
                else if (perlinValue < 6f / 7f)
                {
                    a = (perlinValue - 5f / 7f) / (1f / 7f);
                    b = 1 - a;
                }
                else a = 1;

                Color splatCol = new Color(r, g, b, a);
                splatMap.SetPixel(x, y, splatCol);
            }
        }
        splatMap.Apply();
        PMG.SatelliteLandMaterial.SetTexture("_Control", splatMap);

        // Shore texture
        int shoreTexSize = 2048;
        Texture2D shoreTexture = new Texture2D(shoreTexSize, shoreTexSize);
        for (int y = 0; y < texSize; y++)
            for (int x = 0; x < texSize; x++)
                shoreTexture.SetPixel(x, y, Color.black);

        foreach (GraphConnection c in PMG.InGraphConnections.Where(x => x.IsShore()))
        {
            int steps = 10;
            int fadeRange = 10;
            float maxDistance = fadeRange;
            for(int i = 0; i < steps + 1; i++)
            {
                Vector2 worldPosition = Vector2.Lerp(c.StartNode.Vertex, c.EndNode.Vertex, 1f * i / steps);
                int texX = (int)((worldPosition.x / PolygonMapGenerator.MAP_WIDTH) * shoreTexSize);
                int texY = (int)((worldPosition.y / PolygonMapGenerator.MAP_HEIGHT) * shoreTexSize);
                for(int x = texX - fadeRange; x < texX + fadeRange + 1; x++)
                {
                    for(int y = texY -fadeRange; y < texY + fadeRange + 1; y++)
                    {
                        float oldFactor = shoreTexture.GetPixel(x, y).r;

                        float distance = Math.Abs(x - texX) + Math.Abs(y - texY);
                        float factor = 1f - (distance / maxDistance);
                        //Debug.Log(factor);

                        if(factor > oldFactor) shoreTexture.SetPixel(x, y, new Color(factor, factor, factor));
                    }
                }
            }
        }
        shoreTexture.Apply();
        PMG.SatelliteLandMaterial.SetTexture("_OverlayMask", shoreTexture);

    }
}
