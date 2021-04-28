using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterConnection : MonoBehaviour
{
    public Region FromRegion;
    public Region ToRegion;

    public void Init(Region from, Region to)
    {
        FromRegion = from;
        ToRegion = to;
    }

    public void SetVisible(bool visible)
    {
        GetComponent<MeshRenderer>().enabled = visible;
    }
}
