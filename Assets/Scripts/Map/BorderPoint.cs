using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderPoint : MonoBehaviour
{
    public const float Scale = 0.08f;
    public Vector2 Position;

    public void Init(Vector2 pos)
    {
        Position = pos;
        this.transform.position = new Vector3(pos.x, 0, pos.y);
        this.transform.localScale = new Vector3(Scale, Scale, Scale);
    }
}
