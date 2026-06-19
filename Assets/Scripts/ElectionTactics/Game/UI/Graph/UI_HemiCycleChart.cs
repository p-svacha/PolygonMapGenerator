using ElectionTactics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Draws a static parliament hemicycle. Attach to a UI element; it fills the element.
/// Rows scale with sqrt of seat count; outer rows hold proportionally more seats.
/// </summary>
public class UI_HemicycleChart : GraphBase
{
    // Higher = more rows for a given seat count.
    private float RowDensityFactor = 0.5f;

    // Fraction of a circle's diameter used as the gap between circles.
    private float CircleSpacing = 0.25f;

    // Higher = more even seat distribution between inner and outer rows.
    private float RowStartOffset = 2f;

    // Smallest allowed inner hole as a fraction of outer radius. Prevents center overlap.
    private float MinInnerRadiusRatio = 0.35f;

    // Maximum radius of seat circles
    private float MaxCircleRadius = 25f;

    /// <summary>
    /// Draws the parliament for the given seat distribution.
    /// </summary>
    public void ShowParliament(Dictionary<Party, int> seatsWon)
    {
        MeasureContainer();
        ClearContainer();

        int totalSeats = seatsWon.Values.Sum();
        if (totalSeats <= 0) return;

        List<Color> seatColors = new List<Color>();
        foreach (var kvp in seatsWon.Where(x => x.Value > 0).OrderByDescending(x => x.Value))
            for (int i = 0; i < kvp.Value; i++) seatColors.Add(kvp.Key.Color);

        float R = Mathf.Min(GraphWidth / 2f, GraphHeight);
        float spacingFactor = 1f + CircleSpacing;

        // Pick the row count that yields the largest uniform circle size.
        // For a candidate rowCount, circles are spaced one radialStep apart from an inner
        // radius outward. The binding constraint is whichever is smaller: the radial fit
        // (rows must span R) or the arc fit (outer row must hold its share of seats).
        // We search rowCounts and keep the one maximizing the resulting diameter.
        int bestRowCount = 1;
        float bestDiameter = 0f;
        int[] bestSeatsPerRow = null;

        int maxRows = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalSeats))); // generous upper bound
        if (totalSeats <= 12) maxRows = 1; // Looks better

        for (int rows = 1; rows <= maxRows; rows++)
        {
            // Distribute seats across rows weighted by (r + RowStartOffset).
            float[] w = new float[rows];
            float wSum = 0f;
            for (int r = 0; r < rows; r++) { w[r] = r + RowStartOffset; wSum += w[r]; }

            int[] spr = new int[rows];
            int assigned = 0;
            for (int r = 0; r < rows; r++) { spr[r] = Mathf.FloorToInt(totalSeats * (w[r] / wSum)); assigned += spr[r]; }
            int rem = totalSeats - assigned;
            for (int r = rows - 1; r >= 0 && rem > 0; r--, rem--) spr[r]++;

            int nOuter = spr[rows - 1];

            // Diameter the outer arc allows:
            float dArc = (Mathf.PI * R) / (nOuter * spacingFactor + Mathf.PI * 0.5f);
            // Diameter the radial span allows (rows one step apart spanning R):
            float dRadial = R / ((rows - 1) * spacingFactor + 1f);

            float d = Mathf.Min(dArc, dRadial);

            // Inner ring radius this candidate would produce.
            float radialStepCandidate = d * spacingFactor;
            float innerRingRadius = (R - d / 2f) - (rows - 1) * radialStepCandidate;

            // Reject candidates that pack circles past the minimum inner hole (causes center overlap).
            if (innerRingRadius < MinInnerRadiusRatio * R) continue;

            if (d > bestDiameter)
            {
                bestDiameter = d;
                bestRowCount = rows;
                bestSeatsPerRow = spr;
            }
        }

        int rowCount = bestRowCount;
        int[] seatsPerRow = bestSeatsPerRow;
        float diameter = bestDiameter;
        float circleRadius = diameter / 2f;
        
        if (circleRadius > MaxCircleRadius)
        {
            circleRadius = MaxCircleRadius;
            diameter = 2 * circleRadius;
        }

        float radialStep = diameter * spacingFactor;

        Vector2 origin = new Vector2(GraphWidth / 2f, circleRadius);

        var positions = new List<(float angleFraction, int row, Vector2 pos)>();
        for (int r = 0; r < rowCount; r++)
        {
            int n = seatsPerRow[r];
            if (n == 0) continue;

            float ringRadius = (R - circleRadius) - (rowCount - 1 - r) * radialStep;

            for (int i = 0; i < n; i++)
            {
                float angleT = n == 1 ? 0.5f : (float)i / (n - 1);
                float angleDeg = Mathf.Lerp(180f, 0f, angleT);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector2 pos = origin + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * ringRadius;
                positions.Add((angleT, r, pos));
            }
        }

        var ordered = positions.OrderBy(p => p.angleFraction).ThenBy(p => p.row).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            Color color = i < seatColors.Count ? seatColors[i] : Color.gray;
            CreateCircle(ordered[i].pos, diameter, color);
        }
    }

    /// <summary>
    /// Convenience overload taking a full election result.
    /// </summary>
    public void ShowParliament(GeneralElectionResult result) => ShowParliament(result.SeatsWon);
}
