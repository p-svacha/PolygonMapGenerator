using Boo.Lang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEditor.PackageManager;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using System.Threading;

using static GeometryFunctions;

public class PolygonMapGenerator : MonoBehaviour
{
    public MapGenerationState GenerationState;

    public int Seed;
    public Map Map;
    public Material SatelliteLandMaterial;
    public Material PoliticalLandMaterial;
    public Material PoliticalWaterMaterial;
    public Material SatelliteWaterMaterial;

    public List<GraphNode> CornerNodes = new List<GraphNode>();
    public List<GraphNode> EdgeNodes = new List<GraphNode>(); // Edge nodes (are on the graph edge)
    public List<GraphNode> Nodes = new List<GraphNode>(); // ALL nodes (including edgeNodes)
    public List<GraphConnection> InGraphConnections = new List<GraphConnection>(); // All ingraph connections (WITHOUT edgeConnections)
    public List<GraphConnection> EdgeConnections = new List<GraphConnection>(); // Edge connections (both nodes of the connection are on the graph edge)
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public List<GraphPath> RiverPaths = new List<GraphPath>();

    private List<GraphNode> InvalidNodes = new List<GraphNode>(); // Nodes that have less polygons than borders (and or not edge nodes)

    private List<GraphNode> LastSegmentNodes = new List<GraphNode>(); // Used for knowing which nodes were newly created in the last segment
    private List<GraphConnection> LastSegmenConnections = new List<GraphConnection>();
    private List<GraphPolygon> LastSegmentPolygons = new List<GraphPolygon>(); // Used for knowing which polygons were newly created in the last search period

    public int Width;
    public int Height;

    public const float LINE_DENSITY = 0.12f; // Active lines at the beginning per m^2
    public const float SPLIT_CHANCE = 0.09f; // Chance that a line splits into 2 lines at a vertex
    public const float MAX_TURN_ANGLE = 55; // °
    public const float SNAP_DISTANCE = 0.07f; // Range, where a point snaps to a nearby point
    public const float MIN_SEGMENT_LENGTH = 0.08f;
    public const float MAX_SEGMENT_LENGTH = 0.13f;
    public const float MIN_SPLIT_ANGLE = 55; // °, the minimum angle when a split forms

    public const float MAX_LAND_POLYGON_SIZE = 1.7f; // Land polygons larger than this will be split
    public const float MIN_POLYGON_SIZE = 0.085f; // Polygons smaller than this will get merged with their smallest neighbour

    public int MapSize;

    private Queue<Action> Actions = new Queue<Action>();

    private Thread NameGeneratorThread;

    // Performance analysis
    public DateTime StartCreation;
    public float Time_CreateSegment = 0;
    public float Time_SplitPolygons = 0;
    public float Time_MergePolygons = 0;
    public float Time_CreateWaters = 0;
    
    public float Time_SnapNode = 0;
    public float Time_LineCollision = 0;

    // Generation stats
    int NumMerges = 0;
    int NumSplits = 0;

    // Start is called before the first frame update
    void Start()
    {
        GenerationState = MapGenerationState.Waiting;
        MarkovChainWordGenerator.Init();
        NameGeneratorThread = new Thread(() => MarkovChainWordGenerator.GenerateWords("Province", 4));
        NameGeneratorThread.Start();
    }

    public void GenerateMap(int width, int height)
    {
        StartCreation = DateTime.Now;

        Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        //seed = -663403863;
        UnityEngine.Random.InitState(Seed);

        Width = width;
        Height = height;
        MapSize = Width * Height;

        // Create corner nodes
        GraphNode corner1 = new GraphNode(new Vector2(0, 0), this);
        CornerNodes.Add(corner1);
        EdgeNodes.Add(corner1);
        Nodes.Add(corner1);
        GraphNode corner2 = new GraphNode(new Vector2(Width, 0), this);
        CornerNodes.Add(corner2);
        EdgeNodes.Add(corner2);
        Nodes.Add(corner2);
        GraphNode corner3 = new GraphNode(new Vector2(Width, Height), this);
        CornerNodes.Add(corner3);
        EdgeNodes.Add(corner3);
        Nodes.Add(corner3);
        GraphNode corner4 = new GraphNode(new Vector2(0, Height), this);
        CornerNodes.Add(corner4);
        EdgeNodes.Add(corner4);
        Nodes.Add(corner4);

        // Start with random lines
        int numStartLines = (int)(Width * Height * LINE_DENSITY);
        if (numStartLines == 0) numStartLines = 1;

        Debug.Log("Starting with map creation with " + numStartLines + " random walkers. SEED: " + Seed);

        for (int i = 0; i < numStartLines; i++)
        {
            GraphNode startNode = new GraphNode(RandomPoint(), this);
            Nodes.Add(startNode);
            float startAngle = RandomAngle();

            Actions.Enqueue(() => CreateSegment(startNode, startAngle, changeAngle: true, canSplit: true));
            Actions.Enqueue(() => CreateSegment(startNode, (startAngle + 180) % 360, changeAngle: true, canSplit: true));
        }

        GenerationState = MapGenerationState.CreateInitialGraph;
        Debug.Log(">----------- Creating initial graph...");
    }

