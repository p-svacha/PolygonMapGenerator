using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphPolygon
{
    public List<GraphNode> Nodes = new List<GraphNode>();
    public List<GraphConnection> Connections = new List<GraphConnection>();
    public List<GraphPolygon> Neighbours = new List<GraphPolygon>();
    public List<GraphPath> Rivers = new List<GraphPath>();

    public bool IsEdgePolygon;

    public float Width;
    public float Height;
    public float Area;
    public float Jaggedness; // How close the shape is to a perfect rectangle

    public bool IsWater;
    public bool IsNextToWater;
    public bool HasRiver;

    public Region Region;

    public GraphPolygon(List<GraphNode> nodes, List<GraphConnection> connections)
    {
        Nodes = nodes;
        Connections = connections;
        Area = GeometryFunctions.GetPolygonArea(nodes.Select(x => x.Vertex).ToList());

        IsEdgePolygon = nodes.Any(x => x.Type == BorderPointType.Edge);

        Width = nodes.Max(x => x.Vertex.x) - nodes.Min(x => x.Vertex.x);
        Height = nodes.Max(x => x.Vertex.y) - nodes.Min(x => x.Vertex.y);
        Jaggedness = 1f - (Area / (Width * Height));
    }

    public void SetNeighbours()
    {
        Neighbours.Clear();

        foreach (GraphConnection c in Connections)
        {
            foreach (GraphPolygon p in c.Polygons)
            {
                if (p != this && !Neighbours.Contains(p)) Neighbours.Add(p);
            }
        }
    }

    public void FindRivers()
    {
        HasRiver = Connections.Any(x => x.River != null);
    }

    public bool IsNextToLand()
    {
        return Neighbours.Any(x => !x.IsWater);
    }
}
