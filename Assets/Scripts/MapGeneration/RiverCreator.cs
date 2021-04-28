using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GeometryFunctions;

public static class RiverCreator
{
    private static float START_RIVER_SIZE = 0.01f;
    private static int SIZE_PER_RIVER = 23;
    private static int SIZE_PER_RIVER_RANGE = 6;
    private static float MIN_RIVER_EXPANSION_RATE = 0.0005f;
    private static float MAX_RIVER_EXPANSION_RATE = 0.002f;
    private static float MIN_RIVER_MAX_WIDTH = 0.03f;
    private static float MAX_RIVER_MAX_WIDTH = 0.06f;

    public static void CreateRivers(PolygonMapGenerator PMG)
    {
        int sizePerRiver = UnityEngine.Random.Range(SIZE_PER_RIVER - SIZE_PER_RIVER_RANGE, SIZE_PER_RIVER + SIZE_PER_RIVER_RANGE);
        int numRivers = PMG.GenerationSettings.Area / sizePerRiver;
        Debug.Log("Creating " + numRivers + " rivers.");
        for (int i = 0; i < numRivers; i++) DoCreateRiver(PMG);
    }

    public static void DoCreateRiver(PolygonMapGenerator PMG)
    {
        List<GraphConnection> candidateStartConnections = PMG.InGraphConnections.Where(x => x.River == null && x.Type == BorderType.Inland && x.StartNode.Type != BorderPointType.Shore && x.EndNode.Type != BorderPointType.Shore && x.StartNode.River == null && x.EndNode.River == null).ToList();
        if (candidateStartConnections.Count == 0) return;

        GraphPath river = new GraphPath();
        float riverWidth = START_RIVER_SIZE;

        List<GraphConnection> forbiddenConnections = new List<GraphConnection>();

        GraphConnection lastSegment = candidateStartConnections[UnityEngine.Random.Range(0, candidateStartConnections.Count)];
        GraphNode currentEndPoint;
        GraphNode lastEndPoint;
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            currentEndPoint = lastSegment.StartNode;
            lastEndPoint = lastSegment.EndNode;
        }
        else
        {
            currentEndPoint = lastSegment.EndNode;
            lastEndPoint = lastSegment.StartNode;
        }
        lastEndPoint.RiverWidth = riverWidth;
        currentEndPoint.RiverWidth = riverWidth;

        forbiddenConnections.AddRange(lastEndPoint.Connections);
        river.Nodes.Add(lastEndPoint);
        river.Nodes.Add(currentEndPoint);
        river.Connections.Add(lastSegment);

        bool endRiver = (currentEndPoint.Type == BorderPointType.Shore || currentEndPoint.River != null);
        float expansionRate = UnityEngine.Random.Range(MIN_RIVER_EXPANSION_RATE, MAX_RIVER_EXPANSION_RATE);
        float maxWidth = UnityEngine.Random.Range(MIN_RIVER_MAX_WIDTH, MAX_RIVER_MAX_WIDTH);
        TurnConnectionToRiver(lastSegment, river, riverWidth);

