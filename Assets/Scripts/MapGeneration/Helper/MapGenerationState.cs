using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapGenerationState
{
    Waiting,
    CreateMapBounds,
    CreateInitialGraph,
    FindInitialPolygons,
    RemoveInvalidNodes,
    SplitBigPolygons,
    MergeSmallPolygons,
    CreateWaters,
    CreateTopology,
    CreateRivers,
    DrawMap,
    GenerationDone,
    GenerationAborted
}
