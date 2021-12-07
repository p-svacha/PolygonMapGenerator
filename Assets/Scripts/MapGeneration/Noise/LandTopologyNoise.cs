using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandTopologyNoise : Noise
{
    private float BaseScale = 0.2f;
    private int HillOffsetX;
    private int HillOffsetY;
    private int MountainOffsetX;
    private int MountainOffsetY;
    private int LakeOffsetX;
    private int LakeOffsetY;

    public LandTopologyNoise()
    {
        HillOffsetX = Random.Range(-10000, 10000);
        HillOffsetY = Random.Range(-10000, 10000);
        MountainOffsetX = Random.Range(-10000, 10000);
        MountainOffsetY = Random.Range(-10000, 10000);
        LakeOffsetX = Random.Range(-10000, 10000);
        LakeOffsetY = Random.Range(-10000, 10000);
    }

    /// <summary>
    /// Returns the topology type for the given position:
    /// 0 = Flat
    /// 1 = Hills
    /// 2 = Mountains
    /// 3 = Lake
    /// </summary>
    public override float GetValue(float x, float y, MapGenerationSettings settings)
    {
        float hillValue = Mathf.PerlinNoise(HillOffsetX + x * BaseScale, HillOffsetY + y * BaseScale);
        float mountainValue = Mathf.PerlinNoise(MountainOffsetX + x * BaseScale, MountainOffsetY + y * BaseScale);
        float lakeValue = Mathf.PerlinNoise(LakeOffsetX + x * BaseScale, LakeOffsetY + y * BaseScale);
        if (mountainValue < 0.2f) return 2;
        else if (hillValue < 0.35f) return 1;
        else if (lakeValue < 0.2f) return 3;
        else return 0;
    }
}