    // Update is called once per frame
    void Update()
    {
        if(Map == null) // We're generating the map
        {
            switch(GenerationState)
            {
                case MapGenerationState.CreateInitialGraph:
                    while (Actions.Count > 0) Actions.Dequeue().Invoke();
                    GenerationState = MapGenerationState.FindInitialPolygons;
                    Debug.Log(">----------- Finding initial polygons...");
                    break;

                case MapGenerationState.FindInitialPolygons:
                    FindAllPolygons();
                    SetNeighbours(ignoreErrors: true);
                    GenerationState = MapGenerationState.RemoveInvalidNodes;
                    Debug.Log(">----------- Found " + Polygons.Count + " Polygons. Removing isolated nodes... SEED: " + Seed);
                    break;

                case MapGenerationState.RemoveInvalidNodes:
                    RemoveInvalidNodes();
                    GenerationState = MapGenerationState.SplitBigPolygons;
                    Debug.Log(">----------- Found " + Polygons.Count + " Polygons.  Splitting big polygons...");
                    break;

                case MapGenerationState.SplitBigPolygons:
                    SplitBigPolygons();
                    GenerationState = MapGenerationState.MergeSmallPolygons;
                    Debug.Log(">----------- Performed " + NumSplits + " Splits. Found " + Polygons.Count + " Polygons. Merging small polygons...");
                    break;

                case MapGenerationState.MergeSmallPolygons:
                    MergeSmallPolygons();
                    GenerationState = MapGenerationState.CreateWaters;
                    Debug.Log(">----------- Performed " + NumMerges + " Merges. Found " + Polygons.Count + " Polygons. Creating water...");
                    break;

                case MapGenerationState.CreateWaters:
                    WaterCreator.CreateWaters(this);
                    GenerationState = MapGenerationState.DrawMap;
                    Debug.Log(">----------- Drawing map...");
                    break;

                case MapGenerationState.DrawMap:
                    DrawMap();
                    InitGame();
                    GenerationState = MapGenerationState.GenerationDone;
                    Debug.Log(">----------- Map of size " + MapSize + "u^2 generated with " + Map.Regions.Count + " regions (" + Map.Regions.Where(x => !x.IsWater).Count() + " land, " + Map.Regions.Where(x => x.IsWater).Count() + " water) in " + (DateTime.Now - StartCreation).TotalSeconds + " seconds.");
                    break;

                case MapGenerationState.GenerationAborted:
                    break;
            }
        }  

        /*
        // H - Hide/Unhide borders and borderpoints
        if(Map != null && Input.GetKeyDown(KeyCode.H))
        {
            Map.ToggleHideBorderPoints();
            Map.ToggleHideBorders();
        }

        // R - Remove invalid nodes
        if (Map != null && Input.GetKeyDown(KeyCode.R))
        {
            Map.DestroyAllGameObjects();
            RemoveInvalidNodes();
            DrawMap();
        }
        */

        //WaterCreator.HandleInput(this);
    }

    private void InitGame()
    {
        GameObject gameObject = new GameObject("Game Model");
        GameModel GameModel = gameObject.AddComponent<GameModel>();
        GameModel.Init(Map);
    }


    // ---------------------------------------------- GRAPH CONSTRUCTION -------------------------------------------------


