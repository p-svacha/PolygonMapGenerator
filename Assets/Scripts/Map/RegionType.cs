using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RegionType
{
    Edge, // Polygon is at map edge, always water
    Water, // Polygon is water
    Shore, // Polygon is land, at least 1 adjacent is water
    Landlocked // Polygon is land and surrounded by land
}
