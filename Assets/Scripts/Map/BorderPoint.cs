using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BorderPoint : MonoBehaviour
{
    public Map Map;

    public const float Scale = 0.08f;
    public Vector2 Position;

    public River River;
    public float RiverWidth;

    public BorderPointType Type;
    public int DistanceFromNearestOcean;

    public List<BorderPoint> ConnectedBorderPoints;
    public List<Border> Borders;
    public List<Region> Regions;

    public void Init(Map map, GraphNode n)
    {
        Map = map;

        Position = n.Vertex;
        transform.position = new Vector3(n.Vertex.x, 0, n.Vertex.y);
        transform.localScale = new Vector3(Scale, Scale, Scale);

        Type = n.Type;
        DistanceFromNearestOcean = n.DistanceFromNearestOcean;

        ConnectedBorderPoints = n.ConnectedNodes.Select(x => x.BorderPoint).ToList();
        Borders = n.Connections.Select(x => x.Border).ToList();
        Regions = n.Polygons.Select(x => x.Region).ToList();

        RiverWidth = n.RiverWidth;
    }
}
