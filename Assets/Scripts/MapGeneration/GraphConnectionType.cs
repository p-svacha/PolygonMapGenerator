using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphConnectionType
{
    Edge, // Only one adjacent polygons and it's water
    Water, // Both adjacent polygons are water
    Inland, // Both adjacent polygons are land
    Shore, // One adjacent polygon is water and the other one land
    River // Both adjacent polygons are land but it's a river
}
