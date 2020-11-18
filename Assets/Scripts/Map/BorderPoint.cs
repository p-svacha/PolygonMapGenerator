using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderPoint : MonoBehaviour
{
    public const float Scale = 0.08f;
    public Vector2 Position;

    public River River;
    public float RiverWidth;

    public BorderPointType Type;
    public int DistanceFromNearestOcean;

    public void Init(GraphNode n)
    {
        Position = n.Vertex;
        transform.position = new Vector3(n.Vertex.x, 0, n.Vertex.y);
        transform.localScale = new Vector3(Scale, Scale, Scale);

        Type = n.Type;
        DistanceFromNearestOcean = n.DistanceFromNearestOcean;

        RiverWidth = n.RiverWidth;
    }
}
