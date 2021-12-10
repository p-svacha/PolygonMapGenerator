using UnityEngine;

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 v)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public override bool Equals(object obj)
    {
        if ((obj is SerializableVector3) == false)
        {
            return false;
        }

        var s = (SerializableVector3)obj;
        return x == s.x && y == s.y && z == s.z;
    }

    public override int GetHashCode()
    {
        var hashCode = 373119288;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static bool operator ==(SerializableVector3 a, SerializableVector3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(SerializableVector3 a, SerializableVector3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static implicit operator Vector3(SerializableVector3 x)
    {
        return new Vector3(x.x, x.y, x.z);
    }

    public static implicit operator SerializableVector3(Vector3 x)
    {
        return new SerializableVector3(x.x, x.y, x.z);
    }

}