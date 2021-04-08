using ClipperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GeometryFunctions;

public static class RiverCreator
{
    private static float START_RIVER_SIZE = 0.02f;
    private static int SIZE_PER_RIVER = 23;
    private static int SIZE_PER_RIVER_RANGE = 6;
    private static float MIN_RIVER_EXPANSION_RATE = 0.001f;
    private static float MAX_RIVER_EXPANSION_RATE = 0.004f;
    private static float MIN_RIVER_MAX_WIDTH = 0.065f;
    private static float MAX_RIVER_MAX_WIDTH = 0.11f;

    public static void CreateRivers(PolygonMapGenerator PMG)
    {
        int sizePerRiver = UnityEngine.Random.Range(SIZE_PER_RIVER - SIZE_PER_RIVER_RANGE, SIZE_PER_RIVER + SIZE_PER_RIVER_RANGE);
        int numRivers = PMG.MapSize / sizePerRiver;
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
        lastEndPoint.RiverWidth = 0;
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
        Debug.Log("Creating mesh for river with " + riverPath.Nodes.Count + " points");
        Vector2[] vertices = new Vector2[riverPath.Nodes.Count * 2 - 1];
        List<int> triangles = new List<int>();

        List<IntPoint> path = new List<IntPoint>();
        foreach(Vector2 v in riverPath.Nodes.Select(x => x.Vertex))
            path.Add(new IntPoint(v.x, v.y));
        ClipperOffset offsetter = new ClipperOffset();
        offsetter.AddPath(path, JoinType.jtSquare, EndType.etClosedPolygon);
        PolyTree polyTree = new PolyTree();
        offsetter.Execute(ref polyTree, riverPath.Nodes[1].RiverWidth);

        List<Vector2> polygonVertices = new List<Vector2>();
        foreach (IntPoint p in polyTree.Childs[0].m_polygon)
            polygonVertices.Add(new Vector2((float)p.X, (float)p.Y));

        //MeshGenerator.GeneratePolygon(polygonVertices, PMG);

        for (int i = 0; i < riverPath.Nodes.Count; i++)
        {
            if (i == 0)
            {
                vertices[0] = riverPath.Nodes[i].Vertex;
                triangles.Add(0);
                triangles.Add(2);
                triangles.Add(1);
            }
            else
            {
                Vector2 lastPosition = riverPath.Nodes[i - 1].Vertex;
                Vector2 currentPointPosition = riverPath.Nodes[i].Vertex;
                Vector2 nextPointPosition;
                if (i == riverPath.Nodes.Count - 1)
                {
                    nextPointPosition = currentPointPosition + new Vector2(currentPointPosition.x - lastPosition.x, currentPointPosition.y - lastPosition.y);
                }
                else
                {
                    nextPointPosition = riverPath.Nodes[i + 1].Vertex;
                }

                float beforeConnectionAngle = Vector2.SignedAngle(new Vector2(lastPosition.x - currentPointPosition.x, lastPosition.y - currentPointPosition.y), new Vector2(0, 1));
                float afterConnectionAngle = Vector2.SignedAngle(new Vector2(nextPointPosition.x - currentPointPosition.x, nextPointPosition.y - currentPointPosition.y), new Vector2(0, 1));

                beforeConnectionAngle = mod(beforeConnectionAngle, 360);
                afterConnectionAngle = mod(afterConnectionAngle, 360);

                Debug.Log("angles for point " + i + ": " + beforeConnectionAngle + ", " + afterConnectionAngle);

                if (beforeConnectionAngle > afterConnectionAngle) afterConnectionAngle += 360;

                float meshAngle1 = mod(beforeConnectionAngle + ((afterConnectionAngle - beforeConnectionAngle) / 2), 360);
                float meshAngle2 = mod(meshAngle1 + 180, 360);

                Vector2 meshVertex1 = new Vector2(
                    (float)(currentPointPosition.x + (Math.Sin(ToRad(meshAngle1)) * riverPath.Nodes[i].RiverWidth / 2)),
                    (float)(currentPointPosition.y + (Math.Cos(ToRad(meshAngle1)) * riverPath.Nodes[i].RiverWidth / 2)));
                Vector2 meshVertex2 = new Vector2(
                    (float)(currentPointPosition.x + (Math.Sin(ToRad(meshAngle2)) * riverPath.Nodes[i].RiverWidth / 2)),
                    (float)(currentPointPosition.y + (Math.Cos(ToRad(meshAngle2)) * riverPath.Nodes[i].RiverWidth / 2)));

                vertices[i * 2 - 1] = meshVertex1;
                vertices[i * 2] = meshVertex2;

                if(i > 1)
                {
                    triangles.Add(((i - 1) * 2) - 1);
                    triangles.Add(((i - 1) * 2));
                    triangles.Add(((i - 1) * 2) + 1);

                    triangles.Add(((i - 1) * 2));
                    triangles.Add(((i - 1) * 2) + 2);
                    triangles.Add(((i - 1) * 2) + 1);
                }
            }
        }

        Debug.Log("River vertices");
        foreach (Vector2 v in vertices) Debug.Log(v.ToString());

        // Create mesh
        // Create the Vector3 vertices
        Vector3[] vertices3D = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, 0, vertices[i].y);
            uvs[i] = new Vector2(vertices[i].x / PMG.Width, vertices[i].y / PMG.Height);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices3D;
        msh.uv = uvs;
        msh.triangles = triangles.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        GameObject riverObject = new GameObject("River");
        string name = MarkovChainWordGenerator.GetRandomName(maxLength: 16) + " River";
        River river = riverObject.AddComponent<River>();
        river.Init(name, riverPath.Nodes.Select(x => x.BorderPoint).ToList(), riverPath.Connections.Select(x => x.Border).ToList(), riverPath.Polygons.Select(x => x.Region).ToList());

        riverObject.transform.position = new Vector3(0, 0.001f, 0);
        MeshRenderer renderer = riverObject.AddComponent<MeshRenderer>();
        Color ranCol = Color.red;
        renderer.material.color = ranCol;
        MeshFilter filter = riverObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        riverObject.transform.localScale = new Vector3(1, -1, 1);
        riverObject.name = "River";

        return river;
    }
}
