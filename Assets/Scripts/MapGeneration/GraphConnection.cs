using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphConnection
{
    public GraphNode StartNode;
    public GraphNode EndNode;

    public List<GraphConnection> Connections = new List<GraphConnection>();
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public bool IsEdgeConnection;

    public Border Border;

    public GraphConnection(GraphNode start, GraphNode end)
    {
        StartNode = start;
        EndNode = end;

        IsEdgeConnection = start.IsEdgeNode || end.IsEdgeNode;
    }

    public void SetNeighbours()
    {
        Connections.Clear();

        foreach(GraphConnection c in StartNode.Connections)
        {
            if (c != this && !Connections.Contains(c)) Connections.Add(c);
        }
        foreach (GraphConnection c in EndNode.Connections)
        {
            if (c != this && !Connections.Contains(c)) Connections.Add(c);
        }
    }

    public bool IsShore()
    {
        return (!IsEdgeConnection && Polygons.Where(x => x.IsWater).Count() == 1);
    }

    public override string ToString()
    {
        return StartNode.Vertex.ToString() + "-" + EndNode.Vertex.ToString();
    }

}