        while (!endRiver)
        {
            riverWidth += expansionRate;
            if (riverWidth > maxWidth) riverWidth = maxWidth;

            // Find candidates for next node of river
            List<GraphConnection> candidates = currentEndPoint.Connections.Where(x => 
            x.River == null
            && x.Type == BorderType.Inland
            && !forbiddenConnections.Contains(x)
            && (x.StartNode.River == null || x.EndNode.River == null)
            && (x.StartNode.DistanceFromNearestOcean <= currentEndPoint.DistanceFromNearestOcean)
            && (x.EndNode.DistanceFromNearestOcean <= currentEndPoint.DistanceFromNearestOcean)).ToList();

            if (candidates.Count == 0)
            {

                TurnRiverToLand(lastSegment);
                river.Connections.Remove(lastSegment);
                river.Nodes.Remove(currentEndPoint);

                currentEndPoint = lastSegment.EndNode == currentEndPoint ? lastSegment.StartNode : lastSegment.EndNode;
                foreach (GraphConnection c in currentEndPoint.Connections) if (forbiddenConnections.Contains(c)) forbiddenConnections.Remove(c);
                forbiddenConnections.Add(lastSegment);
                lastSegment = currentEndPoint.Connections.FirstOrDefault(x => x.River != null);
                if (lastSegment == null) return;
            }
            else
            {
                lastSegment = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                river.Connections.Add(lastSegment);

                lastEndPoint = currentEndPoint;
                forbiddenConnections.AddRange(lastEndPoint.Connections);

                currentEndPoint = lastSegment.StartNode == currentEndPoint ? lastSegment.EndNode : lastSegment.StartNode;
                currentEndPoint.RiverWidth = riverWidth;
                river.Nodes.Add(currentEndPoint);

                endRiver = (currentEndPoint.Type == BorderPointType.Shore || (currentEndPoint.River != null && currentEndPoint.Connections.Where(x => x.River != null).Min(x => x.RiverWidth) > riverWidth));
                TurnConnectionToRiver(lastSegment, river, riverWidth);
            }
        }

