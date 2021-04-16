using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureNoise : Noise
{
    private float PoleTemperature;
    private float EquatorTemperature;
    private float TempModifyRange; // The amount the temperature can deviate from the base temperature

    private float BaseScale = 0.15f;
    private int OffsetX;
    private int OffsetY;

    public TemperatureNoise(float poleTemp, float equatorTemp, float tempModifyRange)
    {
        OffsetX = Random.Range(-10000, 10000);
        OffsetY = Random.Range(-10000, 10000);

        PoleTemperature = poleTemp;
        EquatorTemperature = equatorTemp;
        TempModifyRange = tempModifyRange;
    }

    /// <summary>
    /// Returns average temperature for the given position in °C
    /// </summary>
    public override float GetValue(float x, float y, PolygonMapGenerator PMG)
    {
        float yEquator = PMG.Height / 2;
        float baseTemp = PoleTemperature + ((1 - (Mathf.Abs(y - yEquator) / yEquator)) * (EquatorTemperature - PoleTemperature));
        float tempModifier = Mathf.PerlinNoise(OffsetX + x * BaseScale, OffsetY + y * BaseScale) * TempModifyRange - TempModifyRange / 2;
        return baseTemp + tempModifier;
    }
}
