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
    public static List<List<GraphPolygon>> FindClusters(List<GraphPolygon> polygons, bool landConnectionsOnly = true)
    {
        List<List<GraphPolygon>> clusters = new List<List<GraphPolygon>>();

        List<GraphPolygon> polygonsWithoutCluster = new List<GraphPolygon>();
        polygonsWithoutCluster.AddRange(polygons);

        while (polygonsWithoutCluster.Count > 0)
        {
            List<GraphPolygon> cluster = new List<GraphPolygon>();
            Queue<GraphPolygon> polygonsToAdd = new Queue<GraphPolygon>();
            polygonsToAdd.Enqueue(polygonsWithoutCluster[0]);
            while (polygonsToAdd.Count > 0)
            {
                GraphPolygon polygonToAdd = polygonsToAdd.Dequeue();
                cluster.Add(polygonToAdd);
                List<GraphPolygon> neighbouringPolygons = landConnectionsOnly ? polygonToAdd.LandNeighbours.Where(x => polygons.Contains(x)).ToList() : polygonToAdd.Neighbours.Where(x => polygons.Contains(x)).ToList();
                foreach (GraphPolygon neighbourPolygon in neighbouringPolygons)
                    if (!cluster.Contains(neighbourPolygon) && !polygonsToAdd.Contains(neighbourPolygon))
                        polygonsToAdd.Enqueue(neighbourPolygon);
            }
            clusters.Add(cluster);
            foreach (GraphPolygon p in cluster) polygonsWithoutCluster.Remove(p);
        }

        return clusters;
    }

    public static List<List<Region>> FindClusters(List<Region> regions, bool landConnectionsOnly = true)
    {
        return FindClusters(regions.Select(x => x.Polygon).ToList()).Select(x => x.Select(y => y.Region).ToList()).ToList();
    }

    /// <summary>
    /// Returns a list with the two polygons that are closest to each other. First polygon is from fromPolygons, second polygon is from toPolygons
    /// </summary>
    public static List<GraphPolygon> FindClosestPolygons(List<GraphPolygon> fromPolygons, List<GraphPolygon> toPolygons)
    {
        float shortestCentroidDistance = float.MaxValue;
        float shortestPolygonDistance = float.MaxValue;
        List<GraphPolygon> shortestPair = new List<GraphPolygon>();

        foreach(GraphPolygon from in fromPolygons)
        {
            foreach(GraphPolygon to in toPolygons)
            {
                float centroidDistance = Vector2.Distance(from.Centroid, to.Centroid);
                if(centroidDistance < (shortestCentroidDistance + 2f)) // if we are close to shortest distance, take detailed distance checking between all points
                {
                    float polygonDistance = GetPolygonDistance(from, to);
                    if(polygonDistance < shortestPolygonDistance)
                    {
                        shortestCentroidDistance = centroidDistance;
                        shortestPolygonDistance = polygonDistance;
                        shortestPair = new List<GraphPolygon>() { from, to };
                    }
                }
            }
        }
        return shortestPair;
    }

    /// <summary>
    /// Returns the shortest distance between two polygons (distance between two closest points)
    /// </summary>
    public static float GetPolygonDistance(GraphPolygon p1, GraphPolygon p2)
    {
        float shortestDistance = float.MaxValue;
        foreach(GraphNode fromNode in p1.Nodes)
        {
            foreach(GraphNode toNode in p2.Nodes)
            {
                float distance = Vector2.Distance(fromNode.Vertex, toNode.Vertex);
                if(distance < shortestDistance)
                {
                    shortestDistance = distance;
                }
            }
        }
        return shortestDistance;
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
