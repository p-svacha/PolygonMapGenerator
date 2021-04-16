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

    #region Polygon Group Border

    /// <summary>
    /// Given a list of polygons, this function returns a list of the outside borders of the polygon group. Each List<GraphNode> represents one border. The biggest List is the outside border, whereas all others represent holes (like lakes)
    /// </summary>
    public static List<List<GraphNode>> FindOutsideNodes(List<GraphPolygon> cluster)
    {
        List<List<GraphNode>> outsideNodes = new List<List<GraphNode>>();

        // Make a list of all nodes that are on a border
        List<GraphNode> borderNodes = new List<GraphNode>();
        foreach (GraphPolygon p in cluster)
            foreach (GraphNode n in p.Nodes.Where(x => !x.Polygons.All(p => cluster.Contains(p)) || x.Type == BorderPointType.Edge))
                if (!borderNodes.Contains(n)) borderNodes.Add(n);

        while (borderNodes.Count > 0)
        {
            //Debug.Log("Remainig nodes: " + borderNodes.Count);
            List<GraphNode> currentBorder = new List<GraphNode>();

            GraphNode startNode = borderNodes[0];
            currentBorder.Add(startNode);


            GraphNode prevNode = startNode;
            if (startNode.ConnectedNodes.Where(x => borderNodes.Contains(x)).FirstOrDefault() == null)
            {
                foreach (GraphNode n in borderNodes)
                {
                    n.BorderPoint.GetComponent<MeshRenderer>().material.color = Color.red;
                }
                startNode.BorderPoint.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            GraphNode currentNode = startNode.ConnectedNodes.Where(x => borderNodes.Contains(x) && startNode.GetConnectionTo(x).Polygons.Any(p => cluster.Contains(p)) && (startNode.GetConnectionTo(x).Polygons.Any(p => !cluster.Contains(p)) || x.Type == BorderPointType.Edge)).First();

            int counter = 0;
            while (currentNode != startNode && counter < 10010)
            {
                if (counter == 10000) throw new System.Exception("OVERLFOW");
                int remainingConnections = currentNode.ConnectedNodes.Where(x => borderNodes.Contains(x)).Count();
                if (remainingConnections <= 2) borderNodes.Remove(currentNode); // THIS DOESNT REALLY WORK YET: some nodes need to be kept in the list if they are part of multiple outside polygons. find out how to properly code this
                currentBorder.Add(currentNode);

                GraphNode nextNode = FindNextOutsideNode(prevNode, currentNode, cluster, borderNodes);
                prevNode = currentNode;
                currentNode = nextNode;

                counter++;
            }
            borderNodes.Remove(startNode);

            outsideNodes.Add(currentBorder);
        }

        return outsideNodes;
    }

    private static GraphNode FindNextOutsideNode(GraphNode from, GraphNode to, List<GraphPolygon> cluster, List<GraphNode> borderNodes)
    {
        // Next node must fulfill following criteria:
        // - must not be previous node
        // - must have at least 1 polygon belonging to the cluster
        // - must have at least 1 polygon not belonging to the cluster
        // - connection to node must have at least 1 polygon belonging to the cluster
        // - connection to node must have at least 1 polygon not belonging to the cluster

        float smallestAngle = float.MaxValue;
        GraphNode toNode = null;

        foreach (GraphNode connectedNode in to.ConnectedNodes.Where(x => borderNodes.Contains(x) && (to.GetConnectionTo(x).Polygons.Any(p => !cluster.Contains(p)) || x.Type == BorderPointType.Edge) && to.GetConnectionTo(x).Polygons.Any(p => cluster.Contains(p))))
        {
            if (connectedNode != from)
            {
                float angle = Vector2.SignedAngle(to.Vertex - from.Vertex, connectedNode.Vertex - to.Vertex);
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    toNode = connectedNode;
                }
            }
        }
        if (toNode == null)
        {
            from.BorderPoint.GetComponent<MeshRenderer>().material.color = Color.green;
            to.BorderPoint.GetComponent<MeshRenderer>().material.color = Color.red;
            throw new System.Exception("Couldn't find next node of ouf outside border");
        }
        return toNode;
    }

    #endregion
}
