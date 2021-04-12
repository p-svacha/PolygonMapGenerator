using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class contains static functions that can be used for any polygon map.
/// </summary>
public static class PolygonMapFunctions
{
    /// <summary>
    /// Takes a list of regions as an input and splits them into clusters. A cluster is defined as each region is reachable from each other region in the cluster through direct land connections.
    /// </summary>
    public static List<List<Region>> FindClusters(List<Region> regions)
    {
        List<List<Region>> clusters = new List<List<Region>>();

        List<Region> regionsWithoutCluster = new List<Region>();
        regionsWithoutCluster.AddRange(regions);

        while (regionsWithoutCluster.Count > 0)
        {
            List<Region> cluster = new List<Region>();
            Queue<Region> regionsToAdd = new Queue<Region>();
            regionsToAdd.Enqueue(regionsWithoutCluster[0]);
            while (regionsToAdd.Count > 0)
            {
                Region regionToAdd = regionsToAdd.Dequeue();
                cluster.Add(regionToAdd);
                foreach (Region neighbourRegion in regionToAdd.AdjacentRegions.Where(x => !x.IsWater && regions.Contains(x)))
                    if (!cluster.Contains(neighbourRegion) && !regionsToAdd.Contains(neighbourRegion))
                        regionsToAdd.Enqueue(neighbourRegion);
            }
            clusters.Add(cluster);
            foreach (Region r in cluster) regionsWithoutCluster.Remove(r);
        }

        return clusters;
    }
}