    public void CreateSegment(GraphNode startNode, float angle, bool changeAngle, bool canSplit)
    {
        DateTime segmentStamp = DateTime.Now;
        //string s = startNode.Vertex.ToString() + ": ";

        GraphNode endNode = null;
        bool snappedToNode = false;

        // Find initial endPoint of segment
        float newAngle = angle;
        if(changeAngle)
        {
            float angleChange = RandomAngleChange();
            newAngle = (angle + angleChange) % 360;
        }
        float length = RandomSegmentLength();
        Vector2 endPoint = new Vector2((float)(startNode.Vertex.x + (Math.Sin(ToRad(angle)) * length)), (float)(startNode.Vertex.y + (Math.Cos(ToRad(angle)) * length)));

        // If endPoint is out of or close to bounds, put it on the nearest edge and mark it as edge point
        bool isEdgeNode = false;
        if (endPoint.x < SNAP_DISTANCE)
        {
            endPoint.x = 0;
            isEdgeNode = true;
        }
        if (endPoint.x > Width - SNAP_DISTANCE)
        {
            endPoint.x = Width;
            isEdgeNode = true;
        }
        if (endPoint.y < SNAP_DISTANCE)
        {
            endPoint.y = 0;
            isEdgeNode = true;
        }
        if (endPoint.y > Height - SNAP_DISTANCE)
        {
            endPoint.y = Height;
            isEdgeNode = true;
        }

        float searchRange = MAX_SEGMENT_LENGTH * 5f;

        // Search for nearest node
        DateTime stamp = DateTime.Now;
        GraphNode neareastNode = null;
        float nearestDistance = float.MaxValue;
        foreach (GraphNode n in Nodes.Where(x => Math.Abs(x.Vertex.x - startNode.Vertex.x) < searchRange && Math.Abs(x.Vertex.y - startNode.Vertex.y) < searchRange))
        {
            if (n != startNode && !startNode.ConnectedNodes.Contains(n)) // Can't snap to start point (would be empty) and it's neighbours because those segment already exists
            {
                float distance = Vector2.Distance(endPoint, n.Vertex);
                {
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        neareastNode = n;
                    }
                }
            }
        }
        Time_SnapNode += (DateTime.Now - stamp).Milliseconds;

        // If nearest node is within snap distance, mark it as snap node
        if (nearestDistance < SNAP_DISTANCE)
        {
            snappedToNode = true;
            endNode = neareastNode;
            endPoint = endNode.Vertex;
            //Debug.Log(s + "Proximity snap to " + neareastNode.Vertex.ToString());
        }

        // Check for line collisions
        stamp = DateTime.Now;
        bool collisionFound = false;
        GraphNode nearestCollisionNode = null;
        float nearestCollisionNodeDistance = float.MaxValue;
        foreach (GraphNode n in Nodes.Where(x => x != startNode && Math.Abs(x.Vertex.x - startNode.Vertex.x) < searchRange && Math.Abs(x.Vertex.y - startNode.Vertex.y) < searchRange))
        {
            float distance = Vector2.Distance(endPoint, n.Vertex);
            if(distance < 2*MAX_SEGMENT_LENGTH) // Only check close points for collision
            {
                foreach(GraphNode connectedNode in n.ConnectedNodes.Where(x => x != startNode))
                {
                    if (DoLineSegmentsIntersect(startNode.Vertex, endPoint, n.Vertex, connectedNode.Vertex)) // Collision found
                    {
                        collisionFound = true;

                        // If both collision segment endpoints are the startNode or connected to it, we just stop
                        if ((n == startNode || startNode.ConnectedNodes.Contains(n)) && (connectedNode == startNode || startNode.ConnectedNodes.Contains(connectedNode))) return;

                        // Get distance to both endpoints of the segment it collided to (if they are not startnode or connected to startNode)
                        if (n != startNode && !startNode.ConnectedNodes.Contains(n))
                        {
                            float distance1 = Vector2.Distance(startNode.Vertex, n.Vertex);
                            if (distance1 < nearestCollisionNodeDistance)
                            {
                                nearestCollisionNodeDistance = distance1;
                                nearestCollisionNode = n;
                            }
                        }

                        if (connectedNode != startNode && !startNode.ConnectedNodes.Contains(connectedNode))
                        {
                            float distance2 = Vector2.Distance(startNode.Vertex, connectedNode.Vertex);
                            if (distance2 < nearestCollisionNodeDistance)
                            {
                                nearestCollisionNodeDistance = distance2;
                                nearestCollisionNode = connectedNode;
                            }
                        }
                    }
                }
            }
        }
        Time_LineCollision += (DateTime.Now - stamp).Milliseconds;

        // If there was a collision, snap collision segment endpoint that is closest to the start point
        if(collisionFound)
        {
            snappedToNode = true;
            endNode = nearestCollisionNode;
            endPoint = nearestCollisionNode.Vertex;
            //Debug.Log(s + "Collision snap to " + neareastNode.Vertex.ToString());
        }

        // Create new node if no snap
        if(endNode == null)
        {
            endNode = new GraphNode(endPoint, this);
            LastSegmentNodes.Add(endNode);
            Nodes.Add(endNode);
            if (isEdgeNode) EdgeNodes.Add(endNode);
        }

        // Create new connections to end node
        GraphConnection newConnection = AddConnection(startNode, endNode, isEdgeConnection: false);
        LastSegmenConnections.Add(newConnection);

        // Keep going if it was a new node and not on an edge, also with a chance of a split
        if(!snappedToNode && !isEdgeNode)
        {
            Actions.Enqueue(() => CreateSegment(endNode, newAngle, changeAngle: true, canSplit: canSplit));

            if(canSplit && UnityEngine.Random.Range(0f, 1f) < SPLIT_CHANCE)
            {
                //Debug.Log("Split at " + startNode.Vertex.ToString());
                List<int> nodeAngles = startNode.GetNodeAngles();
                float splitAngle = RandomValidAngle(nodeAngles);
                Actions.Enqueue(() => CreateSegment(startNode, splitAngle, changeAngle: false, canSplit: canSplit));
            }
        }

