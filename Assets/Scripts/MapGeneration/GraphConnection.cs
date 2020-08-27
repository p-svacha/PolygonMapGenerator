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

    public GraphConnectionType Type;

    public Border Border;

    public GraphConnection(GraphNode start, GraphNode end)
    {
        StartNode = start;
        EndNode = end;

        Type = (start.IsEdgeNode || end.IsEdgeNode) ? GraphConnectionType.Edge : GraphConnectionType.Inland;
    }

    public void SetNeighboursAndType()
    {
        Connections.Clear();

        // Set connected connections
        foreach(GraphConnection c in StartNode.Connections)
        {
            if (c != this && !Connections.Contains(c)) Connections.Add(c);
        }
        foreach (GraphConnection c in EndNode.Connections)
        {
            if (c != this && !Connections.Contains(c)) Connections.Add(c);
        }

        // Set type
        if (StartNode.IsEdgeNode || EndNode.IsEdgeNode) Type = GraphConnectionType.Edge;
        else if (Polygons.All(x => x.IsWater)) Type = GraphConnectionType.Water;
        else if (Polygons.All(x => !x.IsWater)) Type = GraphConnectionType.Inland;
        else Type = GraphConnectionType.Shore;

    }

    public override string ToString()
    {
        return StartNode.Vertex.ToString() + "-" + EndNode.Vertex.ToString();
    }

}
