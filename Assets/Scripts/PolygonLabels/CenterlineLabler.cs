using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiLib;
using VoronoiLib.Structures;

/// <summary>
/// The centerline labler draws names on polygons. (see https://observablehq.com/@veltman/centerline-labeling)
/// Uses an open source voronoi library (see https://github.com/Zalgo2462/VoronoiLib)
/// </summary>
public static class CenterlineLabler
{

    public static GameObject LabelPolygon(List<List<GraphNode>> polygonBorders, string label, Color color)
    {
        // 1. Compute voronoi of given points with "VoronoiLib"
        List<FortuneSite> points = new List<FortuneSite>();
        List<GraphNode> nodes = new List<GraphNode>();
        foreach (List<GraphNode> polygonBorder in polygonBorders)
            foreach (GraphNode n in polygonBorder)
                if (!nodes.Contains(n)) nodes.Add(n);
        foreach (GraphNode n in nodes) points.Add(new FortuneSite(n.Vertex.x, n.Vertex.y));

        float margin = 0.2f;
        float minX = nodes.Min(x => x.Vertex.x) - margin;
        float maxX = nodes.Max(x => x.Vertex.x) + margin;
        float minY = nodes.Min(x => x.Vertex.y) - margin;
        float maxY = nodes.Max(x => x.Vertex.y) + margin; 
        LinkedList<VEdge> voronoi = FortunesAlgorithm.Run(points, minX, minY, maxX, maxY);

        // 2. Remove all edges that have either start or end outside of polygon
        List<Vector2> vertices = nodes.Select(x => x.Vertex).ToList();
        List<VEdge> edgesToRemove = new List<VEdge>();
        foreach(VEdge edge in voronoi)
        {
            if(!GeometryFunctions.IsPointInPolygon4(vertices, new Vector2((float)edge.Start.X, (float)edge.Start.Y)) || !GeometryFunctions.IsPointInPolygon4(vertices, new Vector2((float)edge.End.X, (float)edge.End.Y)))
            {
                edgesToRemove.Add(edge);
            }
        }
        foreach (VEdge edge in edgesToRemove) voronoi.Remove(edge);

        // DEBUG voronoi
        foreach (VEdge edge in voronoi) Debug.DrawLine(new Vector3((float)edge.Start.X, 0f, (float)edge.Start.Y), new Vector3((float)edge.End.X, 0f, (float)edge.End.Y), Color.red, 30);

        // 3. Turn remaining edges into a graph (create a list of for each point representing the points it is connected to)
        Dictionary<VPoint, List<VPoint>> voronoiGraph = new Dictionary<VPoint, List<VPoint>>();
        foreach(VEdge edge in voronoi)
        {
            // handle start point
            if (!voronoiGraph.ContainsKey(edge.Start)) voronoiGraph.Add(edge.Start, new List<VPoint>() { edge.End });
            else if (!voronoiGraph[edge.Start].Contains(edge.End)) voronoiGraph[edge.Start].Add(edge.End);

            // handle end point
            if (!voronoiGraph.ContainsKey(edge.End)) voronoiGraph.Add(edge.End, new List<VPoint>() { edge.Start });
            else if (!voronoiGraph[edge.End].Contains(edge.Start)) voronoiGraph[edge.End].Add(edge.Start);
        }

        // 4. Find longest path (with consideration for straightness) between two leaves in graph - this is the centerline
        Dictionary<VPoint, List<VPoint>> voronoiLeaves = voronoiGraph.Where(x => x.Value.Count == 1).ToDictionary(x => x.Key, x => x.Value);

        float curvePenalty = 0.00007f;
        float longestDistance = float.MinValue;
        List<VPoint> centerLine = new List<VPoint>();
        float longestDistanceNoPenalty = float.MinValue;
        List<VPoint> centerLineNoPenalty = new List<VPoint>();

        foreach(KeyValuePair<VPoint, List<VPoint>> startPoint in voronoiLeaves)
        {
            foreach(KeyValuePair<VPoint, List<VPoint>> endPoint in voronoiLeaves)
            {
                if (startPoint.Key == endPoint.Key) continue;

                List<VPoint> leavesPath = GetShortestPath(new List<VPoint>() { startPoint.Key }, endPoint.Key, voronoiGraph);
                if (leavesPath == null) continue;
                float distanceWithPenalty = GetPathDistance(leavesPath, curvePenalty);
                if(distanceWithPenalty > longestDistance)
                {
                    longestDistance = distanceWithPenalty;
                    centerLine = leavesPath;
                }

                float distanceNoPenalty = GetPathDistance(leavesPath, 0f);
                if (distanceNoPenalty > longestDistanceNoPenalty)
                {
                    longestDistanceNoPenalty = distanceNoPenalty;
                    centerLineNoPenalty = leavesPath;
                }
            }
        }

        // If the straight centerline is too short, take the longer curvy one instead
        if (GetPathDistance(centerLine, 0f) < 0.4f * GetPathDistance(centerLineNoPenalty, 0f)) centerLine = centerLineNoPenalty;

        // 5. Smoothen the centerline
        int smoothSteps = 5;
        List<VPoint> smoothCenterLine = new List<VPoint>();
        smoothCenterLine.AddRange(centerLine);
        for (int i = 0; i < smoothSteps; i++) smoothCenterLine = SmoothLine(smoothCenterLine);
        

        // DEBUG centerline
        //Debug.Log("Longest path without curve penalty: " + GetPathDistance(centerLineNoPenalty, 0f));
        //Debug.Log("Longest path with curve penalty: " + GetPathDistance(centerLine, curvePenalty));
        //for (int i = 1; i < centerLineNoPenalty.Count; i++) Debug.DrawLine(new Vector3((float)centerLineNoPenalty[i - 1].X, 0f, (float)centerLineNoPenalty[i - 1].Y), new Vector3((float)centerLineNoPenalty[i].X, 0f, (float)centerLineNoPenalty[i].Y), Color.blue, 30);
        //for (int i = 1; i < centerLine.Count; i++) Debug.DrawLine(new Vector3((float)centerLine[i - 1].X, 0f, (float)centerLine[i - 1].Y), new Vector3((float)centerLine[i].X, 0f, (float)centerLine[i].Y), Color.red, 30);
        for (int i = 1; i < smoothCenterLine.Count; i++) Debug.DrawLine(new Vector3((float)smoothCenterLine[i - 1].X, 0f, (float)smoothCenterLine[i - 1].Y), new Vector3((float)smoothCenterLine[i].X, 0f, (float)smoothCenterLine[i].Y), Color.blue, 30);

        // 6. Make sure the path goes from left to right 
        double xChange = 0;
        for(int i = 1; i < smoothCenterLine.Count; i++) xChange += smoothCenterLine[i].X - smoothCenterLine[i - 1].X;
        if (xChange < 0) smoothCenterLine.Reverse();


        // 7. Place text along centerline
        return DrawTextAlongPath(label, smoothCenterLine, color);
    }

    
    /// <summary>
    /// Returns the best path between the given start point (sole element of currentPath) and the endpoint within the voronoiGraph
    /// </summary>
    private static List<VPoint> GetShortestPath(List<VPoint> currentPath, VPoint endPoint, Dictionary<VPoint, List<VPoint>> voronoiGraph)
    {
        

        VPoint currentPoint = currentPath.Last();
        if (voronoiGraph[currentPoint].Contains(endPoint))
        {
            List<VPoint> nextPath = new List<VPoint>();
            nextPath.AddRange(currentPath);
            nextPath.Add(endPoint);
            return nextPath;
        }

        else if (voronoiGraph[currentPoint].All(x => currentPath.Contains(x))) return null;
        else
        {
            List<VPoint> shortestPath = null;
            float shortestDistance = float.MaxValue;

            foreach (VPoint nextPoint in voronoiGraph[currentPoint].Where(x => !currentPath.Contains(x)))
            {
                List<VPoint> nextPath = new List<VPoint>();
                nextPath.AddRange(currentPath);
                nextPath.Add(nextPoint);
                List<VPoint> nextPointPath = GetShortestPath(nextPath, endPoint, voronoiGraph);
                if (nextPointPath != null)
                {
                    float nextPointPathDistance = GetPathDistance(nextPointPath, 0f);
                    if(nextPointPathDistance < shortestDistance)
                    {
                        shortestDistance = nextPointPathDistance;
                        shortestPath = nextPointPath;
                    }
                }
            }
            return shortestPath;
        }
    }

