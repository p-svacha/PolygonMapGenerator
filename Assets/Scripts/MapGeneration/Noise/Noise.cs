using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Noise
{
    public abstract float GetValue(float x, float y, MapGenerationSettings settings);
}