        // Add polygons to river
        foreach (GraphConnection c in river.Connections)
        {
            foreach (GraphPolygon p in c.Polygons)
            {
                if (!river.Polygons.Contains(p)) river.Polygons.Add(p);
                p.Rivers.Add(river);
            }
        }
        PMG.RiverPaths.Add(river);
    }

    private static void TurnConnectionToRiver(GraphConnection c, GraphPath river, float width)
    {
        c.River = river;
        c.StartNode.River = river;
        c.EndNode.River = river;
        c.RiverWidth = width;
        foreach (GraphPolygon p in c.Polygons) p.FindRivers();
    }
    private static void TurnRiverToLand(GraphConnection c)
    {
        c.River = null;
        c.StartNode.River = null;
        c.EndNode.River = null;
        c.RiverWidth = 0;
        foreach (GraphPolygon p in c.Polygons) p.FindRivers();
    }

    public static River CreateRiverObject(GraphPath riverPath, PolygonMapGenerator PMG)
    {
        //Debug.Log("Creating mesh for river with " + riverPath.Nodes.Count + " points");

        // Calculate vertices of river polygon
        List<Vector2> polygonVerticesHalf1 = new List<Vector2>();
        List<Vector2> polygonVerticesHalf2 = new List<Vector2>();
        for(int i = 1; i < riverPath.Nodes.Count - 1; i++)
        {
            Vector2 startPoint = riverPath.Nodes[i - 1].Vertex;
            Vector2 thisPoint = riverPath.Nodes[i].Vertex;
            Vector2 nextPoint = riverPath.Nodes[i + 1].Vertex;

            float startWidth = riverPath.Nodes[i - 1].RiverWidth;
            float endWidth = riverPath.Nodes[i].RiverWidth;

            //Debug.Log("River point " + i + ": startWidth = " + startWidth + ", endWidth = " + endWidth);
            
            if(i == 1) // Add two starting points
            {
                Vector2 rotatedVector = GeometryFunctions.RotateVector((thisPoint - startPoint).normalized * startWidth, 90);
                polygonVerticesHalf1.Add(startPoint + rotatedVector);
                polygonVerticesHalf2.Add(startPoint - rotatedVector);
            }

            polygonVerticesHalf1.Add(GeometryFunctions.GetOffsetIntersection(startPoint, thisPoint, nextPoint, startWidth, endWidth, true));
            polygonVerticesHalf2.Add(GeometryFunctions.GetOffsetIntersection(startPoint, thisPoint, nextPoint, startWidth, endWidth, false));

            if(i == riverPath.Nodes.Count - 2) // Add two ending points (calculate river delta by taking intersecion between river and shoreline
            {
                GraphNode lastNode = riverPath.Nodes.Last();
                List<GraphConnection> shoreDelta = lastNode.Connections.Where(x => x.Type == BorderType.Shore).ToList();
                List<GraphNode> riverDeltaPoints = new List<GraphNode>();
                foreach(GraphConnection delta in shoreDelta)
                {
                    if (delta.StartNode == lastNode) riverDeltaPoints.Add(delta.EndNode);
                    else riverDeltaPoints.Add(delta.StartNode);
                }

                Vector2 endPoint1, endPoint2;

                // BUG: this method doesn't work 100%
                GraphPolygon firstPolygon = riverPath.Nodes[i].Polygons.FirstOrDefault(x => GeometryFunctions.IsPointInPolygon4(x.Nodes.Select(x => x.Vertex).ToList(), polygonVerticesHalf1.Last()));
                if(firstPolygon == null)
                {
                    throw new Exception("Couldn't find direction of river delta. is river too short? length = " + riverPath.Nodes.Count);
                }

                bool addDeltaMidPoint = true;
                if(riverDeltaPoints[0].Polygons.Contains(firstPolygon))
                {
                    endPoint1 = GeometryFunctions.GetOffsetIntersection(thisPoint, nextPoint, riverDeltaPoints[0].Vertex, endWidth, 0f, true);
                    endPoint2 = GeometryFunctions.GetOffsetIntersection(thisPoint, nextPoint, riverDeltaPoints[1].Vertex, endWidth, 0f, false);

                    if (!GeometryFunctions.IsPointOnLineSegment(endPoint1, riverDeltaPoints[0].Vertex, lastNode.Vertex))
                    {
                        endPoint1 = lastNode.Vertex;
                        addDeltaMidPoint = false;
                    }
                    if (!GeometryFunctions.IsPointOnLineSegment(endPoint2, riverDeltaPoints[1].Vertex, lastNode.Vertex))
                    {
                        endPoint2 = lastNode.Vertex;
                        addDeltaMidPoint = false;
                    }
                }
                else
                {
                    endPoint1 = GeometryFunctions.GetOffsetIntersection(thisPoint, nextPoint, riverDeltaPoints[1].Vertex, endWidth, 0f, true);
                    endPoint2 = GeometryFunctions.GetOffsetIntersection(thisPoint, nextPoint, riverDeltaPoints[0].Vertex, endWidth, 0f, false);

                    if (!GeometryFunctions.IsPointOnLineSegment(endPoint1, riverDeltaPoints[1].Vertex, lastNode.Vertex))
                    {
                        endPoint1 = lastNode.Vertex;
                        addDeltaMidPoint = false;
                    }
                    if (!GeometryFunctions.IsPointOnLineSegment(endPoint2, riverDeltaPoints[0].Vertex, lastNode.Vertex))
                    {
                        endPoint2 = lastNode.Vertex;
                        addDeltaMidPoint = false;
                    }
                }

                polygonVerticesHalf1.Add(endPoint1);
                if (addDeltaMidPoint) polygonVerticesHalf1.Add(lastNode.Vertex);
                polygonVerticesHalf2.Add(endPoint2);
            }
        }

        polygonVerticesHalf2.Reverse();
        List<Vector2> polygonVertices = polygonVerticesHalf1;
        polygonVertices.AddRange(polygonVerticesHalf2);

        List<Vector2> polygonVerticesList = polygonVertices.ToList();
        if (GeometryFunctions.IsClockwise(polygonVerticesList)) polygonVerticesList.Reverse();

        // Create object 
        GameObject riverObject = MeshGenerator.GeneratePolygon(polygonVerticesList, PMG, layer: PolygonMapGenerator.LAYER_RIVER);

        River river = riverObject.AddComponent<River>();
        river.Init(riverPath.Nodes.Select(x => x.BorderPoint).ToList(), riverPath.Connections.Select(x => x.Border).ToList(), riverPath.Polygons.Select(x => x.Region).ToList());

        riverObject.GetComponent<MeshRenderer>().material.color = Color.red;

        riverObject.name = "River";

        return river;
    }
}
