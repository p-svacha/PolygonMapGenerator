using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryFunctions
{
    #region Public Functions


    /// <summary>
    /// Modulo that can handle negative ints
    /// </summary>
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Modulo that can handle negative floats
    /// </summary>
    public static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Returns true if line segment 'p1q1' and 'p2q2' intersect.
    /// </summary>
    public static Boolean DoLineSegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        // Find the four orientations needed for general and 
        // special cases 
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case 
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases 
        // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;

        // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;

        // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;

        return false; // Doesn't fall in any of the above cases 
    }

    /// <summary>
    /// Returns the area of a polygon with the given points
    /// </summary>
    public static float GetPolygonArea(List<Vector2> points)
    {
        // Add the first point to the end.
        int num_points = points.Count;
        Vector2[] pts = new Vector2[num_points + 1];
        for (int i = 0; i < points.Count; i++) pts[i] = points[i];
        pts[num_points] = points[0];

        // Get the areas.
        float area = 0;
        for (int i = 0; i < num_points; i++)
        {
            area +=
                (pts[i + 1].x - pts[i].x) *
                (pts[i + 1].y + pts[i].y) / 2;
        }

        // Return the result.
        return Math.Abs(area);
    }

    /// <summary>
    /// Returns how much apart two degree values are. For example the degree distance between 350° and 10° would return 20°.
    /// </summary>
    public static int DegreeDistance(int deg1, int deg2)
    {
        int absDistance = deg1 > deg2 ? deg1 - deg2 : deg2 - deg1;
        return absDistance <= 180 ? absDistance : 360 - absDistance;
    }

    /// <summary>
    /// Returns a the rotated vector of a given vector by x degrees.
    /// </summary>
    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        if (degrees == 90) return new Vector2(-v.y, v.x);
        if (degrees == -90) return new Vector2(v.y, -v.x);

        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;

        float vx = (cos * tx) - (sin * ty);
        float vy = (sin * tx) + (cos * ty);
        return new Vector2(vx, vy);
    }

    /// <summary>
    /// Returns if the polygon with the given points is in clockwise or anticlockwise rotation
    /// </summary>
    public static bool IsClockwise(List<Vector2> points)
    {
        int num_points = points.Count;
        Vector2[] pts = new Vector2[num_points + 1];
        for (int i = 0; i < points.Count; i++) pts[i] = points[i];
        pts[num_points] = points[0];

        // Get the areas.
        float area = 0;
        for (int i = 0; i < num_points; i++)
        {
            area +=
                (pts[i + 1].x - pts[i].x) *
                (pts[i + 1].y + pts[i].y) / 2;
        }

        return area < 0;
    }

    /// <summary>
    /// Determines if the given point is inside the polygon
    /// </summary>
    public static bool IsPointInPolygon4(List<Vector2> polygon, Vector2 testPoint)
    {
        bool result = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
            {
                if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    /// <summary>
    /// This function returns the intersection of two lines (prevPoint - thisPoint, thisPoint - nextPoint) with a certain offset of two of those lines. Useful for adding a certain width to a path (i.e. borders or rivers).
    /// It does it by making a parallel line to the two lines with a given offset and then taking the intersection point of the offsetted parallel lines.
    /// The function will return the offseted point on the INSIDE of the three points, given the direction (clockwise or not) of the three points. If you want it on the outside, simply switch the direction.
    /// </summary>
    public static Vector2 GetOffsetIntersection(Vector2 prevPoint, Vector2 thisPoint, Vector2 nextPoint, float prevOffset, float nextOffset, bool clockwise)
    {
        Vector2 toBefore = (prevPoint - thisPoint).normalized;
        Vector2 toAfter = (nextPoint - thisPoint).normalized;

        Vector2 toBefore90 = RotateVector(toBefore, clockwise ? -90 : 90);
        Vector2 toAfter90 = RotateVector(toAfter, clockwise ? 90 : -90);

        if (IsParallel(thisPoint - prevPoint, nextPoint - thisPoint)) return thisPoint + toBefore90 * ((prevOffset + nextOffset) / 2);

        Vector2 beforeParallelStart = thisPoint + toBefore90 * prevOffset;
        Vector2 beforeParallelEnd = prevPoint + toBefore90 * prevOffset;

        Vector2 afterParallelStart = thisPoint + toAfter90 * nextOffset;
        Vector2 afterParallelEnd = nextPoint + toAfter90 * nextOffset;

        Vector2 targetPoint = FindIntersection(beforeParallelStart, beforeParallelEnd, afterParallelStart, afterParallelEnd);

        return targetPoint;
    }

    /// <summary>
    /// Returns if the two vectors p and q are close to parallel
    /// </summary>
    public static bool IsParallel(Vector2 p, Vector2 q)
    {
        Vector2 pn = p.normalized;
        Vector2 qn = q.normalized;
        float xDiff = Math.Abs(pn.x - qn.x);
        float yDiff = Math.Abs(pn.y - qn.y);
        return ((xDiff + yDiff) < 0.002f); 
    }

    public static bool IsPointOnLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        float AB = Mathf.Sqrt((lineEnd.x - lineStart.x) * (lineEnd.x - lineStart.x) + (lineEnd.y - lineStart.y) * (lineEnd.y - lineStart.y));
        float AP = Mathf.Sqrt((point.x - lineStart.x) * (point.x - lineStart.x) + (point.y - lineStart.y) * (point.y - lineStart.y));
        float PB = Mathf.Sqrt((lineEnd.x - point.x) * (lineEnd.x - point.x) + (lineEnd.y - point.y) * (lineEnd.y - point.y));
        return Mathf.Abs(AB - (AP + PB)) < 0.001f;
    }

    #endregion

    #region Private functions

    // Given three colinear Vector2s p, q, r, the function checks if 
    // Vector2 q lies on line segment 'pr' 
    private static Boolean onSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
            q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
            return true;

        return false;
    }

    // To find orientation of ordered triplet (p, q, r). 
    // The function returns following values 
    // 0 --> p, q and r are colinear 
    // 1 --> Clockwise 
    // 2 --> Counterclockwise 
    private static int orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-Vector2s/ 
        // for details of below formula. 
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // colinear 

        return (val > 0) ? 1 : 2; // clock or counterclock wise 
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private static Vector2 FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        bool segments_intersect = false;
        Vector2 intersection = new Vector2(0, 0);

        // Check parallel
        if(Math.Abs(p1.x - p2.x) < 0.001f && Math.Abs(p3.x - p4.x) < 0.001f)
        {
            Debug.LogWarning("WARNING: A PARALLEL LINE HAS BEEN FOUND IN THE SECOND CHECK THAT HAS BEEN SKIPPED IN FIRST CHECK. RETURNING 0/0 VECTOR");
            intersection = new Vector2(0, 0);
            return intersection;
        }
        float a1 = (p1.y - p2.y) / (p1.x - p2.x);
        float a2 = (p3.y - p4.y) / (p3.x - p4.x);
        if( Math.Abs(a1 - a2) < 0.00001f)
        {
            intersection = new Vector2(0, 0);
            return intersection;
        }

        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.y - p1.y;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.y - p3.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
        if (float.IsInfinity(t1))
        {
            // The lines are parallel (or close enough to it).
            segments_intersect = false;
            intersection = new Vector2(0,0);
            return intersection;
        }

        float t2 =
            ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

        return intersection;
    }

    #endregion


}
