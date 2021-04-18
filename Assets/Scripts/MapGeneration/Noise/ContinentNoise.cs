using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinentNoise : Noise
{
    private RidgedMultifractalNoise rmfn = new RidgedMultifractalNoise(1, 2, 6, Random.Range(int.MinValue, int.MaxValue));
    private float baseScale = 0.3f;

    /// <summary>
    /// Returns the land type for the given position:
    /// 0 = Water
    /// 1 = Land
    /// </summary>
    public override float GetValue(float x, float y, PolygonMapGenerator PMG)
    {
        float val = (float)(rmfn.GetValue(x * baseScale, y * baseScale, 1));
        return val;
    }
}
