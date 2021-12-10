using MapGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Used to make maps saveable and loadable. The object respresents a serializable version of all data needed to draw a map.
/// Can be extracted from and fed back into a <see cref="PolygonMapGenerator"/>
/// UNTESTED AND UNUSED! JUST USE THE SEED FOR NOW!
/// </summary>
[Serializable]
public static class MapHasher
{
    public static string GetMapHash(PolygonMapGenerator PMG)
    {
        StringBuilder hash = new StringBuilder();

        // Line 0 (GenerationSettings.*): seed;width;height;minPolygonArea;maxPolygonArea;minContinentSize;maxContinentSize;mapType;continentSizeScale
        MapGenerationSettings mgs = PMG.GenerationSettings;
        hash.AppendLine(mgs.Seed + ";" + mgs.Width + ";" + mgs.Height + ";" + mgs.MinPolygonArea + ";" + mgs.MaxPolygonArea + ";" + mgs.MinContinentSize + ";" + mgs.MaxContinentSize + ";" + (int)mgs.MapType + ";" + mgs.ContinentSizeScaleFactor);

        // Line 1: n_nodes;n_ingraphconnections;n_egdeconnections;n_polygons;n_paths;n_landmasses;n_waterbodies;n_continents
        hash.AppendLine(PMG.Nodes.Count + ";" + PMG.InGraphConnections.Count + ";" + PMG.EdgeConnections.Count + ";" + PMG.Polygons.Count + ";" + PMG.RiverPaths.Count + ";" + PMG.Landmasses.Count + ";" + PMG.WaterBodies.Count + ";" + PMG.Continents.Count);

        // Node Lines: id;positionX;positionY;riverWidth;distanceFromNearestOcean
        foreach (GraphNode n in PMG.Nodes) hash.AppendLine(n.Id + ";" + n.Vertex.x + ";" + n.Vertex.y + ";" + n.RiverWidth + ";" + n.DistanceFromNearestOcean);

        // InGraphConnection Lines: id;fromNodeId;toNodeId;riverWidth
        foreach (GraphConnection c in PMG.InGraphConnections) hash.AppendLine(c.Id + ";" + c.StartNode.Id + ";" + c.EndNode.Id + ";" + c.RiverWidth);

        // EdgeConnection Lines: id;fromNodeId;toNodeId;riverWidth
        foreach (GraphConnection c in PMG.EdgeConnections) hash.AppendLine(c.Id + ";" + c.StartNode.Id + ";" + c.EndNode.Id + ";" + c.RiverWidth);

        // Polygon Lines: id;[nodeIds];[connectionIds]
        foreach(GraphPolygon p in PMG.Polygons)
        {
            hash.Append(p.Id + ";");
            foreach (GraphNode n in p.Nodes) hash.Append(n.Id + ",");
            hash.Append(";");
            foreach (GraphConnection c in p.Connections) hash.Append(c.Id + ",");
            hash.AppendLine();
        }

        // River Path Lines: id;[nodeIds];[connectionIds];[polygonIds]
        foreach(GraphPath r in PMG.RiverPaths)
        {
            hash.Append(r.Id + ";");
            foreach (GraphNode n in r.Nodes) hash.Append(n.Id + ",");
            hash.Append(";");
            foreach (GraphConnection c in r.Connections) hash.Append(c.Id + ",");
            hash.Append(";");
            foreach (GraphPolygon p in r.Polygons) hash.Append(p.Id + ",");
            hash.AppendLine();
        }

        // Landmass Lines: [polygonIds]
        foreach(List<GraphPolygon> list in PMG.Landmasses)
        {
            foreach (GraphPolygon p in list) hash.Append("p.Id" + ",");
            hash.AppendLine();
        }

        // Water Body Lines: [polygonIds]
        foreach (List<GraphPolygon> list in PMG.WaterBodies)
        {
            foreach (GraphPolygon p in list) hash.Append("p.Id" + ",");
            hash.AppendLine();
        }

        // Continent Lines: [polygonIds]
        foreach (List<GraphPolygon> list in PMG.Continents)
        {
            foreach (GraphPolygon p in list) hash.Append("p.Id" + ",");
            hash.AppendLine();
        }

        return hash.ToString();
    }

