using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinentNoise : Noise
{
    private RidgedMultifractalNoise RmfNoise;
    private const float BASE_SCALE = 0.3f;
    private float Scale;

    public ContinentNoise(float scaleFactor)
    {
        Scale = BASE_SCALE * scaleFactor;
        RmfNoise = new RidgedMultifractalNoise(1, 2, 6, Random.Range(int.MinValue, int.MaxValue));
    }

    /// <summary>
    /// Returns the land type for the given position:
    /// 0 = Water
    /// 1 = Land
    /// </summary>
    public override float GetValue(float x, float y, MapGenerationSettings settings)
    {
        float val = (float)(RmfNoise.GetValue(x * Scale, y * Scale, 1));
        return val;
    }
}
