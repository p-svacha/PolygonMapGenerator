using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterConnection : MonoBehaviour
{
    public Region FromRegion;
    public Region ToRegion;

    public BorderPoint FromPoint;
    public BorderPoint ToPoint;

    public float Length;
    public Vector2 Center;
    public float Angle;

    public void Init(Region from, Region to, BorderPoint fromPoint, BorderPoint toPoint)
    {
        FromRegion = from;
        ToRegion = to;
        FromPoint = fromPoint;
        ToPoint = toPoint;
        Center = Vector2.Lerp(FromPoint.Position, ToPoint.Position, 0.5f);
        Angle = Vector2.SignedAngle((toPoint.Position - fromPoint.Position), new Vector2(1, 0));
        Length = Vector2.Distance(fromPoint.Position, ToPoint.Position);
    }

    public void SetVisible(bool visible)
    {
        GetComponent<MeshRenderer>().enabled = visible;
    }
}
