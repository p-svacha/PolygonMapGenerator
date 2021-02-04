using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphAnimationType
{
    None,   // Graph is not in animation
    Init,   // Graph is animating with bars rising from the bottom with all the same speed
    Update  // Graph is animating towards new values with existing bars
}
