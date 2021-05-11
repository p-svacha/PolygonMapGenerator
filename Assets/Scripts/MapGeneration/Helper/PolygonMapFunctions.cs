using System;
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
        return FindClusters(regions.Select(x => x.Polygon).ToList(), landConnectionsOnly).Select(x => x.Select(y => y.Region).ToList()).ToList();
    }

    /// <summary>
    /// Splits a list of borders into clusters, where in each cluster the borders are connected
    /// </summary>
    public static List<List<GraphConnection>> FindBorderClusters(List<GraphConnection> borders)
    {
        List<List<GraphConnection>> clusters = new List<List<GraphConnection>>();

        List<GraphConnection> bordersWithoutCluster = new List<GraphConnection>();
        bordersWithoutCluster.AddRange(borders);

        while (bordersWithoutCluster.Count > 0)
        {
            List<GraphConnection> cluster = new List<GraphConnection>();
            Queue<GraphConnection> bordersToAdd = new Queue<GraphConnection>();
            bordersToAdd.Enqueue(bordersWithoutCluster[0]);
            while (bordersToAdd.Count > 0)
            {
                GraphConnection borderToAdd = bordersToAdd.Dequeue();
                cluster.Add(borderToAdd);
                List<GraphConnection> connectedBorders = borderToAdd.Connections.Where(x => borders.Contains(x)).ToList();
                foreach (GraphConnection connectedBorder in connectedBorders)
                    if (!cluster.Contains(connectedBorder) && !bordersToAdd.Contains(connectedBorder))
                        bordersToAdd.Enqueue(connectedBorder);
            }
            clusters.Add(cluster);
            foreach (GraphConnection border in cluster) bordersWithoutCluster.Remove(border);
        }

        return clusters;
    }

    public static List<List<Border>> FindBorderClusters(List<Border> borders)
    {
        return FindBorderClusters(borders.Select(x => x.Connection).ToList()).Select(x => x.Select(y => y.Border).ToList()).ToList();
    }

    /// <summary>
    /// Finds and returns the center position of a list of connected borders including the angle of the border.
    /// </summary>
    public static Tuple<Vector2, float> FindBorderCenter(List<Border> borders)
    {
        // Make a dictionary with how many times each border point appears
        Dictionary<BorderPoint, int> bpOccurences = new Dictionary<BorderPoint, int>();
        foreach(Border border in borders)
        {
            if (bpOccurences.ContainsKey(border.StartPoint)) bpOccurences[border.StartPoint]++;
            else bpOccurences.Add(border.StartPoint, 1);
            if (bpOccurences.ContainsKey(border.EndPoint)) bpOccurences[border.EndPoint]++;
            else bpOccurences.Add(border.EndPoint, 1);
        }

        // Take a border point with only 1 occurence as start point
        BorderPoint currentPoint = bpOccurences.Where(x => x.Value == 1).First().Key;
        Border currentBorder = borders.Where(x => x.StartPoint == currentPoint || x.EndPoint == currentPoint).First();

        // Find center position starting with startPoint
        float currentDistance = 0f;
        float targetDistance = borders.Sum(x => x.Length) / 2f;
        while(currentDistance + currentBorder.Length < targetDistance)
        {
            currentDistance += currentBorder.Length;

            currentPoint = currentBorder.StartPoint == currentPoint ? currentBorder.EndPoint : currentBorder.StartPoint;
            currentBorder = currentPoint.Borders.Where(x => x != currentBorder && borders.Contains(x)).First();
        }

        float restDistance = targetDistance - currentDistance;
        float factor = restDistance / currentBorder.Length;

        if (currentPoint == currentBorder.StartPoint) return new Tuple<Vector2, float>(Vector2.Lerp(currentBorder.StartPoint.Position, currentBorder.EndPoint.Position, factor), currentBorder.Angle);
        else return new Tuple<Vector2, float>(Vector2.Lerp(currentBorder.EndPoint.Position, currentBorder.StartPoint.Position, factor), currentBorder.Angle);
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
    /// Returns the number of polygons that have to be traversed (through water or land neighbour) to get from p1 to p2 (neighbouring polygons have distance = 1)
    /// </summary>
    public static int GetRegionDistance(GraphPolygon p1, GraphPolygon p2)
    {
        int range = 0;
        List<GraphPolygon> rangePolygons = new List<GraphPolygon>() { p1 };
        while(!rangePolygons.Contains(p2) && range < 50)
        {
            range++;
            List<GraphPolygon> polygonsToAdd = new List<GraphPolygon>();
            foreach(GraphPolygon poly in rangePolygons)
            {
                foreach(GraphPolygon neighbour in poly.Neighbours)
                {
                    if (!rangePolygons.Contains(neighbour) && !polygonsToAdd.Contains(neighbour)) polygonsToAdd.Add(neighbour);
                }
            }
            rangePolygons.AddRange(polygonsToAdd);
        }
        return range;
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

    /// <summary>
    /// Returns a list containing the two graphnodes that are the closest to each other from p1 and p2. The first element is a node from p1, the second element a node from p2.
    /// if ignoreMultiNodes is true, only nodes that have exactly 2 polygons are considered
    /// if shoreOnly is true, only nodes that have at least one water polygon will be considered
    /// </summary>
    public static List<GraphNode> GetClosestPolygonNodes(GraphPolygon p1, GraphPolygon p2, bool ignoreMultiNodes, bool shoreOnly)
    {
        float shortestDistance = float.MaxValue;
        GraphNode n1 = null;
        GraphNode n2 = null;

        List<GraphNode> p1Nodes = p1.Nodes;
        List<GraphNode> p2Nodes = p2.Nodes;
        if(ignoreMultiNodes)
        {
            p1Nodes = p1Nodes.Where(x => x.Polygons.Count == 2).ToList();
            p2Nodes = p2Nodes.Where(x => x.Polygons.Count == 2).ToList();
        }
        if(shoreOnly)
        {
            p1Nodes = p1Nodes.Where(x => x.Type == BorderPointType.Shore).ToList();
            p2Nodes = p2Nodes.Where(x => x.Type == BorderPointType.Shore).ToList();
        }

        foreach (GraphNode fromNode in p1Nodes)
        {
            foreach (GraphNode toNode in p2Nodes)
            {
                float distance = Vector2.Distance(fromNode.Vertex, toNode.Vertex);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    n1 = fromNode;
                    n2 = toNode;
                }
            }
        }
        return new List<GraphNode>() { n1, n2 };
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
