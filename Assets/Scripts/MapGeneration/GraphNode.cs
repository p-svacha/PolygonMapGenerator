using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Networking;

public class GraphNode
{
    public static int idCounter = 0;

    public int Id;
    public Vector2 Vertex;

    public bool IsEdgeNode;

    public List<GraphNode> ConnectedNodes = new List<GraphNode>();
    public List<GraphConnection> Connections = new List<GraphConnection>();
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public List<GraphNode> VisitedNeighbours = new List<GraphNode>(); // Used for performant polygon finding

    public BorderPoint BorderPoint; // corresponding visual border point on the map

    public GraphNode(Vector2 v)
    {
        Id = idCounter++;
        Vertex = v;

        IsEdgeNode = (v.x == 0 || v.x == PolygonMapGenerator.MAP_WIDTH || v.y == 0 || v.y == PolygonMapGenerator.MAP_HEIGHT);
    }

    /// <summary>
    /// Returns a list containing all angles of connections of this node
    /// </summary>
    /// <returns></returns>
    public List<int> GetNodeAngles()
    {
        List<int> nodeAngles = new List<int>();
        foreach (GraphNode n in ConnectedNodes)
        {
            float angle = Vector2.SignedAngle(new Vector2(n.Vertex.x - Vertex.x, n.Vertex.y - Vertex.y), new Vector2(0, 1));
            nodeAngles.Add((int)angle);
        }
        return nodeAngles;
    }

    public GraphConnection GetConnectionTo(GraphNode n)
    {
        return Connections.Where(x => (x.StartNode == this && x.EndNode == n) || (x.StartNode == n && x.EndNode == this)).First();
    }

    public bool IsWaterNode()
    {
        return Polygons.All(x => x.IsWater);
    }
}