    /// <summary>
    /// Returns the total distance of a path. A curve penalty can be added so straighter paths get favoured
    /// </summary>
    private static float GetPathDistance(List<VPoint> points, float curvePenalty)
    {
        float sum = 0f;
        for(int i = 1; i < points.Count; i++)
        {
            Vector2 lastPoint = new Vector2((float)points[i - 1].X, (float)points[i - 1].Y);
            Vector2 thisPoint = new Vector2((float)points[i].X, (float)points[i].Y);

            sum += Vector2.Distance(lastPoint, thisPoint);

            if(i < points.Count - 1 && curvePenalty != 0)
            {
                Vector2 nextPoint = new Vector2((float)points[i + 1].X, (float)points[i + 1].Y);

                Vector2 from = thisPoint - lastPoint;
                Vector2 to = nextPoint - thisPoint;

                float angle = Vector2.Angle(from, to);
                sum -= (angle * angle * curvePenalty);
            }
        }
        return sum;
    }

    private static List<VPoint> SmoothLine(List<VPoint> line)
    {
        List<VPoint> smoothLine = new List<VPoint>();
        smoothLine.Add(line[0]);
        for (int i = 1; i < line.Count - 1; i++)
        {
            List<VPoint> avgPoints = new List<VPoint>();
            if (i > 0) avgPoints.Add(line[i - 1]);
            avgPoints.Add(line[i]);
            if (i < line.Count - 1) avgPoints.Add(line[i + 1]);

            VPoint smoothenedPoint = new VPoint(avgPoints.Average(x => x.X), avgPoints.Average(x => x.Y));
            smoothLine.Add(smoothenedPoint);
        }
        smoothLine.Add(line.Last());
        return smoothLine;
    }

