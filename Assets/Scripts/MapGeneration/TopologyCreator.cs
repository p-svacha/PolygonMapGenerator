using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TopologyCreator
{
    // Start is called before the first frame update
    public static void CreateTopology(PolygonMapGenerator PMG)
    {
        CreateLandTopology(PMG);
        CreateWaterTopology(PMG);
        FindNodeOceanDistances(PMG);
    }

    private static void CreateLandTopology(PolygonMapGenerator PMG)
    {
        // Land altitude is >= 1
        List<GraphPolygon> firstLevelPolygons = PMG.Polygons.Where(x => !x.IsWater && x.IsNextToWater).ToList();
        ApplyLandTopologyLevel(firstLevelPolygons, 1);
    }

    private static void ApplyLandTopologyLevel(List<GraphPolygon> levelPolygons, int altitude)
    {
        HashSet<GraphPolygon> nextLevelPolygons = new HashSet<GraphPolygon>();
        foreach (GraphPolygon p in levelPolygons) p.DistanceFromNearestWater = altitude;
        foreach (GraphPolygon p in levelPolygons)
        {
            foreach (GraphPolygon neighbour in p.AdjacentPolygons.Where(x => !x.IsWater && x.DistanceFromNearestWater == 0)) nextLevelPolygons.Add(neighbour);
        }
        if(nextLevelPolygons.Count > 0) ApplyLandTopologyLevel(nextLevelPolygons.ToList(), altitude + 1);
    }

    private static void CreateWaterTopology(PolygonMapGenerator PMG)
    {
        // Water altitude is <= -1
        List<GraphPolygon> firstLevelPolygons = PMG.Polygons.Where(x => x.IsWater && x.IsNextToLand()).ToList();
        ApplyWaterTopologyLevel(firstLevelPolygons, -1);
    }

    private static void ApplyWaterTopologyLevel(List<GraphPolygon> levelPolygons, int altitude)
    {
        HashSet<GraphPolygon> nextLevelPolygons = new HashSet<GraphPolygon>();
        foreach (GraphPolygon p in levelPolygons) p.DistanceFromNearestWater = altitude;
        foreach (GraphPolygon p in levelPolygons)
        {
            foreach (GraphPolygon neighbour in p.AdjacentPolygons.Where(x => x.IsWater && x.DistanceFromNearestWater == 0)) nextLevelPolygons.Add(neighbour);
        }
        if (nextLevelPolygons.Count > 0) ApplyWaterTopologyLevel(nextLevelPolygons.ToList(), altitude - 1);
    }

    private static void FindNodeOceanDistances(PolygonMapGenerator PMG)
    {
        List<GraphNode> firstLevelNodes = PMG.Nodes.Where(x => x.Type == BorderPointType.Shore).ToList();
        ApplyNodeDepthLevel(firstLevelNodes, 1);
    }
    private static void ApplyNodeDepthLevel(List<GraphNode> levelNodes, int distance)
    {
        HashSet<GraphNode> nextLevelNodes = new HashSet<GraphNode>();
        foreach (GraphNode n in levelNodes) n.DistanceFromNearestOcean = distance;
        foreach (GraphNode n in levelNodes)
        {
            foreach (GraphNode neighbour in n.ConnectedNodes.Where(x => x.Type != BorderPointType.Water && x.DistanceFromNearestOcean == 0)) nextLevelNodes.Add(neighbour);
        }
        if (nextLevelNodes.Count > 0) ApplyNodeDepthLevel(nextLevelNodes.ToList(), distance + 1);
    }
}
