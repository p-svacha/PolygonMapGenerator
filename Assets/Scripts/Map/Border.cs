using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Border : MonoBehaviour
{
    public BorderPoint StartPoint;
    public BorderPoint EndPoint;

    public List<Region> Regions = new List<Region>();
    public River River;

    public Vector2 Center;
    public float Angle;
    public float Length;

    public const float BorderHeight = 0.05f;
    public const float BorderWidth = 0.01f;

    public void Init(BorderPoint start, BorderPoint end, List<Region> regions)
    {
        StartPoint = start;
        EndPoint = end;
        Regions = regions;

        Center = new Vector2((start.Position.x + end.Position.x) / 2, (start.Position.y + end.Position.y) / 2);
        Length = Vector2.Distance(start.Position, end.Position);
        Angle = Vector2.SignedAngle((end.Position - start.Position), new Vector2(1,0));

        this.transform.position = new Vector3(Center.x, BorderHeight * 0.5f, Center.y);
        this.transform.rotation = Quaternion.Euler(0, Angle, 0);
        this.transform.localScale = new Vector3(Length, BorderHeight, BorderWidth);
        
    }
}