    /// <summary>
    /// Draws a text along a given path. 
    /// Position defines where (relatively) on the path the text is drawn (0.5 = center).
    /// PreferredSize defines the font size in world space. If the path is too short for the whole text, the size is scaled down.
    /// LetterPaddingFactor defines the space between letters relatively to the font size. (1 = space is equal to font size)
    /// </summary>
    private static GameObject DrawTextAlongPath(string text, List<VPoint> path, Color color, float position = 0.5f, float preferredSize = 0.15f, float letterPaddingFactor = 1f)
    {
        GameObject textObject = new GameObject(text);

        float letterPadding = preferredSize * letterPaddingFactor;
        float totalTextWidth = text.Length * letterPadding; // approx
        float curPointDistance = 0f;
        int curPointIndex = 0;
        float pointDistance = Vector2.Distance(VPointToVector2(path[curPointIndex]), VPointToVector2(path[curPointIndex + 1]));

        // Calculate where on the path to start with the text
        float totalPathDistance = GetPathDistance(path, 0f);
        float desiredStartDistance = (totalPathDistance * 0.5f) - (totalTextWidth * 0.5f);

        while(desiredStartDistance < 0.1f)
        {
            preferredSize -= 0.01f;
            letterPadding = preferredSize * letterPaddingFactor;
            totalTextWidth = text.Length * letterPadding;
            desiredStartDistance = (totalPathDistance * 0.5f) - (totalTextWidth * 0.5f);
        }

        float curDistance = desiredStartDistance;
        while(desiredStartDistance > pointDistance)
        {
            desiredStartDistance -= pointDistance;
            curPointDistance += pointDistance;
            curPointIndex++;
            pointDistance = Vector2.Distance(VPointToVector2(path[curPointIndex]), VPointToVector2(path[curPointIndex + 1]));
        }
        //Debug.Log("curDistance: " + curDistance + ", curIndex: " + curPointIndex + ", curPointDistance: " + curPointDistance + ", totalTextWidth: " + totalTextWidth + ", pathDistance: " + totalPathDistance);

        // Go through each char individually and place them along the path
        foreach (char c in text)
        {
            // Instantiate textmesh object with chat
            GameObject letterObject = new GameObject(c.ToString());
            letterObject.transform.SetParent(textObject.transform); 
            TextMesh textMesh = letterObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.text = c.ToString();
            textMesh.color = color;
            letterObject.transform.localScale = new Vector3(preferredSize, preferredSize, preferredSize);

            // Make it smooth and crisp
            textMesh.fontSize = 120;
            letterObject.transform.localScale /= 10;

            // Calculate and set position/rotation of textmesh
            float angle = Vector2.SignedAngle(VPointToVector2(path[curPointIndex + 1]) - VPointToVector2(path[curPointIndex]), Vector2.up);
            Vector2 position2D = Vector2.Lerp(VPointToVector2(path[curPointIndex]), VPointToVector2(path[curPointIndex + 1]), (curDistance - curPointDistance) / pointDistance);
            letterObject.transform.position = new Vector3(position2D.x, 0f, position2D.y);
            letterObject.transform.rotation = Quaternion.Euler(90f, angle - 90f, 0f);

            // Update current position
            curDistance += letterPadding;
            float tmpWidth = (curDistance - curPointDistance);
            while(tmpWidth > pointDistance)
            {
                tmpWidth -= pointDistance;
                curPointDistance += pointDistance;
                curPointIndex++;
                pointDistance = Vector2.Distance(VPointToVector2(path[curPointIndex]), VPointToVector2(path[curPointIndex + 1]));
            }
        }

        return textObject;
    }

    private static Vector2 VPointToVector2(VPoint point)
    {
        return new Vector2((float)point.X, (float)point.Y);
    }
}
