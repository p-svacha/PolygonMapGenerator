using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapType
{
    Regional, // Uses constructed map generation (with continent cuts, ball oceans, land/water expansion) without island restriction
    Island, // Uses constructed map generation (with continent cuts, ball oceans, land/water expansion) with island restriction
    FractalNoise, // Uses ridged multifractal noise to deicde where land and where water is
    BigOceans // Doesn't split big polygons and turns them into ocean instead
}