    public static Map LoadMapFromHash(PolygonMapGenerator PMG, string mapHash)
    {
        string[] lines = mapHash.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        int lineIndex = 0;

        // Line 0: MapGenerationSettings: seed;width;height;minPolygonArea;maxPolygonArea;minContinentSize;maxContinentSize;mapType;continentSizeScale
        int debugI = 0;
        Debug.Log(debugI++);
        string[] mgsLine = lines[lineIndex++].Split(';');
        int seed = int.Parse(mgsLine[0]);
        int width = int.Parse(mgsLine[1]);
        int height = int.Parse(mgsLine[2]);
        float minPolygonArea = float.Parse(mgsLine[3]);
        float maxPolygonArea = float.Parse(mgsLine[4]);
        int minContinentSize = int.Parse(mgsLine[5]);
        int maxContinentSize = int.Parse(mgsLine[6]);
        MapType mapType = (MapType)int.Parse(mgsLine[7]);
        float continentSizeScale = float.Parse(mgsLine[8]);
        PMG.GenerationSettings = new MapGenerationSettings(seed, width, height, minPolygonArea, maxPolygonArea, minContinentSize, maxContinentSize, mapType, continentSizeScale);

        Debug.Log(debugI++);
        // Line 1: n_nodes;n_ingraphconnections;n_egdeconnections;n_polygons;n_paths;n_landmasses;n_waterbodies;n_continents
        string[] line1 = lines[lineIndex++].Split(';');
        int n_nodes = int.Parse(line1[0]);
        int n_inGraphConnections = int.Parse(line1[1]);
        int n_edgeConnections = int.Parse(line1[2]);
        int n_polygons = int.Parse(line1[3]);
        int n_riverPaths = int.Parse(line1[4]);
        int n_landmasses = int.Parse(line1[5]);
        int n_waterBodies = int.Parse(line1[6]);
        int n_continents = int.Parse(line1[7]);

        Debug.Log(debugI++);
        // Node Lines: id;positionX;positionY;riverWidth;distanceFromNearestOcean
        PMG.Nodes.Clear();
        Dictionary<int, GraphNode> nodeMap = new Dictionary<int, GraphNode>();
        for(int i = 0; i < n_nodes; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            int id = int.Parse(attributes[0]);
            float x = float.Parse(attributes[1]);
            float y = float.Parse(attributes[2]);
            float riverWidth = float.Parse(attributes[3]);
            int distanceFromNearestOcean = int.Parse(attributes[4]);
            GraphNode node = new GraphNode(id, x, y, riverWidth, distanceFromNearestOcean);
            nodeMap.Add(id, node);
            PMG.Nodes.Add(node);
        }

        Debug.Log(debugI++);
        // InGraphConnection Lines: id;fromNodeId;toNodeId;riverWidth
        PMG.InGraphConnections.Clear();
        Dictionary<int, GraphConnection> connectionMap = new Dictionary<int, GraphConnection>();
        for(int i = 0; i < n_inGraphConnections; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            int id = int.Parse(attributes[0]);
            int startNodeId = int.Parse(attributes[1]);
            int endNodeId = int.Parse(attributes[2]);
            float riverWidth = float.Parse(attributes[3]);
            GraphConnection connection = new GraphConnection(id, nodeMap[startNodeId], nodeMap[endNodeId], riverWidth);
            connectionMap.Add(id, connection);
            PMG.InGraphConnections.Add(connection);
        }

        Debug.Log(debugI++);
        // EdgeConnection Lines: id;fromNodeId;toNodeId;riverWidth
        PMG.EdgeConnections.Clear();
        for (int i = 0; i < n_edgeConnections; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            int id = int.Parse(attributes[0]);
            int startNodeId = int.Parse(attributes[1]);
            int endNodeId = int.Parse(attributes[2]);
            float riverWidth = float.Parse(attributes[3]);
            GraphConnection connection = new GraphConnection(id, nodeMap[startNodeId], nodeMap[endNodeId], riverWidth);
            connectionMap.Add(id, connection);
            PMG.EdgeConnections.Add(connection);
        }

        Debug.Log(debugI++);
        // Polygon Lines: id;[nodeIds];[connectionIds]
        PMG.Polygons.Clear();
        Dictionary<int, GraphPolygon> polygonMap = new Dictionary<int, GraphPolygon>();
        for(int i = 0; i < n_polygons; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            int id = int.Parse(attributes[0]);

            List<GraphNode> nodes = new List<GraphNode>();
            string[] nodeIds = attributes[1].Split(',');
            foreach (string s in nodeIds) nodes.Add(nodeMap[int.Parse(s)]);

            List<GraphConnection> connections = new List<GraphConnection>();
            string[] connectionIds = attributes[2].Split(',');
            foreach (string s in connectionIds) connections.Add(connectionMap[int.Parse(s)]);

            GraphPolygon polygon = new GraphPolygon(id, nodes, connections);
            polygonMap.Add(id, polygon);
            PMG.Polygons.Add(polygon);
        }

        Debug.Log(debugI++);
        // River Path Lines: id;[nodeIds];[connectionIds];[polygonIds]
        PMG.RiverPaths.Clear();
        for (int i = 0; i < n_riverPaths; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            int id = int.Parse(attributes[0]);

            List<GraphNode> nodes = new List<GraphNode>();
            string[] nodeIds = attributes[1].Split(',');
            foreach (string s in nodeIds) nodes.Add(nodeMap[int.Parse(s)]);

            List<GraphConnection> connections = new List<GraphConnection>();
            string[] connectionIds = attributes[2].Split(',');
            foreach (string s in connectionIds) connections.Add(connectionMap[int.Parse(s)]);

            List<GraphPolygon> polygons = new List<GraphPolygon>();
            string[] polygonIds = attributes[3].Split(',');
            foreach (string s in polygonIds) polygons.Add(polygonMap[int.Parse(s)]);

            GraphPath path = new GraphPath(id, nodes, connections, polygons);
            PMG.RiverPaths.Add(path);
        }

        Debug.Log(debugI++);
        // Landmass Lines: [polygonIds]
        PMG.Landmasses.Clear();
        for (int i = 0; i < n_landmasses; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            List<GraphPolygon> polygons = new List<GraphPolygon>();
            string[] polygonIds = attributes[0].Split(',');
            foreach (string s in polygonIds) polygons.Add(polygonMap[int.Parse(s)]);
            PMG.Landmasses.Add(polygons);
        }

        Debug.Log(debugI++);
        // WaterBody Lines: [polygonIds]
        PMG.WaterBodies.Clear();
        for (int i = 0; i < n_waterBodies; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            List<GraphPolygon> polygons = new List<GraphPolygon>();
            string[] polygonIds = attributes[0].Split(',');
            foreach (string s in polygonIds) polygons.Add(polygonMap[int.Parse(s)]);
            PMG.WaterBodies.Add(polygons);
        }

        Debug.Log(debugI++);
        // Continent Lines: [polygonIds]
        PMG.Continents.Clear();
        for (int i = 0; i < n_continents; i++)
        {
            string[] attributes = lines[lineIndex++].Split(';');
            List<GraphPolygon> polygons = new List<GraphPolygon>();
            string[] polygonIds = attributes[0].Split(',');
            foreach (string s in polygonIds) polygons.Add(polygonMap[int.Parse(s)]);
            PMG.Continents.Add(polygons);
        }


        return PMG.DrawMap();
    }
}
