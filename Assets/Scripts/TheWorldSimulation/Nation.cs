using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nation
{
    public string Name;
    public Sprite Flag;
    public Color PrimaryColor;
    public Color SecondaryColor;
    public Region Capital;

    public List<Region> Regions = new List<Region>();
    public float Area;

    public List<GameObject> NationPolygons = new List<GameObject>();

    public void AddRegion(Region region)
    {
        if (region.Nation != null) region.Nation.RemoveRegion(region);
        Regions.Add(region);
        region.SetNation(this);
        region.SetColor(PrimaryColor);
        CalculateArea();
    }

    public void RemoveRegion(Region region)
    {
        Regions.Remove(region);
        region.SetNation(null);
        CalculateArea();
    }

    private void CalculateArea()
    {
        Area = Regions.Sum(x => x.Area);
    }

    public void CreateNationPolygons()
    {
        // 1. find all distinct clusters in nation (in case the nation has multiple parts that are not directly connected)
        List<Region> unassignedRegions = new List<Region>();
        foreach (Region r in Regions) unassignedRegions.Add(r);
        List<List<Region>> regionClusters = new List<List<Region>>();
        while (unassignedRegions.Count > 0)
        {
            List<Region> searchedRegions = new List<Region>();
            Queue<Region> searchQueue = new Queue<Region>();
            List<Region> cluster = new List<Region>();
            Region startRegion = unassignedRegions[0];
            cluster.Add(startRegion);
            searchedRegions.Add(startRegion);
            unassignedRegions.Remove(startRegion);

            foreach (Region adj in startRegion.AdjacentRegions.Where(x => !searchedRegions.Contains(x))) searchQueue.Enqueue(adj);

            while(searchQueue.Count > 0)
            {
                Region searchRegion = searchQueue.Dequeue();
                searchedRegions.Add(searchRegion);
                if(Regions.Contains(searchRegion))
                {
                    cluster.Add(searchRegion);
                    unassignedRegions.Remove(searchRegion);
                    foreach (Region adj in searchRegion.AdjacentRegions.Where(x => !searchedRegions.Contains(x))) searchQueue.Enqueue(adj);
                }
            }

            regionClusters.Add(cluster);
        }

        Debug.Log("Found " + regionClusters.Count + " clusters");

        // 2. find outside edges for each cluster and draw polygon
        foreach(List<Region> regionCluster in regionClusters)
        {
            List<GraphPolygon> cluster = regionCluster.Select(x => x.Polygon).ToList();
            List<GraphNode> polygonNodes = new List<GraphNode>();
            GraphNode startNode = cluster[0].Nodes.Where(x => !x.Polygons.All(p => cluster.Contains(p))).First();
            polygonNodes.Add(startNode);

            GraphNode prevNode = startNode;
            GraphNode currentNode = startNode.ConnectedNodes.Where(x => !x.Polygons.All(p => cluster.Contains(p) && x.Polygons.Any(p => cluster.Contains(p)))).First();
            int counter = 0;
            while(currentNode != startNode && counter < 10010)
            {
                if (counter == 10000) throw new System.Exception("OVERLFOW");
                polygonNodes.Add(currentNode);

                GraphNode nextNode = NextRightmostNodeInCluster(prevNode, currentNode, cluster);
                prevNode = currentNode;
                currentNode = nextNode;

                counter++;
            }

            // Mesh
            GameObject polygon = MeshGenerator.GeneratePolygon(polygonNodes, null);

            // Collider
            polygon.AddComponent<MeshCollider>();
        }
    }

    private GraphNode NextRightmostNodeInCluster(GraphNode from, GraphNode to, List<GraphPolygon> cluster)
    {
        float smallestAngle = float.MaxValue;
        GraphNode toNode = null;
        foreach (GraphNode connectedNode in to.ConnectedNodes.Where(x => !x.Polygons.All(p => cluster.Contains(p)) && x.Polygons.Any(p => cluster.Contains(p))))
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
        return toNode;
    }
}
