using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryFunctions
{
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

    // The main function that returns true if line segment 'p1q1' 
    // and 'p2q2' intersect. 
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

    public static float ToRad(float angle)
    {
        return (float)(Math.PI / 180 * angle);
    }

    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    public static float mod(float x, float m)
    {
        return (x % m + m) % m;
    }

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

    public static int DegreeDistance(int deg1, int deg2)
    {
        int absDistance = deg1 > deg2 ? deg1 - deg2 : deg2 - deg1;
        return absDistance <= 180 ? absDistance : 360-absDistance;
    }

    public static bool IsInRange(Vector2 v, float minX, float maxX, float minY, float maxY)
    {
        return (v.x >= minX && v.x <= maxX && v.y >= minY && v.y <= maxY);
    }

    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;

        float vx = (cos * tx) - (sin * ty);
        float vy = (sin * tx) + (cos * ty);
        return new Vector2(vx, vy);
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    public static Vector2 FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        bool lines_intersect = false;
        bool segments_intersect = false;
        Vector2 intersection = new Vector2(0, 0);

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
            lines_intersect = false;
            segments_intersect = false;
            intersection = new Vector2(0,0);
            return intersection;
        }
        lines_intersect = true;

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

}
