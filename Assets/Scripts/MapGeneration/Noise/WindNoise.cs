using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindNoise : Noise
{
    private float BaseScale = 0.1f;
    private int OffsetX;
    private int OffsetY;

    public WindNoise()
    {
        OffsetX = Random.Range(-10000, 10000);
        OffsetY = Random.Range(-10000, 10000);
    }

    /// <summary>
    /// Returns the wind direction for the given position in ° (0-359)
    /// </summary>
    public override float GetValue(float x, float y, MapGenerationSettings settings)
    {
        float noiseValue = Mathf.PerlinNoise(OffsetX + x * BaseScale, OffsetY + y * BaseScale);
        float value = mod((int)(noiseValue * 720f), 360);
        return value;
    }

    private int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