        Time_CreateSegment += (DateTime.Now - segmentStamp).Milliseconds;
    }

    private GraphConnection AddConnection(GraphNode start, GraphNode end, bool isEdgeConnection)
    {
        if (start.ConnectedNodes.Contains(end) && end.ConnectedNodes.Contains(start)) return null;

        GraphConnection connection = new GraphConnection(start, end);
        start.Connections.Add(connection);
        end.Connections.Add(connection);
        start.ConnectedNodes.Add(end);
        end.ConnectedNodes.Add(start);
        
        if (isEdgeConnection) EdgeConnections.Add(connection);
        else InGraphConnections.Add(connection);

        return connection;
    }

    private void RemoveConnection(GraphConnection c, bool isEdgeConnection)
    {
        c.StartNode.ConnectedNodes.Remove(c.EndNode);
        c.EndNode.ConnectedNodes.Remove(c.StartNode);
        c.StartNode.Connections.Remove(c);
        c.EndNode.Connections.Remove(c);

        if (isEdgeConnection) EdgeConnections.Remove(c);
        else InGraphConnections.Remove(c);
    }

    private void RemovePolygon(GraphPolygon p)
    {
        foreach (GraphNode n in p.Nodes) if (n.Polygons.Contains(p)) n.Polygons.Remove(p);
        foreach (GraphConnection c in p.Connections) if (c.Polygons.Contains(p)) c.Polygons.Remove(p);
        foreach (GraphPolygon np in p.Neighbours) if (np.Neighbours.Contains(p)) np.Neighbours.Remove(p);
        Polygons.Remove(p);
    }


    // ---------------------------------------------- MAP INITIALIZATION -------------------------------------------------

    private void SetNeighbours(bool ignoreErrors = false)
    {
        if (!ignoreErrors)
        {
            // Set neighbours of connections and polygons
            foreach (GraphConnection c in InGraphConnections)
                if (c.Polygons.Count != 2) Debug.Log("Ingraph connection " + c.ToString() + " does not have 2 polygons as expected! It has " + c.Polygons.Count);
            foreach (GraphConnection c in EdgeConnections)
                if (c.Polygons.Count != 1) Debug.Log("Edge connection " + c.ToString() + " does not have 1 polygon as expected! It has " + c.Polygons.Count);
        }

        foreach (GraphConnection c in InGraphConnections) c.SetNeighbours();
        foreach (GraphConnection c in EdgeConnections) c.SetNeighbours();
        foreach (GraphPolygon p in Polygons) p.SetNeighbours();
    }

    private void MergeSmallPolygons()
    {
        GraphPolygon smallest = Polygons.OrderBy(x => x.Area).First();
        while (Polygons.OrderBy(x => x.Area).First().Area < MIN_POLYGON_SIZE)
        {
            GraphPolygon smallestNeighbour = smallest.Neighbours.OrderBy(x => x.Area).First();
            MergePolygons(smallest, smallestNeighbour);
            smallest = Polygons.OrderBy(x => x.Area).First();
        }
    }

    private void SplitBigPolygons()
    {               
        GraphPolygon largest = Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First();
        while (largest.Area > MAX_LAND_POLYGON_SIZE)
        {
            SplitPolygon(largest);
            largest = Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First();
        }
    }

    public void DrawMap()
    {
        // Reset map
        if(Map != null)
            Map.DestroyAllGameObjects();

        Map = new Map(this);

        
        // Add border points
        foreach (GraphNode n in Nodes)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            BorderPoint borderPoint = sphere.AddComponent<BorderPoint>();
            n.BorderPoint = borderPoint;
            Map.BorderPoints.Add(borderPoint);
        }

        // Add inmap borders (no edge borders)
        foreach (GraphConnection c in InGraphConnections)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Border border = cube.AddComponent<Border>();
            c.Border = border;
            Map.Borders.Add(border);
        }
        // Add edge borders
        foreach (GraphConnection c in EdgeConnections)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Border border = cube.AddComponent<Border>();
            c.Border = border;
            Map.EdgeBorders.Add(border);
        }

        // Add regions
        foreach (GraphPolygon p in Polygons)
        {
            // Mesh
            GameObject polygon = MeshGenerator.GeneratePolygon(p.Nodes.Select(x => x.Vertex).ToList(), this);

            // Collider
            polygon.AddComponent<MeshCollider>();

            // Region
            Region region = polygon.AddComponent<Region>();
            p.Region = region;
            Map.Regions.Add(region);
        }

        foreach (GraphNode n in Nodes) n.BorderPoint.Init(n.Vertex);
        foreach (GraphConnection c in InGraphConnections) c.Border.Init(c.StartNode.BorderPoint, c.EndNode.BorderPoint, c.Polygons.Select(x => x.Region).ToList());
        foreach (GraphConnection c in EdgeConnections) c.Border.Init(c.StartNode.BorderPoint, c.EndNode.BorderPoint, c.Polygons.Select(x => x.Region).ToList());
        foreach (GraphPolygon p in Polygons) p.Region.Init(p, PoliticalWaterMaterial, SatelliteWaterMaterial, SatelliteLandMaterial, PoliticalLandMaterial);

        Map.ToggleHideBorderPoints();
        Map.ToggleHideBorders();

        TextureGenerator.GenerateSatelliteTexture(this);

        Map.InitializeMap(RiverPaths);
    }

    /// <summary>
    /// Removes all edge connections and polygons and makes new ones
    /// </summary>
    private void ReconnectEdgeNodes()
    {
        // Remove edge connections
        List<GraphConnection> edgeCopy = new List<GraphConnection>();
        edgeCopy.AddRange(EdgeConnections);
        foreach (GraphConnection con in edgeCopy) RemoveConnection(con, isEdgeConnection: true);

        // Remove edge polygons
        List<GraphPolygon> edgePolygons = Polygons.Where(x => x.IsEdgePolygon).ToList();
        foreach (GraphPolygon p in edgePolygons) RemovePolygon(p);

        GraphNode startNode = EdgeNodes[0];
        GraphNode currentNode = EdgeNodes[0];

        // Bottom edge
        float curX = 0;
        while (curX != Width)
        {
            GraphNode nextNode = EdgeNodes.Where(x => x.Vertex.y == 0 && x.Vertex.x > curX).OrderBy(x => x.Vertex.x).First();
            AddConnection(currentNode, nextNode, isEdgeConnection: true);
            currentNode = nextNode;
            curX = nextNode.Vertex.x;
        }

        // Right edge
        float curY = 0;
        while (curY != Height)
        {
            GraphNode nextNode = EdgeNodes.Where(x => x.Vertex.x == Width && x.Vertex.y > curY).OrderBy(x => x.Vertex.y).First();
            AddConnection(currentNode, nextNode, isEdgeConnection: true);
            currentNode = nextNode;
            curY = nextNode.Vertex.y;
        }

        // Top edge
        curX = Width;
        while (curX != 0)
        {
            GraphNode nextNode = EdgeNodes.Where(x => x.Vertex.y == Height && x.Vertex.x < curX).OrderByDescending(x => x.Vertex.x).First();
            AddConnection(currentNode, nextNode, isEdgeConnection: true);
            currentNode = nextNode;
            curX = nextNode.Vertex.x;
        }

        // Left edge
        curY = Height;
        while (curY != 0)
        {
            GraphNode nextNode = EdgeNodes.Where(x => x.Vertex.x == 0 && x.Vertex.y < curY).OrderByDescending(x => x.Vertex.y).First();
            AddConnection(currentNode, nextNode, isEdgeConnection: true);
            currentNode = nextNode;
            curY = nextNode.Vertex.y;
        }

    }

    private void FindAllPolygons()
    {
        ReconnectEdgeNodes();
        
        // Check for isolated nodes
        foreach (GraphNode n in Nodes)
        {
            n.VisitedNeighbours.Clear();

            if(n.ConnectedNodes.Count < 2)
            {
                Debug.Log("Vertex at " + n.Vertex.x + "/" + n.Vertex.y + " only has " + n.ConnectedNodes.Count + " neighbours.");
            }
        }

        // Find polygons in graph
        foreach (GraphNode n in Nodes.Where(x => x.Connections.Count > 2))
        {
            FindPolygonsFromNode(n, isEdgeNode: false, ignoreVisitedNodes: false, removePolygons: false);
        }
    }

    private void FindPolygonsFromNode(GraphNode n, bool isEdgeNode, bool ignoreVisitedNodes, bool removePolygons)
    {
        if (isEdgeNode)
        {
            ReconnectEdgeNodes();
            foreach (GraphNode edgeNode in EdgeNodes) FindPolygonsFromNode(edgeNode, false, ignoreVisitedNodes, removePolygons: false);
        }

        if (removePolygons)
        {
            // Remove polygons of node
            List<GraphPolygon> nodePolygons = new List<GraphPolygon>();
            nodePolygons.AddRange(n.Polygons);
            foreach (GraphPolygon p in nodePolygons) RemovePolygon(p);
        }

        foreach (GraphNode connectedNode in n.ConnectedNodes)
        {
            List<GraphNode> polygon = new List<GraphNode>() { n, connectedNode };
            List<GraphConnection> connections = new List<GraphConnection>() { n.GetConnectionTo(connectedNode) };
            FindPolygon(polygon, connections, ignoreVisitedNodes: ignoreVisitedNodes);
            AddPolygon(polygon, connections);
        }
    }

    public void AddPolygon(List<GraphNode> nodes, List<GraphConnection> connections, bool outerPolygon = false)
    {
        // Check if polygon is empty
        if (nodes.Count == 0) return;

        // Check if only consisting of outside nodes
        if (nodes.All(x => EdgeNodes.Contains(x))) return;

        
        int[] edges = nodes.OrderBy(x => x.Id).Select(x => x.Id).ToArray();
        foreach (GraphPolygon p in Polygons)
        {
            // Check if already exists
            int[] pEdges = p.Nodes.OrderBy(x => x.Id).Select(x => x.Id).ToArray();
            if (Enumerable.SequenceEqual(edges, pEdges)) return; // Already exists
        }

        GraphPolygon newPolygon = new GraphPolygon(nodes, connections);
        if (outerPolygon) newPolygon.IsOuterPolygon = true;

        //Debug.Log("Adding new polygon with area " + newPolygon.Area);

        // Add polygon to nodes
        foreach (GraphNode n in nodes)
        {
            n.Polygons.Add(newPolygon);
        }
        // Add polygon to connections
        foreach(GraphConnection c in connections)
        {
            c.Polygons.Add(newPolygon);
        }

        Polygons.Add(newPolygon);
        LastSegmentPolygons.Add(newPolygon);
    }

    private void FindPolygon(List<GraphNode> nodes, List<GraphConnection> connections, bool ignoreVisitedNodes)
    {
        GraphNode sourceNode = nodes[0];
        GraphNode fromNode = nodes[nodes.Count - 2];
        GraphNode angleNode = nodes[nodes.Count - 1];


        if (!ignoreVisitedNodes)
        {
            if (fromNode.VisitedNeighbours.Contains(angleNode)) // We have already done this
            {
                nodes.Clear();
                return;
            }

            fromNode.VisitedNeighbours.Add(angleNode);
        }
        

        GraphNode toNode = NextRightmostNode(fromNode, angleNode);
        if (toNode == null) Debug.Log("FAILED TO FIND RIGHTMOST NODE");

        
        if (toNode == sourceNode) // We are at our starting node, polygon complete, still check if the next node would be second node to see if it's a valid polygon
        {
            if (NextRightmostNode(angleNode, toNode) != nodes[1])
            {
                //Debug.Log("Invalid balloon polygon at " + sourceNode.Vertex.ToString());
                nodes.Clear();
            }
            else connections.Add(angleNode.GetConnectionTo(toNode));
        }
        else if(!nodes.Contains(toNode)) // We are still finding the polygon
        {
            nodes.Add(toNode);
            connections.Add(angleNode.GetConnectionTo(toNode));
            FindPolygon(nodes, connections, ignoreVisitedNodes);
        }
        else // We are at a already visited node that is not the starting node, ABORT
        {
            nodes.Clear();
        }
    }

    private GraphNode NextRightmostNode(GraphNode from, GraphNode to)
    {
        float smallestAngle = float.MaxValue;
        GraphNode toNode = null;
        foreach (GraphNode connectedNode in to.ConnectedNodes)
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

    private void RemoveInvalidNodes()
    {
        // Look for isolated nodes (nodes that have less connections that polygons)
        InvalidNodes = Nodes.Where(x => x.Connections.Count > x.Polygons.Count && !EdgeNodes.Contains(x)).ToList();

        if (InvalidNodes.Count == 0) return; // Nothing to do

        // 1. Identify balloon node polygons (nodes that have a polygon and more connections than polygons)
        List<GraphNode> balloonNodes = Nodes.Where(x => x.Type != BorderPointType.Edge && x.Connections.Count - x.Polygons.Count >= 2 && x.Polygons.Count > 0).ToList();
        List<GraphPolygon> balloonNodePolygons = new List<GraphPolygon>();
        foreach(GraphNode b in balloonNodes)
        {
            foreach (GraphPolygon p in b.Polygons) if (!balloonNodePolygons.Contains(p)) balloonNodePolygons.Add(p);
        }

        // 2. Remove all nodes that have no polygon, + their connections
        List<GraphNode> isolatedNodes = Nodes.Where(x => x.Type != BorderPointType.Edge && x.Polygons.Count == 0).ToList();
        foreach(GraphNode n in isolatedNodes)
        {
            List<GraphConnection> connectionsToRemove = new List<GraphConnection>();
            connectionsToRemove.AddRange(n.Connections);
            foreach (GraphConnection c in connectionsToRemove) RemoveConnection(c, isEdgeConnection: false);
            Nodes.Remove(n);
        }

        // 3. Remove balloon node clusters that are not connected to any edge
        foreach(GraphPolygon p in balloonNodePolygons)
        {
            List<GraphPolygon> cluster = new List<GraphPolygon>();
            if(!isPolygonConnectedToEdge(p, cluster))
            {
                // Remove all connections in cluster
                foreach(GraphPolygon clusterPoly in cluster)
                {
                    foreach (GraphConnection c in clusterPoly.Connections) RemoveConnection(c, isEdgeConnection: false);
                    Polygons.Remove(clusterPoly);
                }
                // Remove all nodes in cluster
                foreach (GraphPolygon clusterPoly in cluster)
                {
                    foreach (GraphNode n in clusterPoly.Nodes)
                    {
                        if (n.Connections.Count == 0) Nodes.Remove(n);
                        else if (n.Connections.Count == 1)
                        {
                            RemoveConnection(n.Connections[0], isEdgeConnection: false);
                            Nodes.Remove(n);
                        }
                    }
                        
                }
            }
        }

        FindAllPolygons();
        SetNeighbours(ignoreErrors: true);
    }

    private bool isPolygonConnectedToEdge(GraphPolygon p, List<GraphPolygon> visitedPolygons)
    {
        Queue<GraphPolygon> polygonsToCheck = new Queue<GraphPolygon>();
        polygonsToCheck.Enqueue(p);

        while(polygonsToCheck.Count > 0)
        {
            GraphPolygon poly = polygonsToCheck.Dequeue();
            visitedPolygons.Add(poly);
            if (poly.IsEdgePolygon) return true;
            foreach (GraphPolygon pn in poly.Neighbours) if (!visitedPolygons.Contains(pn)) polygonsToCheck.Enqueue(pn);
        }
        return false;
    }

    // ---------------------------------------------- MAP MODIFICATION -------------------------------------------------

    private void MergePolygons(GraphPolygon p1, GraphPolygon p2)
    {
        NumMerges++;

        // Create relevant lists
        List<GraphConnection> consBetweenPolygons = p1.Connections.Where(x => x.Polygons.Contains(p2)).ToList();
        List<GraphNode> nodesBetweenPolygons = p1.Nodes.Where(x => x.Polygons.Count == 2 && x.Polygons.Contains(p2) && x.Type != BorderPointType.Edge).ToList();

        //Debug.Log("Merging polygon with area " + p1.Area + " with polygon with area " + p2.Area + ". Removing " + nodesBetweenPolygons.Count + " nodes and " + consBetweenPolygons.Count + " connections.");
        //if (p1.IsEdgePolygon) Debug.Log("######################################################################################################### P1 is edge");
        //if (p2.IsEdgePolygon) Debug.Log("######################################################################################################### P2 is edge");

        // Remove polygons from everywhere
        RemovePolygon(p1);
        RemovePolygon(p2);

        // Remove the nodes between the polygons from everywhere
        foreach (GraphNode n in nodesBetweenPolygons)
        {
            foreach (GraphNode neighbourNode in n.ConnectedNodes) neighbourNode.ConnectedNodes.Remove(n);
            Nodes.Remove(n);
        }

        // Remove the connections between the nodes from everywhere
        foreach(GraphConnection con in consBetweenPolygons)
        {
            RemoveConnection(con, isEdgeConnection: false);
        }

        FindPolygonsFromNode(p1.Nodes.Where(x => !nodesBetweenPolygons.Contains(x)).ToList()[0], isEdgeNode: false, ignoreVisitedNodes: true, removePolygons: true);
        SetNeighbours();
    }

    private void SplitPolygon(GraphPolygon p)
    {
        NumSplits++;

        if (p.IsEdgePolygon) throw new Exception("ERROR: SPLITTING EDGE POLYGONS NOT YET IMPLEMENTED");

        // Select random node where the split start
        int splitNodeId = UnityEngine.Random.Range(0, p.Nodes.Count);

        GraphNode splitNode = p.Nodes[splitNodeId];
        GraphNode beforeNode = p.Nodes[splitNodeId == 0 ? p.Nodes.Count - 1 : splitNodeId - 1];
        GraphNode afterNode = p.Nodes[splitNodeId == p.Nodes.Count - 1 ? 0 : splitNodeId + 1];

        int beforeConnectionAngle = (int)(Vector2.SignedAngle(new Vector2(beforeNode.Vertex.x - splitNode.Vertex.x, beforeNode.Vertex.y - splitNode.Vertex.y), new Vector2(0, 1)));
        int afterConnectionAngle = (int)(Vector2.SignedAngle(new Vector2(afterNode.Vertex.x - splitNode.Vertex.x, afterNode.Vertex.y - splitNode.Vertex.y), new Vector2(0, 1)));

        beforeConnectionAngle = mod(beforeConnectionAngle, 360);
        afterConnectionAngle = mod(afterConnectionAngle, 360);

        // Get list of valid angles that are inside the polygon
        List<int> validAngles = new List<int>();
        if (beforeConnectionAngle < afterConnectionAngle)
        {
            for (int i = 0; i < 360; i++)
            {
                int beforeDistance = DegreeDistance(i, beforeConnectionAngle);
                int afterDistance = DegreeDistance(i, afterConnectionAngle);
                if ((i < beforeConnectionAngle || i > afterConnectionAngle) && beforeDistance > MIN_SPLIT_ANGLE && afterDistance > MIN_SPLIT_ANGLE) validAngles.Add(i);
            }
        }
        else
        {
            for (int i = 0; i < 360; i++)
            {
                int beforeDistance = DegreeDistance(i, beforeConnectionAngle);
                int afterDistance = DegreeDistance(i, afterConnectionAngle);
                if ((i < beforeConnectionAngle && i > afterConnectionAngle) && beforeDistance > MIN_SPLIT_ANGLE && afterDistance > MIN_SPLIT_ANGLE) validAngles.Add(i);
            }
        }

        // If there are no good angles, try again from another random node
        if (validAngles.Count == 0)
        {
            SplitPolygon(p);
            return;
        }
        int chosenAngle = validAngles[UnityEngine.Random.Range(0, validAngles.Count)];

        // Create new segment that will split the polygon
        LastSegmentNodes.Clear();
        LastSegmenConnections.Clear();
        Actions.Enqueue(() => CreateSegment(splitNode, chosenAngle, changeAngle: false, canSplit: false));
        while (Actions.Count > 0) Actions.Dequeue().Invoke();

        // Find new polygons that were created in the split
        LastSegmentPolygons.Clear();
        FindPolygonsFromNode(splitNode, isEdgeNode: false, ignoreVisitedNodes: true, removePolygons: true);

        // We check if the split has generated invalid nodes. If it has, remove the last split by removing all connections from added nodes
        InvalidNodes = Nodes.Where(x => x.Connections.Count > x.Polygons.Count && !EdgeNodes.Contains(x)).ToList();
        if (InvalidNodes.Count > 0)
        {
            foreach(GraphConnection c in LastSegmenConnections)
                RemoveConnection(c, isEdgeConnection: false);
            foreach (GraphNode n in LastSegmentNodes) 
                Nodes.Remove(n);

            FindPolygonsFromNode(splitNode, isEdgeNode: false, ignoreVisitedNodes: true, removePolygons: true);
        }

        SetNeighbours();
    }

    // ---------------------------------------------- RADNOM VALUE FUNCTIONS -------------------------------------------------
    public float RandomSegmentLength()
    {
        return UnityEngine.Random.Range(MIN_SEGMENT_LENGTH, MAX_SEGMENT_LENGTH);
    }
    public float RandomAngle()
    {
        return UnityEngine.Random.Range(0f, 360f);
    }
    public float RandomAngleChange()
    {
        return UnityEngine.Random.Range(-MAX_TURN_ANGLE, MAX_TURN_ANGLE);
    }
    public Vector2 RandomPoint()
    {
        return new Vector2(UnityEngine.Random.Range(MAX_SEGMENT_LENGTH, Width - MAX_SEGMENT_LENGTH), UnityEngine.Random.Range(MAX_SEGMENT_LENGTH, Height - MAX_SEGMENT_LENGTH));
    }
    public int RandomValidAngle(List<int> angles) // Returns an angle that is at least MIN_SPLIT_ANGLE away from an angle in the list
    {
        List<int> otherAngles = new List<int>();
        foreach(int angle in angles)
        {
            int modAngle = mod(angle, 360);
            otherAngles.Add(modAngle);
            otherAngles.Add(modAngle - 360);
            otherAngles.Add(modAngle + 360);
        }

        List<int> validAngles = new List<int>();
        int bestAngle = 0;
        int bestAngleDistance = 0;
        for (int i = 0; i < 360; i++)
        {
            int minDistance = int.MaxValue;
            foreach (int a in otherAngles)
            {
                int distance = Math.Abs(i - a);
                if (distance < minDistance) minDistance = distance;
            }
            if (minDistance > MIN_SPLIT_ANGLE)
            {
                validAngles.Add(i);
            }
            if(minDistance > bestAngleDistance)
            {
                bestAngleDistance = minDistance;
                bestAngle = i;
            }
        }

        if (validAngles.Count == 0) return bestAngle;

        int randomValidAngle = validAngles[UnityEngine.Random.Range(0, validAngles.Count)];
        return randomValidAngle;

    }

}
