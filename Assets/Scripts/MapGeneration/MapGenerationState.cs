using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapGenerationState
{
    Waiting,
    CreateInitialGraph,
    FindInitialPolygons,
    RemoveInvalidNodes,
    SplitBigPolygons,
    MergeSmallPolygons,
    CreateWaters,
    DrawMap,
    GenerationDone,
    GenerationAborted
}
