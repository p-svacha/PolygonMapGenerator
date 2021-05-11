using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Border : MonoBehaviour
{
    public GraphConnection Connection;

    public BorderPoint StartPoint;
    public BorderPoint EndPoint;

    public List<Region> Regions = new List<Region>();
    public River River;

    public Vector2 Center;
    public float Angle;
    public float Length;

    public const float BorderHeight = 0.05f;
    public const float BorderWidth = 0.01f;

    public void Init(GraphConnection connection)
    {
        Connection = connection;

        StartPoint = Connection.StartNode.BorderPoint;
        EndPoint = Connection.EndNode.BorderPoint;
        Regions = Connection.Polygons.Select(x => x.Region).ToList();

        Center = new Vector2((StartPoint.Position.x + EndPoint.Position.x) / 2, (StartPoint.Position.y + EndPoint.Position.y) / 2);
        Length = Vector2.Distance(StartPoint.Position, EndPoint.Position);
        Angle = Vector2.SignedAngle((EndPoint.Position - StartPoint.Position), new Vector2(1,0));

        this.transform.position = new Vector3(Center.x, BorderHeight * 0.5f, Center.y);
        this.transform.rotation = Quaternion.Euler(0, Angle, 0);
        this.transform.localScale = new Vector3(Length, BorderHeight, BorderWidth);
        
    }
}
