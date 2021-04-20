using UnityEngine;

public class PrecipitationNoise : Noise
{
    private int PolePrecipitation;
    private int EquatorPrecipitation;

    private float BaseScale = 0.2f;
    private int OffsetX;
    private int OffsetY;

    public PrecipitationNoise(int polePrec, int equatorPrec)
    {
        OffsetX = Random.Range(-10000, 10000);
        OffsetY = Random.Range(-10000, 10000);

        PolePrecipitation = polePrec;
        EquatorPrecipitation = equatorPrec;
    }

    /// <summary>
    /// Returns the annual precipitation for the given position in mm
    /// </summary>
    public override float GetValue(float x, float y, MapGenerationSettings settings)
    {
        float yEquator = settings.Height / 2;
        float basePrec = PolePrecipitation + ((1 - (Mathf.Abs(y - yEquator) / yEquator)) * (EquatorPrecipitation - PolePrecipitation));
        float precMoidifer = Mathf.PerlinNoise(OffsetX + x * BaseScale, OffsetY + y * BaseScale);
        return basePrec * precMoidifer;
    }
}
