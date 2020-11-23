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

    public List<GraphNode> CornerNodes = new List<GraphNode>();
    public List<GraphNode> EdgeNodes = new List<GraphNode>(); // Edge nodes (are on the graph edge)
    public List<GraphNode> Nodes = new List<GraphNode>(); // ALL nodes (including edgeNodes)
    public List<GraphConnection> InGraphConnections = new List<GraphConnection>(); // All ingraph connections (WITHOUT edgeConnections)
    public List<GraphConnection> EdgeConnections = new List<GraphConnection>(); // Edge connections (both nodes of the connection are on the graph edge)
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public List<GraphPath> RiverPaths = new List<GraphPath>();

    private List<GraphNode> InvalidNodes = new List<GraphNode>(); // Nodes that have less polygons than borders (and or not edge nodes)

    private List<GraphNode> LastSegmentNodes = new List<GraphNode>(); // Used for knowing which nodes were newly created in the last segment
    private List<GraphConnection> LastSegmentConnections = new List<GraphConnection>();
    private List<GraphPolygon> LastSegmentPolygons = new List<GraphPolygon>(); // Used for knowing which polygons were newly created in the last search period

    private List<GraphPolygon> LastRemovedPolygons = new List<GraphPolygon>(); // Used when splitting or merging polygons to find polygons that remained unchanged so the attributes can be transferred

    public Action Callback; // Gets called when map generation is done
    public int Width;
    public int Height;
    public float MinPolygonArea;
    public float MaxPolygonArea;
    public bool Island;
    public bool ShowRegionBorders;

    public const float START_LINES_PER_KM = 0.2f; // Active lines per km map side length
    public const bool RANDOM_START_LINE_POSITIONS = false; // If true, start lines start at completely random positions. If false, they start on a grid

    public const float SPLIT_CHANCE = 0.09f; // Chance that a line splits into 2 lines at a vertex
    public const float MAX_TURN_ANGLE = 45; // °
    public const float SNAP_DISTANCE = 0.07f; // Range, where a point snaps to a nearby point
    public const float MIN_SEGMENT_LENGTH = 0.08f;
    public const float MAX_SEGMENT_LENGTH = 0.13f;
    public const float MIN_SPLIT_ANGLE = 55; // °, the minimum angle when a split forms

    //public const float MAX_LAND_POLYGON_SIZE = 1.5f; // Land polygons larger than this will be split
    //public const float MIN_POLYGON_SIZE = 0.08f; // Polygons smaller than this will get merged with their smallest neighbour

    public int MapSize;

    private Queue<Action> Actions = new Queue<Action>();

    private Thread NameGeneratorThread;

    // Performance analysis
    public DateTime StartCreation;
    public DateTime StateTimeStamp;

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

    public void GenerateMap(int width, int height, float minPolygonArea, float maxPolygonArea, bool island, bool drawRegionBorders = false, Action callback = null)
    {
        Callback = callback;

        StartCreation = DateTime.Now;
        StateTimeStamp = DateTime.Now;

        Reset();

        Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        //seed = -663403863;
        Debug.Log("Generating " + width + "/" + height + (island ? " island" : " non-island") + " map with region sizes between " + minPolygonArea + " and " + maxPolygonArea + ".");
        Debug.Log("SEED: " + Seed);
        UnityEngine.Random.InitState(Seed);

        Width = width;
        Height = height;
        MinPolygonArea = minPolygonArea;
        MaxPolygonArea = maxPolygonArea;
        Island = island;
        ShowRegionBorders = drawRegionBorders;
        MapSize = Width * Height;

        SwitchState(MapGenerationState.CreateMapBounds);
    }

    #region Generation State Flow

    void Update()
    {
        switch(GenerationState)
        {
            case MapGenerationState.CreateMapBounds:
                if (Island) CreateIslandMapBounds();
                else CreateNonIslandMapBounds();
                SwitchState(MapGenerationState.CreateInitialGraph);
                break;

            case MapGenerationState.CreateInitialGraph:
                CreateInitialGraph();
                SwitchState(MapGenerationState.FindInitialPolygons);
                break;

            case MapGenerationState.FindInitialPolygons:
                FindAllPolygons();
                SwitchState(MapGenerationState.RemoveInvalidNodes);
                break;

            case MapGenerationState.RemoveInvalidNodes:
                RemoveInvalidNodes();
                SwitchState(MapGenerationState.SplitBigPolygons);
                break;

            case MapGenerationState.SplitBigPolygons:
                SplitBigPolygons();
                SwitchState(MapGenerationState.MergeSmallPolygons);
                break;

            case MapGenerationState.MergeSmallPolygons:
                MergeSmallPolygons();
                SwitchState(MapGenerationState.CreateWaters);
                break;

            case MapGenerationState.CreateWaters:
                WaterCreator.CreateWaters(this);
                SwitchState(MapGenerationState.FindWaterNeighbours);
                break;

            case MapGenerationState.FindWaterNeighbours:
                FindWaterNeighbours();
                SwitchState(MapGenerationState.DrawMap);
                break;

                /*
            case MapGenerationState.CreateTopology:
                TopologyCreator.CreateTopology(this);
                SwitchState(MapGenerationState.CreateRivers);
                break;

            case MapGenerationState.CreateRivers:
                RiverCreator.CreateRivers(this);
                SwitchState(MapGenerationState.DrawMap);
                break;
                */

            case MapGenerationState.DrawMap:
                DrawMap(ShowRegionBorders);
                SwitchState(MapGenerationState.GenerationDone);
                Debug.Log(">----------- Map of size " + MapSize + "u^2 generated with " + Map.Regions.Count + " regions (" + Map.Regions.Where(x => !x.IsWater).Count() + " land, " + Map.Regions.Where(x => x.IsWater).Count() + " water) in " + (DateTime.Now - StartCreation).TotalSeconds + " seconds.");
                break;

            case MapGenerationState.GenerationAborted:
                break;
        }
    }

    private void SwitchState(MapGenerationState state)
    {
        MapGenerationState currentState = GenerationState;
        MapGenerationState nextState = state;

        float stateTime = (DateTime.Now - StateTimeStamp).Milliseconds;
        Debug.Log("State " + currentState.ToString() + " finished after " + stateTime + " ms.");
        StateTimeStamp = DateTime.Now;

        Debug.Log("Starting state " + nextState.ToString());

        GenerationState = nextState;
    }

    private void Reset()
    {
        if (Map != null) Map.DestroyAllGameObjects();
        Map = null;

        CornerNodes.Clear();
        EdgeNodes.Clear();
        Nodes.Clear();
        InGraphConnections.Clear();
        EdgeConnections.Clear();
        Polygons.Clear();

        RiverPaths.Clear();

        InvalidNodes.Clear();
        LastSegmentNodes.Clear();
        LastSegmentConnections.Clear();
        LastSegmentPolygons.Clear();

        Actions.Clear();
    }

    #endregion

    #region Map Bounds

    private void CreateIslandMapBounds()
    {
        CreateCornerNodes();

        // Create Connections between corner nodes
        AddConnection(CornerNode_BotLeft, CornerNode_BotRight, isEdgeConnection: true);
        AddConnection(CornerNode_BotRight, CornerNode_TopRight, isEdgeConnection: true);
        AddConnection(CornerNode_TopRight, CornerNode_TopLeft, isEdgeConnection: true);
        AddConnection(CornerNode_TopLeft, CornerNode_BotLeft, isEdgeConnection: true);

        // Create inner corner nodes
        float borderMargin = 2 * MAX_SEGMENT_LENGTH;
        GraphNode innerCornerBotLeft = new GraphNode(new Vector2(borderMargin, borderMargin), this);
        AddNode(innerCornerBotLeft);
        GraphNode innerCornerBotRight = new GraphNode(new Vector2(Width - borderMargin, borderMargin), this);
        AddNode(innerCornerBotRight);
        GraphNode innerCornerTopRight = new GraphNode(new Vector2(Width - borderMargin, Height - borderMargin), this);
        AddNode(innerCornerTopRight);
        GraphNode innerCornerTopLeft = new GraphNode(new Vector2(borderMargin, Height - borderMargin), this);
        AddNode(innerCornerTopLeft);

        // Create connections between corners and inner corners
        AddConnection(CornerNode_BotLeft, innerCornerBotLeft, isEdgeConnection: false);
        AddConnection(CornerNode_BotRight, innerCornerBotRight, isEdgeConnection: false);
        AddConnection(CornerNode_TopRight, innerCornerTopRight, isEdgeConnection: false);
        AddConnection(CornerNode_TopLeft, innerCornerTopLeft, isEdgeConnection: false);

        // Create bottom map edge between inner corners
        float y = 0f;
        float x = borderMargin;
        GraphNode lastNode = innerCornerBotLeft;
        while(x < Width - 3 * MAX_SEGMENT_LENGTH)
        {
            x += RandomSegmentLength();
            y = MAX_SEGMENT_LENGTH + RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerBotRight, isEdgeConnection: false);

        // Create right map edge between inner corners
        y = borderMargin;
        lastNode = innerCornerBotRight;
        while (y < Height - 3 * MAX_SEGMENT_LENGTH)
        {
            y += RandomSegmentLength();
            x = Width - MAX_SEGMENT_LENGTH - RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerTopRight, isEdgeConnection: false);

        // Create top map edge between inner corners
        x = Width - borderMargin;
        lastNode = innerCornerTopRight;
        while (x > 3 * MAX_SEGMENT_LENGTH)
        {
            x -= RandomSegmentLength();
            y = Height - MAX_SEGMENT_LENGTH - RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerTopLeft, isEdgeConnection: false);

        // Create left map edge between inner corners
        y = Height - borderMargin;
        lastNode = innerCornerTopLeft;
        while (y > 3 * MAX_SEGMENT_LENGTH)
        {
            y -= RandomSegmentLength();
            x = MAX_SEGMENT_LENGTH + RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerBotLeft, isEdgeConnection: false);
    }

    private void CreateNonIslandMapBounds()
    {
        CreateCornerNodes();

        // Create bottom map edge between inner corners
        float y = 0f;
        float x = 0f;
        GraphNode lastNode = CornerNode_BotLeft;
        while (x < Width - 1.5f * MAX_SEGMENT_LENGTH)
        {
            x += RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_BotRight, isEdgeConnection: true);

        // Create right map edge between inner corners
        x = Width;
        y = 0f;
        lastNode = CornerNode_BotRight;
        while (y < Height - 1.5f * MAX_SEGMENT_LENGTH)
        {
            y += RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_TopRight, isEdgeConnection: true);

        // Create top map edge between inner corners
        x = Width;
        y = Height;
        lastNode = CornerNode_TopRight;
        while (x > 1.5 * MAX_SEGMENT_LENGTH)
        {
            x -= RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_TopLeft, isEdgeConnection: true);

        // Create left map edge between inner corners
        x = 0f;
        y = Height;
        lastNode = CornerNode_TopLeft;
        while (y > 1.5 * MAX_SEGMENT_LENGTH)
        {
            y -= RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_BotLeft, isEdgeConnection: true);
    }

    private void CreateCornerNodes()
    {
        GraphNode cornerBotLeft = new GraphNode(new Vector2(0, 0), this);
        AddNode(cornerBotLeft, isEdgeNode: true, isCornerNode: true);

        GraphNode cornerBotRight = new GraphNode(new Vector2(Width, 0), this);
        AddNode(cornerBotRight, isEdgeNode: true, isCornerNode: true);

        GraphNode cornerTopRight = new GraphNode(new Vector2(Width, Height), this);
        AddNode(cornerTopRight, isEdgeNode: true, isCornerNode: true);

        GraphNode cornerTopLeft = new GraphNode(new Vector2(0, Height), this);
        AddNode(cornerTopLeft, isEdgeNode: true, isCornerNode: true);
    }

    #endregion

    #region Graph Construction 

    private void CreateInitialGraph()
    {
        // Init random walkers at certain positions
        int xStartLines = (int)(START_LINES_PER_KM * Width);
        int yStartLines = (int)(START_LINES_PER_KM * Height);
        float xStartLineStep = (float)(Width) / (xStartLines + 1f);
        float yStartLineStep = (float)(Height) / (yStartLines + 1f);
        for (int y = 0; y < yStartLines; y++)
        {
            for (int x = 0; x < xStartLines; x++)
            {
                Vector2 startPosition = RANDOM_START_LINE_POSITIONS ? RandomPoint() :
                    new Vector2((x + 1) * xStartLineStep, (y + 1) * yStartLineStep);
                GraphNode startNode = new GraphNode(startPosition, this);
                AddNode(startNode);
                float startAngle = RandomAngle();

                Actions.Enqueue(() => CreateSegment(startNode, startAngle, changeAngle: true, canSplit: true));
                Actions.Enqueue(() => CreateSegment(startNode, (startAngle + 180) % 360, changeAngle: true, canSplit: true));
            }
        }

        // Let the random walkers walk until all are done
        while (Actions.Count > 0) Actions.Dequeue().Invoke();
    }

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
            AddNode(endNode);
        }

        // Create new connections to end node
        GraphConnection newConnection = AddConnection(startNode, endNode, isEdgeConnection: false);
        LastSegmentConnections.Add(newConnection);

        // Keep going if it was a new node and not on an edge, also with a chance of a split
        if(!snappedToNode)
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
    }

    private void AddNode(GraphNode n, bool isEdgeNode = false, bool isCornerNode = false)
    {
        Nodes.Add(n);
        if (isEdgeNode) EdgeNodes.Add(n);
        if (isCornerNode) CornerNodes.Add(n);
    }

    private void RemoveNode(GraphNode n, bool forceRemove = false)
    {
        if (n.Connections.Count > 0 && !forceRemove) throw new Exception("It is not allowed to remove a node that's still part of a connection.");
        if (n.Polygons.Count > 0 && !forceRemove) throw new Exception("It is not allowed to remove a node that' still part of a polygon.");

        Nodes.Remove(n);
    }

    private GraphConnection AddConnection(GraphNode start, GraphNode end, bool isEdgeConnection)
    {
        if (start.ConnectedNodes.Contains(end) && end.ConnectedNodes.Contains(start)) return null;
        GraphConnection connection = new GraphConnection(start, end);

        // Set neighbour connections
        foreach (GraphConnection neighbour in start.Connections)
        {
            if (!neighbour.Connections.Contains(connection)) neighbour.Connections.Add(connection);
            if (!connection.Connections.Contains(neighbour)) connection.Connections.Add(neighbour);
        }
        foreach (GraphConnection neighbour in end.Connections)
        {
            if (!neighbour.Connections.Contains(connection)) neighbour.Connections.Add(connection);
            if (!connection.Connections.Contains(neighbour)) connection.Connections.Add(neighbour);
        }

        // Add to nodes
        start.Connections.Add(connection);
        end.Connections.Add(connection);
        start.ConnectedNodes.Add(end);
        end.ConnectedNodes.Add(start);
        
        // Add to lists
        if (isEdgeConnection) EdgeConnections.Add(connection);
        else InGraphConnections.Add(connection);

        return connection;
    }

    private void RemoveConnection(GraphConnection c, bool forceRemove = false)
    {
        if (c.Polygons.Count > 0 && !forceRemove) throw new Exception("It is not allowed to remove a connection that's still part of a polygon.");

        // Remove from neighbour connections
        foreach (GraphConnection neighbour in c.StartNode.Connections) if (neighbour.Connections.Contains(c)) neighbour.Connections.Remove(c);
        foreach (GraphConnection neighbour in c.EndNode.Connections) if (neighbour.Connections.Contains(c)) neighbour.Connections.Remove(c);

        // Remove from nodes
        c.StartNode.ConnectedNodes.Remove(c.EndNode);
        c.EndNode.ConnectedNodes.Remove(c.StartNode);
        c.StartNode.Connections.Remove(c);
        c.EndNode.Connections.Remove(c);

        InGraphConnections.Remove(c);
    }

    public void AddPolygon(List<GraphNode> nodes, List<GraphConnection> connections, bool outerPolygon = false, bool isWater = false)
    {
        // Abort if polygon is empty
        if (nodes.Count == 0) return;

        GraphPolygon newPolygon = new GraphPolygon(nodes, connections)
        {
            IsWater = isWater
        };
        AddPolygon(newPolygon, outerPolygon);
    }

    private void AddPolygon(GraphPolygon newPolygon, bool outerPolygon = false)
    {
        // Abort if polygon is empty
        if (newPolygon.Nodes.Count == 0) return;

        // Abort if contains all corner nodes
        if (CornerNodes.All(x => newPolygon.Nodes.Contains(x))) return;

        // Abort if polygon already exists
        if (Polygons.Any(x => x.HasSameNodesAs(newPolygon))) return;

        // If polygon was recently deleted, re-add the deleted one back (can happen while merging or splitting).
        foreach (GraphPolygon removedPoly in LastRemovedPolygons)
        {
            if (removedPoly.HasSameNodesAs(newPolygon))
            {
                LastRemovedPolygons.Clear();
                AddPolygon(removedPoly);
                return;
            }
        }

        if (outerPolygon)
        {
            newPolygon.IsOuterPolygon = true;
            newPolygon.DistanceFromNearestWater = -3;
        }

        //Debug.Log("Adding new polygon with area " + newPolygon.Area);

        // Add polygon to nodes
        foreach (GraphNode n in newPolygon.Nodes)
        {
            n.Polygons.Add(newPolygon);
        }
        // Add polygon to connections and set neighbours
        foreach (GraphConnection c in newPolygon.Connections)
        {
            if (c.Polygons.Count > 1) throw new Exception("It is not allowed to add a polygon to a connection that already has 2 polygons");
            foreach (GraphPolygon neighbour in c.Polygons)
            {
                if (!newPolygon.AdjacentPolygons.Contains(neighbour)) newPolygon.AdjacentPolygons.Add(neighbour);
                if (!neighbour.AdjacentPolygons.Contains(newPolygon)) neighbour.AdjacentPolygons.Add(newPolygon);
            }
            if (!c.Polygons.Contains(newPolygon)) c.Polygons.Add(newPolygon);
        }

        Polygons.Add(newPolygon);
        LastSegmentPolygons.Add(newPolygon);
    }

    private void RemovePolygon(GraphPolygon p)
    {
        foreach (GraphNode n in p.Nodes) if (n.Polygons.Contains(p)) n.Polygons.Remove(p);
        foreach (GraphConnection c in p.Connections) if (c.Polygons.Contains(p)) c.Polygons.Remove(p);
        foreach (GraphPolygon np in p.AdjacentPolygons) if (np != p) np.AdjacentPolygons.Remove(p);
        p.AdjacentPolygons.Clear();
        Polygons.Remove(p);
    }

    #endregion

    #region Map Initialization

    private void MergeSmallPolygons()
    {
        GraphPolygon smallest = Polygons.OrderBy(x => x.Area).First();
        while (Polygons.OrderBy(x => x.Area).First().Area < MinPolygonArea)
        {
            GraphPolygon smallestNeighbour = smallest.AdjacentPolygons.OrderBy(x => x.Area).First();
            MergePolygons(smallest, smallestNeighbour);
            smallest = Polygons.OrderBy(x => x.Area).First();
        }
    }

    private void SplitBigPolygons()
    {               
        GraphPolygon largest = Island ? Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First() : Polygons.OrderByDescending(x => x.Area).First();
        while (largest.Area > MaxPolygonArea)
        {
            SplitPolygon(largest);
            largest = Island ? Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First() : Polygons.OrderByDescending(x => x.Area).First();
        }
    }

    public void DrawMap(bool showRegionBorders)
    {
        if (Map != null) Map.DestroyAllGameObjects();

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

        foreach (GraphNode n in Nodes) n.BorderPoint.Init(n);
        foreach (GraphConnection c in InGraphConnections) c.Border.Init(c.StartNode.BorderPoint, c.EndNode.BorderPoint, c.Polygons.Select(x => x.Region).ToList());
        foreach (GraphConnection c in EdgeConnections) c.Border.Init(c.StartNode.BorderPoint, c.EndNode.BorderPoint, c.Polygons.Select(x => x.Region).ToList());
        foreach (GraphPolygon p in Polygons) p.Region.Init(p);

        Map.ToggleHideBorderPoints();
        Map.ToggleHideBorders();

        Map.InitializeMap(this, showRegionBorders);

        Callback?.Invoke();
    }

    private void FindAllPolygons()
    {
        // Check for isolated nodes
        foreach (GraphNode n in Nodes)
        {
            n.VisitedNeighbours.Clear();

            if(n.ConnectedNodes.Count < 2)
            {
                Debug.Log("Node at " + n.Vertex.x + "/" + n.Vertex.y + " only has " + n.ConnectedNodes.Count + " neighbours.");
            }
        }

        // Find polygons in graph
        foreach (GraphNode n in Nodes.Except(EdgeNodes).Where(x => x.Connections.Count > 2))
        {
            FindPolygonsFromNode(n, ignoreVisitedNodes: false, removePolygons: false);
        }
    }

    private void FindPolygonsFromNode(GraphNode n, bool ignoreVisitedNodes, bool removePolygons, bool markFoundAsWater = false)
    {
        LastRemovedPolygons.Clear();

        if (removePolygons)
        {
            // Remove polygons of node
            List<GraphPolygon> nodePolygons = new List<GraphPolygon>();
            nodePolygons.AddRange(n.Polygons);
            foreach (GraphPolygon p in nodePolygons)
            {
                RemovePolygon(p);
                LastRemovedPolygons.Add(p);
            }
        }

        foreach (GraphNode connectedNode in n.ConnectedNodes)
        {
            List<GraphNode> polygon = new List<GraphNode>() { n, connectedNode };
            List<GraphConnection> connections = new List<GraphConnection>() { n.GetConnectionTo(connectedNode) };
            FindPolygon(polygon, connections, ignoreVisitedNodes: ignoreVisitedNodes);
            AddPolygon(polygon, connections, isWater: markFoundAsWater);
        }
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
        if (toNode == null) throw new Exception("FAILED TO FIND RIGHTMOST NODE");

        
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
        // Look for isolated nodes (nodes that have less connections than polygons)
        InvalidNodes = Nodes.Where(x => x.Connections.Count > x.Polygons.Count && !EdgeNodes.Contains(x)).ToList();

        if (InvalidNodes.Count == 0) return; // Nothing to do

        // 1. Identify balloon node polygons (polygons that hava nodes that have more connections than polygons)
        List<GraphNode> balloonNodes = Nodes.Where(x => x.Type != BorderPointType.Edge && x.Connections.Count - x.Polygons.Count >= 2 && x.Polygons.Count > 0).ToList();
        HashSet<GraphPolygon> balloonNodePolygons = new HashSet<GraphPolygon>();
        foreach(GraphNode b in balloonNodes)
        {
            foreach (GraphPolygon p in b.Polygons) balloonNodePolygons.Add(p);
        }

        // 2. Remove all nodes and their connections that have no polygon
        List<GraphNode> isolatedNodes = Nodes.Where(x => x.Type != BorderPointType.Edge && x.Polygons.Count == 0).ToList();
        foreach(GraphNode n in isolatedNodes)
        {
            List<GraphConnection> connectionsToRemove = new List<GraphConnection>();
            connectionsToRemove.AddRange(n.Connections);
            foreach (GraphConnection c in connectionsToRemove) RemoveConnection(c);
            RemoveNode(n);
        }

        // 3. Remove balloon node clusters that are not connected to any edge
        foreach(GraphPolygon p in balloonNodePolygons)
        {
            List<GraphPolygon> cluster = new List<GraphPolygon>();
            if(!IsPolygonConnectedToEdge(p, cluster))
            {
                // Remove all connections in cluster
                foreach(GraphPolygon clusterPoly in cluster)
                {
                    foreach (GraphConnection c in clusterPoly.Connections) RemoveConnection(c, forceRemove: true);
                    RemovePolygon(clusterPoly);
                }
                // Remove all nodes in cluster
                foreach (GraphPolygon clusterPoly in cluster)
                {
                    foreach (GraphNode n in clusterPoly.Nodes)
                    {
                        if (n.Connections.Count == 0) RemoveNode(n, forceRemove: true);
                        else if (n.Connections.Count == 1)
                        {
                            RemoveConnection(n.Connections[0]);
                            RemoveNode(n, forceRemove: true);
                        }
                    }
                        
                }
            }
        }

        FindAllPolygons();
    }

    public void FindWaterNeighbours()
    {
        foreach (GraphPolygon poly in Polygons) poly.WaterNeighbours.Clear();

        // Direct water neighbours: If two regions are connected to the same water region, but are not adjacent, they will be assigned water neighbours.
        foreach(GraphPolygon poly in Polygons.Where(x => !x.IsWater && x.IsNextToWater))
        {
            foreach (GraphPolygon adjacentWater in poly.AdjacentPolygons.Where(x => x.IsWater))
            {
                foreach(GraphPolygon waterNeighbour in adjacentWater.AdjacentPolygons)
                {
                    if(!waterNeighbour.IsWater && waterNeighbour != poly && !poly.AdjacentPolygons.Contains(waterNeighbour) && !poly.WaterNeighbours.Contains(waterNeighbour)) 
                    {
                        AssignWaterNeighbours(poly, waterNeighbour);
                    }
                }
            }
        }

        // Look for islands (regions which have no land or water neighbours yet): Find the closest land by going through water regions and assign them water neighbours
        List<GraphPolygon> islands = Polygons.Where(x => !x.IsWater && x.LandNeighbours.Count == 0 && x.WaterNeighbours.Count == 0).ToList();
        foreach(GraphPolygon island in islands)
        {
            List<GraphPolygon> adjacentWaters = island.AdjacentPolygons.Where(x => x.IsWater).ToList();
            List<GraphPolygon> closestLand = FindClosestLandTo(island, adjacentWaters);
            foreach (GraphPolygon closeLand in closestLand) AssignWaterNeighbours(island, closeLand);
        }
    }
    private void AssignWaterNeighbours(GraphPolygon p1, GraphPolygon p2)
    {
        p1.WaterNeighbours.Add(p2);
        p2.WaterNeighbours.Add(p1);
    }

    private List<GraphPolygon> FindClosestLandTo(GraphPolygon origin, List<GraphPolygon> waterCluster)
    {
        List<GraphPolygon> closeLand = new List<GraphPolygon>();
        List<GraphPolygon> nextWaterCluster = new List<GraphPolygon>();
        foreach(GraphPolygon water in waterCluster)
        {
            foreach(GraphPolygon adj in water.AdjacentPolygons)
            {
                if(adj != origin)
                {
                    if (adj.IsWater) nextWaterCluster.Add(adj);
                    else closeLand.Add(adj);
                }
            }
        }
        if (closeLand.Count > 0) return closeLand;
        else return FindClosestLandTo(origin, nextWaterCluster);
    }

    /// <summary>
    /// Checks and returns if the given polygon is connected to any edge through its neighbours (or if it is floating in the void).
    /// </summary>
    private bool IsPolygonConnectedToEdge(GraphPolygon p, List<GraphPolygon> visitedPolygons)
    {
        if (p == null) throw new Exception("Polygon that should be checked is null");

        Queue<GraphPolygon> polygonsToCheck = new Queue<GraphPolygon>();
        polygonsToCheck.Enqueue(p);

        while(polygonsToCheck.Count > 0)
        {
            GraphPolygon poly = polygonsToCheck.Dequeue();
            visitedPolygons.Add(poly);
            if (poly.IsEdgePolygon) return true;
            foreach (GraphPolygon pn in poly.AdjacentPolygons) if (!visitedPolygons.Contains(pn)) polygonsToCheck.Enqueue(pn);
        }
        return false;
    }

    #endregion

    #region Map Modification

    private void MergePolygons(GraphPolygon p1, GraphPolygon p2)
    {
        NumMerges++;

        // Create relevant lists
        List<GraphConnection> consBetweenPolygons = p1.Connections.Where(x => x.Polygons.Contains(p2)).ToList();
        List<GraphNode> nodesBetweenPolygons = p1.Nodes.Where(x => x.Polygons.Count == 2 && x.Polygons.Contains(p2) && x.Type != BorderPointType.Edge).ToList();

        // Remove polygons from everywhere
        RemovePolygon(p1);
        RemovePolygon(p2);

        // Remove the connections between the nodes from everywhere
        foreach (GraphConnection con in consBetweenPolygons)
        {
            RemoveConnection(con);
        }

        // Remove the nodes between the polygons from everywhere
        foreach (GraphNode n in nodesBetweenPolygons)
        {
            foreach (GraphNode neighbourNode in n.ConnectedNodes) neighbourNode.ConnectedNodes.Remove(n);
            RemoveNode(n);
        }

        FindPolygonsFromNode(p1.Nodes.Where(x => !nodesBetweenPolygons.Contains(x)).ToList()[0], ignoreVisitedNodes: true, removePolygons: false);
    }

    /// <summary>
    /// Splits the given polygon into two seperate polygons at random. Can do nothing in certain cases. If forceSplit is true, it will try until a valid split is reached.
    /// </summary>
    public void SplitPolygon(GraphPolygon p)
    {
        if (p.Area < 3 * MinPolygonArea)
        {
            Debug.Log("Polygon is too small to split. Abort.");
            return;
        }

        NumSplits++;

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
        List<GraphPolygon> preSplitPolygons = new List<GraphPolygon>();
        foreach (GraphPolygon poly in splitNode.Polygons) preSplitPolygons.Add(poly);
        LastSegmentNodes.Clear();
        LastSegmentConnections.Clear();
        Actions.Enqueue(() => CreateSegment(splitNode, chosenAngle, changeAngle: false, canSplit: false));
        while (Actions.Count > 0) Actions.Dequeue().Invoke();

        // Find new polygons that were created in the split
        LastSegmentPolygons.Clear();
        FindPolygonsFromNode(splitNode, ignoreVisitedNodes: true, removePolygons: true, markFoundAsWater: p.IsWater);

        // We check if the split has generated invalid nodes. If it has, remove the last split and start again.
        InvalidNodes = Nodes.Where(x => x.Connections.Count > x.Polygons.Count && !EdgeNodes.Contains(x)).ToList();
        if (InvalidNodes.Count > 0)
        {
            //Debug.Log("Split created invalid nodes.");
            foreach (GraphPolygon poly in LastSegmentPolygons)
                RemovePolygon(poly);
            foreach (GraphConnection c in LastSegmentConnections)
                RemoveConnection(c);
            foreach (GraphNode n in LastSegmentNodes)
                RemoveNode(n);

            foreach (GraphPolygon pp in preSplitPolygons) AddPolygon(pp);

            SplitPolygon(p);
            return;
        }
        
        // We check if one of the new polyons is too small. If yes, remove the last split and start again.
        if(LastSegmentPolygons.Any(x => x.Area < MinPolygonArea))
        {
            //Debug.Log("Split created too small polygon.");
            foreach (GraphPolygon poly in LastSegmentPolygons) 
                RemovePolygon(poly);
            foreach (GraphConnection c in LastSegmentConnections)
                RemoveConnection(c);
            foreach (GraphNode n in LastSegmentNodes)
                RemoveNode(n);

            foreach (GraphPolygon pp in preSplitPolygons) AddPolygon(pp);

            SplitPolygon(p);
            return;
        }
    }

    #endregion

    #region Random Value Functions

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

    #endregion

    #region Getters

    public GraphNode CornerNode_BotLeft { get { return CornerNodes[0]; } }
    public GraphNode CornerNode_BotRight { get { return CornerNodes[1]; } }
    public GraphNode CornerNode_TopRight { get { return CornerNodes[2]; } }
    public GraphNode CornerNode_TopLeft { get { return CornerNodes[3]; } }

    #endregion
}
