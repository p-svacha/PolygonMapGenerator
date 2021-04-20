using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTopologyNoise : Noise
{
    private float BaseScale = 0.2f;
    private int DeepOceanOffsetX;
    private int DeepOceanOffsetY;

    public WaterTopologyNoise()
    {
        DeepOceanOffsetX = Random.Range(-10000, 10000);
        DeepOceanOffsetY = Random.Range(-10000, 10000);
    }

    /// <summary>
    /// Returns the water topology type for the given position:
    /// 0 = Ocean
    /// 1 = Deep Ocean
    /// </summary>
    public override float GetValue(float x, float y, MapGenerationSettings settings)
    {
        float deepOceanValue = Mathf.PerlinNoise(DeepOceanOffsetX + x * BaseScale, DeepOceanOffsetY + y * BaseScale);
        if (deepOceanValue < 0.5f) return 1;
        else return 0;
    }
}
