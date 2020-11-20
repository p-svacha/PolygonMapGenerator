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
    public GraphPath River;

    public BorderType Type;
    public float RiverWidth;

    public Border Border;

    public GraphConnection(GraphNode start, GraphNode end)
    {
        StartNode = start;
        EndNode = end;

        Type = (start.Type == BorderPointType.Edge || end.Type == BorderPointType.Edge) ? BorderType.Edge : BorderType.Inland;
    }

    public void SetType()
    {
        if (Type == BorderType.Edge) return;
        else if (Polygons.All(x => x.IsWater)) Type = BorderType.Water;
        else if (Polygons.All(x => !x.IsWater)) Type = BorderType.Inland;
        else Type = BorderType.Shore;
    }

    public override string ToString()
    {
        return StartNode.Vertex.ToString() + "-" + EndNode.Vertex.ToString();
    }

}
