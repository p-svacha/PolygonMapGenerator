using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BorderPointType
{
    Edge, // Node is on map edge
    Water, // All adjacent polygons are water
    Inland, // All adjacent polygons are land
    Shore, // At least 1 adjacent polygons is water, at least 1 is land
}

