using UnityEngine;

[System.Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }
}