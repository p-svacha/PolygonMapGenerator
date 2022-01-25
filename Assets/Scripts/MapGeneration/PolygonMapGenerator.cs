using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using static GeometryFunctions;
using MapGeneration.ContinentCreation;
using MapGeneration;
using System.Text;

public class PolygonMapGenerator : MonoBehaviour
{
    public MapGenerationState GenerationState;

    public MapGenerationSettings GenerationSettings; // Settings containing all necessary information for the kind of map that should be created
    public int Seed;
    private Map Map;
    public Action<Map> Callback; // Gets called when map generation is done

    public List<GraphNode> CornerNodes = new List<GraphNode>();
    public List<GraphNode> EdgeNodes = new List<GraphNode>(); // Edge nodes (are on the graph edge)
    public List<GraphNode> Nodes = new List<GraphNode>(); // ALL nodes (including edgeNodes)
    public List<GraphConnection> InGraphConnections = new List<GraphConnection>(); // All ingraph connections (WITHOUT edgeConnections)
    public List<GraphConnection> EdgeConnections = new List<GraphConnection>(); // Edge connections (both nodes of the connection are on the graph edge)
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public List<GraphPath> RiverPaths = new List<GraphPath>();

    public List<List<GraphPolygon>> Landmasses = new List<List<GraphPolygon>>();
    public List<List<GraphPolygon>> WaterBodies = new List<List<GraphPolygon>>();
    public List<List<GraphPolygon>> Continents = new List<List<GraphPolygon>>();

    // Temporary data during generation
    private List<GraphNode> InvalidNodes = new List<GraphNode>(); // Nodes that have less polygons than borders (and or not edge nodes)

    private List<GraphNode> LastSegmentNodes = new List<GraphNode>(); // Used for knowing which nodes were newly created in the last segment
    private List<GraphConnection> LastSegmentConnections = new List<GraphConnection>();
    private List<GraphPolygon> LastSegmentPolygons = new List<GraphPolygon>(); // Used for knowing which polygons were newly created in the last search period

    private List<GraphPolygon> LastRemovedPolygons = new List<GraphPolygon>(); // Used when splitting or merging polygons to find polygons that remained unchanged so the attributes can be transferred

    // Default values
    public const float DefaultRegionBorderWidth = 0.005f;
    public const float DefaulContinentBorderWidth = 0.02f;
    public const float DefaultShorelineBorderWidth = 0.02f;
    public static Color DefaultLandColor = new Color(0.74f, 0.93f, 0.70f);
    public static Color DefaultWaterColor = new Color(0.29f, 0.53f, 0.75f);

    // Layers (y-height of different objects so they don't overlap
    public static float LAYER_REGION = 0f;
    public static float LAYER_REGION_BORDER = 0.00001f;
    public static float LAYER_RIVER = 0.00002f;
    public static float LAYER_SHORE = 0.00003f;
    public static float LAYER_CONTINENT = 0.00003f;
    public static float LAYER_WATER_CONNECTION = 0.00004f;

    public static float LAYER_OVERLAY1 = 0.00010f;
    public static float LAYER_OVERLAY2 = 0.00011f;
    public static float LAYER_OVERLAY3 = 0.00012f;

    // Graph construction values
    private int NumStartLines;
    public const float DEFAULT_START_LINES_PER_KM = 0.6f; // Number of random walker lines per km the map generation starts with
    public const float BIG_OCEANS_START_LINES_PER_KM = 0.3f; // Number of random walker lines per km the map generation starts with
    public const bool RANDOM_START_LINE_POSITIONS = false; // If true, start lines start at completely random positions. If false, they start on an evenly distributed grid

    private float RealSplitChance; // Actual chance that a line splits into 2 lines at a vertex
    public const float DEFAULT_SPLIT_CHANCE = 0.0675f; // Chance that a line splits into 2 lines at a vertex with an average desired polygon area of 1. The value changes depending on the average desired polygon area
    public const float MAX_TURN_ANGLE = 45; // °
    public const float SNAP_DISTANCE = 0.07f; // Range, where a point snaps to a nearby point
    public const float MIN_SEGMENT_LENGTH = 0.08f;
    public const float MAX_SEGMENT_LENGTH = 0.13f;
    public const float MIN_SPLIT_ANGLE = 55; // °, the minimum angle when a split forms

    private Queue<Action> Actions = new Queue<Action>();

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
    }

    public void GenerateMap(MapGenerationSettings settings, Action<Map> callback = null)
    {
        GenerationSettings = settings;
        Callback = callback;

        StartCreation = DateTime.Now;
        StateTimeStamp = DateTime.Now;

        Reset();

        Seed = settings.Seed;
        Debug.Log("Generating " + GenerationSettings.Width + "/" + GenerationSettings.Height + GenerationSettings.MapType.ToString() + " map with region sizes between " + GenerationSettings.MinPolygonArea + " and " + GenerationSettings.MaxPolygonArea + ".");
        Debug.Log("SEED: " + Seed);
        UnityEngine.Random.InitState(Seed);

        RealSplitChance = DEFAULT_SPLIT_CHANCE * (1f / GenerationSettings.AvgPolygonArea);

        SwitchState(MapGenerationState.CreateMapBounds);
    }

    #region Generation State Flow

    void Update()
    {
        switch(GenerationState)
        {
            case MapGenerationState.CreateMapBounds:
                if(GenerationSettings.MapType == MapType.Regional) CreateNonIslandMapBounds();
                else CreateIslandMapBounds();
                SwitchState(MapGenerationState.CreateInitialGraph);
                break;

            case MapGenerationState.CreateInitialGraph:
                CreateInitialGraph();
                SwitchState(MapGenerationState.FindInitialPolygons);
                break;

            case MapGenerationState.FindInitialPolygons:
                FindAllPolygons();
                Debug.Log("Found " + Polygons.Count + " initial polygons before merge/split");
                SwitchState(MapGenerationState.RemoveInvalidNodes);
                break;

            case MapGenerationState.RemoveInvalidNodes:
                RemoveInvalidNodes();
                if (GenerationSettings.MapType == MapType.BigOceans) SwitchState(MapGenerationState.MergeSmallPolygons);
                else SwitchState(MapGenerationState.SplitBigPolygons);
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
                SwitchState(MapGenerationState.CreateTopology); // Skip river creation
                break;

            case MapGenerationState.CreateTopology:
                TopologyCreator.CreateTopology(this);
                SwitchState(MapGenerationState.CreateRivers);
                break;

            case MapGenerationState.CreateRivers:
                RiverCreator.CreateRivers(this);
                SwitchState(MapGenerationState.ApplyBiomes);
                break;

            case MapGenerationState.ApplyBiomes:
                BiomeCreator.CreateBiomes(this);
                SwitchState(MapGenerationState.FindWaterNeighbours);
                break;

            case MapGenerationState.FindWaterNeighbours:
                FindWaterNeighbours();
                SwitchState(MapGenerationState.CreateContinents);
                break;

            case MapGenerationState.CreateContinents:
                ContinentCreator.CreateContinents(this);
                SwitchState(MapGenerationState.DrawMap);
                break;

            case MapGenerationState.DrawMap:
                DrawMap();
                SwitchState(MapGenerationState.GenerationDone);
                Debug.Log(">----------- Map of size " + GenerationSettings.Area + "u^2 generated with " + Map.Regions.Count + " regions (" + Map.Regions.Where(x => !x.IsWater).Count() + " land, " + Map.Regions.Where(x => x.IsWater).Count() + " water) in " + (DateTime.Now - StartCreation).TotalSeconds + " seconds.\nPerformed " + NumMerges + " merges and " + NumSplits + " splits.");
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
        Map = null;

        NumSplits = 0;
        NumMerges = 0;

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
        GraphNode innerCornerBotRight = new GraphNode(new Vector2(GenerationSettings.Width - borderMargin, borderMargin), this);
        AddNode(innerCornerBotRight);
        GraphNode innerCornerTopRight = new GraphNode(new Vector2(GenerationSettings.Width - borderMargin, GenerationSettings.Height - borderMargin), this);
        AddNode(innerCornerTopRight);
        GraphNode innerCornerTopLeft = new GraphNode(new Vector2(borderMargin, GenerationSettings.Height - borderMargin), this);
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
        while(x < GenerationSettings.Width - 3 * MAX_SEGMENT_LENGTH)
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
        while (y < GenerationSettings.Height - 3 * MAX_SEGMENT_LENGTH)
        {
            y += RandomSegmentLength();
            x = GenerationSettings.Width - MAX_SEGMENT_LENGTH - RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerTopRight, isEdgeConnection: false);

        // Create top map edge between inner corners
        x = GenerationSettings.Width - borderMargin;
        lastNode = innerCornerTopRight;
        while (x > 3 * MAX_SEGMENT_LENGTH)
        {
            x -= RandomSegmentLength();
            y = GenerationSettings.Height - MAX_SEGMENT_LENGTH - RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode);
            AddConnection(lastNode, nextNode, isEdgeConnection: false);
            lastNode = nextNode;
        }
        AddConnection(lastNode, innerCornerTopLeft, isEdgeConnection: false);

        // Create left map edge between inner corners
        y = GenerationSettings.Height - borderMargin;
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
        while (x < GenerationSettings.Width - 1.5f * MAX_SEGMENT_LENGTH)
        {
            x += RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_BotRight, isEdgeConnection: true);

        // Create right map edge between inner corners
        x = GenerationSettings.Width;
        y = 0f;
        lastNode = CornerNode_BotRight;
        while (y < GenerationSettings.Height - 1.5f * MAX_SEGMENT_LENGTH)
        {
            y += RandomSegmentLength();
            GraphNode nextNode = new GraphNode(new Vector2(x, y), this);
            AddNode(nextNode, isEdgeNode: true);
            AddConnection(lastNode, nextNode, isEdgeConnection: true);
            lastNode = nextNode;
        }
        AddConnection(lastNode, CornerNode_TopRight, isEdgeConnection: true);

        // Create top map edge between inner corners
        x = GenerationSettings.Width;
        y = GenerationSettings.Height;
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
        y = GenerationSettings.Height;
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

        GraphNode cornerBotRight = new GraphNode(new Vector2(GenerationSettings.Width, 0), this);
        AddNode(cornerBotRight, isEdgeNode: true, isCornerNode: true);

        GraphNode cornerTopRight = new GraphNode(new Vector2(GenerationSettings.Width, GenerationSettings.Height), this);
        AddNode(cornerTopRight, isEdgeNode: true, isCornerNode: true);

        GraphNode cornerTopLeft = new GraphNode(new Vector2(0, GenerationSettings.Height), this);
        AddNode(cornerTopLeft, isEdgeNode: true, isCornerNode: true);
    }

    #endregion

    #region Graph Construction 

    private void CreateInitialGraph()
    {
        // Init random walkers at certain positions
        float startLinesPerKm = GenerationSettings.MapType == MapType.BigOceans ? BIG_OCEANS_START_LINES_PER_KM : DEFAULT_START_LINES_PER_KM;
        int xStartLines = (int)(startLinesPerKm * GenerationSettings.Width);
        int yStartLines = (int)(startLinesPerKm * GenerationSettings.Height);
        NumStartLines = xStartLines * yStartLines;
        float xStartLineStep = (float)(GenerationSettings.Width) / (xStartLines + 1f);
        float yStartLineStep = (float)(GenerationSettings.Height) / (yStartLines + 1f);
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
        Vector2 endPoint = new Vector2((float)(startNode.Vertex.x + (Math.Sin(Mathf.Deg2Rad * angle) * length)), (float)(startNode.Vertex.y + (Math.Cos(Mathf.Deg2Rad * angle) * length)));

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

            if(canSplit && UnityEngine.Random.Range(0f, 1f) < RealSplitChance)
            {
                //Debug.Log("Split at " + startNode.Vertex.ToString());
                List<int> nodeAngles = startNode.GetNodeAngles();
                float splitAngle = RandomValidAngle(nodeAngles);
                Actions.Enqueue(() => CreateSegment(startNode, splitAngle, changeAngle: false, canSplit: canSplit));
            }
        }
    }

    public void AddNode(GraphNode n, bool isEdgeNode = false, bool isCornerNode = false)
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
                newPolygon.AddAdjacentPolygon(neighbour);
                neighbour.AddAdjacentPolygon(newPolygon);
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
        while (Polygons.OrderBy(x => x.Area).First().Area < GenerationSettings.MinPolygonArea)
        {
            GraphPolygon smallestNeighbour = smallest.AdjacentPolygons.OrderBy(x => x.Area).First();
            MergePolygons(smallest, smallestNeighbour);
            smallest = Polygons.OrderBy(x => x.Area).First();
        }
    }

    private void SplitBigPolygons()
    {
        // Debug Info for monitoring performance of initial graph
        List<float> orderedAreas = Polygons.Select(x => x.Area).OrderByDescending(x => x).ToList();
        float initialAvgArea = orderedAreas.Average();
        float initialMedianArea = orderedAreas[orderedAreas.Count / 2];
        Debug.Log(DEFAULT_START_LINES_PER_KM + " ( " + NumStartLines + " ) start lines per km in an area of " + GenerationSettings.Area + " created " + Polygons.Count + " initial polygons with an average area of " + initialAvgArea + " and a median area of " + initialMedianArea);

        GraphPolygon largest = GenerationSettings.MapType != MapType.Regional ? Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First() : Polygons.OrderByDescending(x => x.Area).First();
        while (largest.Area > GenerationSettings.MaxPolygonArea)
        {
            SplitPolygon(largest);
            largest = GenerationSettings.MapType != MapType.Regional ? Polygons.Where(x => !x.IsEdgePolygon).OrderByDescending(x => x.Area).First() : Polygons.OrderByDescending(x => x.Area).First();
        }
    }

    /// <summary>
    /// Redraw should be called when regions of the map have changed, for example after a merge/split or after turnToLand/turnToWater
    /// </summary>
    public void Redraw()
    {
        SwitchState(MapGenerationState.FindWaterNeighbours);
    }

    public Map DrawMap()
    {
        // Validate
        foreach (GraphPolygon p in Polygons)
            foreach (GraphNode n in p.Nodes)
                if (!Nodes.Contains(n)) Debug.Log("################################ Node " + n.ToString() + " not found!");

        Map = new Map(GenerationSettings);

        // Init all gameobjects
        GameObject mapObject = new GameObject("Map");
        Map.RootObject = mapObject;
        Map.BorderPointContainer = new GameObject("BorderPoints");
        Map.BorderPointContainer.transform.SetParent(mapObject.transform);
        Map.BorderContainer = new GameObject("Borders");
        Map.BorderContainer.transform.SetParent(mapObject.transform);
        Map.RegionContainer = new GameObject("Regions");
        Map.RegionContainer.transform.SetParent(mapObject.transform);
        Map.RiverContainer = new GameObject("Rivers");
        Map.RiverContainer.transform.SetParent(mapObject.transform);
        Map.ContinentContainer = new GameObject("Continents");
        Map.ContinentContainer.transform.SetParent(mapObject.transform);
        Map.WaterConnectionContainer = new GameObject("Water Connections");
        Map.WaterConnectionContainer.transform.SetParent(mapObject.transform);

        // Add border points
        Map.BorderPoints = new List<BorderPoint>();
        foreach (GraphNode n in Nodes)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(Map.BorderPointContainer.transform);
            BorderPoint borderPoint = sphere.AddComponent<BorderPoint>();
            n.BorderPoint = borderPoint;
            Map.BorderPoints.Add(borderPoint);
        }

        // Add inmap borders (no edge borders)
        Map.Borders = new List<Border>();
        foreach (GraphConnection c in InGraphConnections)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(Map.BorderContainer.transform);
            Border border = cube.AddComponent<Border>();
            c.Border = border;
            Map.Borders.Add(border);
        }
        // Add edge borders
        Map.EdgeBorders = new List<Border>();
        foreach (GraphConnection c in EdgeConnections)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(Map.BorderContainer.transform);
            Border border = cube.AddComponent<Border>();
            c.Border = border;
            Map.EdgeBorders.Add(border);
        }

        // Add regions
        Map.Regions = new List<Region>();
        foreach (GraphPolygon p in Polygons)
        {
            // Mesh
            GameObject polygon = MeshGenerator.GeneratePolygon(p.Nodes.Select(x => x.Vertex).ToList(), this, layer: LAYER_REGION);
            //polygon.transform.SetParent(Map.RegionContainer.transform);

            // Collider
            polygon.AddComponent<MeshCollider>();

            // Region
            Region region = polygon.AddComponent<Region>();
            p.Region = region;
            Map.Regions.Add(region);
        }

        foreach (GraphNode n in Nodes) n.BorderPoint.Init(Map, n);
        foreach (GraphConnection c in InGraphConnections) c.Border.Init(c);
        foreach (GraphConnection c in EdgeConnections) c.Border.Init(c);
        foreach (GraphPolygon p in Polygons) p.Region.Init(p);

        // Add rivers
        Map.Rivers = new List<River>();
        foreach (GraphPath r in RiverPaths)
        {
            River riverObject = RiverCreator.CreateRiverObject(r, this);
            riverObject.transform.SetParent(Map.RiverContainer.transform);
            Map.Rivers.Add(riverObject);
        }

        Map.ToggleHideBorderPoints();
        Map.ToggleHideBorders();

        // Add landmasses
        Map.Landmasses = new List<Landmass>();
        foreach(List<GraphPolygon> landmassList in Landmasses)
        {
            GameObject landmassObject = new GameObject("Landmass");
            landmassObject.transform.SetParent(Map.RegionContainer.transform);
            Landmass landmass = landmassObject.AddComponent<Landmass>();
            landmass.Init(landmassList.Select(x => x.Region).ToList());

            foreach(Region r in landmass.Regions)
            {
                r.Landmass = landmass;
                r.transform.SetParent(landmassObject.transform);
            }

            Map.Landmasses.Add(landmass);
        }

        // Add water bodies
        Map.WaterBodies = new List<WaterBody>();
        foreach(List<GraphPolygon> waterBodyList in WaterBodies)
        {
            GameObject waterBodyObject = new GameObject("WaterBody");
            waterBodyObject.transform.SetParent(Map.RegionContainer.transform);
            WaterBody waterBody = waterBodyObject.AddComponent<WaterBody>();
            waterBody.Init(waterBodyList.Select(x => x.Region).ToList());

            foreach(Region r in waterBody.Regions)
            {
                r.WaterBody = waterBody;
                r.transform.SetParent(waterBodyObject.transform);
            }

            Map.WaterBodies.Add(waterBody);
        }

        // Add continents
        Map.Continents = new List<Continent>();
        foreach (List<GraphPolygon> continentList in Continents)
        {
            GameObject continentObject = new GameObject("Continent");
            continentObject.transform.SetParent(Map.ContinentContainer.transform);
            Continent continent = continentObject.AddComponent<Continent>();
            continent.Init(continentList.Select(x => x.Region).ToList());

            foreach (Region r in continent.Regions) r.Continent = continent;

            Map.Continents.Add(continent);
        }

        // Add water connections
        Map.WaterConnections = new List<WaterConnection>();
        List<GraphPolygon> visitedPolygons = new List<GraphPolygon>();
        foreach(GraphPolygon polygon in Polygons)
        {
            visitedPolygons.Add(polygon);
            foreach(GraphPolygon waterNeighbour in polygon.WaterNeighbours)
            {
                if(!visitedPolygons.Contains(waterNeighbour))
                {
                    List<GraphNode> closestNodes = PolygonMapFunctions.GetClosestPolygonNodes(polygon, waterNeighbour, ignoreMultiNodes: true, shoreOnly: true);
                    if(closestNodes[0] == null || closestNodes[1] == null)
                    {
                        polygon.Region.SetColor(Color.red);
                        waterNeighbour.Region.SetColor(Color.red);
                        throw new Exception("TODO: fix this bug - No good nodes found, look for red polygons to see where");
                    }
                    GameObject waterConObject = MeshGenerator.DrawLine(closestNodes[0].Vertex, closestNodes[1].Vertex, 0.02f, new Color(0.2f, 0.2f, 0.2f), LAYER_WATER_CONNECTION, 0.001f);
                    waterConObject.transform.SetParent(Map.WaterConnectionContainer.transform);
                    WaterConnection waterConnection = waterConObject.AddComponent<WaterConnection>();
                    waterConnection.Init(polygon.Region, waterNeighbour.Region, closestNodes[0].BorderPoint, closestNodes[1].BorderPoint);

                    polygon.Region.WaterConnections.Add(waterConnection);
                    waterNeighbour.Region.WaterConnections.Add(waterConnection);

                    Map.WaterConnections.Add(waterConnection);
                }
            }
        }

        Callback?.Invoke(Map);
        return Map;
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
        foreach (GraphPolygon poly in Polygons)
        {
            poly.WaterNeighbours.Clear();
            poly.UpdateNeighbours();
        }

        // For each landmass, find the shortest distance from a polygon in that landmass to a polygon in another landmass. They will be connected.
        // All other polygon pairs, that are within a certain margin of the shortest distance (sorted from shortest to longest distance) will also get connected if
        // there is no water connection which connects very similar polygons (polygons within a certain range)
        foreach(List<GraphPolygon> landmassPolygons in Landmasses)
        {
            if (Landmasses.Count == 1) break;

            List<GraphPolygon> nonLandmassPolygons = Polygons.Where(x => !x.IsWater && !landmassPolygons.Contains(x)).ToList();

            // First make a dictionary that stores the poi-distance between every landmass polygon to every non-landmass polygon
            Dictionary<Tuple<GraphPolygon, GraphPolygon>, float> poiDistances = new Dictionary<Tuple<GraphPolygon, GraphPolygon>, float>();
            foreach(GraphPolygon landmassPoly in landmassPolygons)
            {
                foreach(GraphPolygon nonLandmassPoly in nonLandmassPolygons)
                {
                    poiDistances.Add(new Tuple<GraphPolygon, GraphPolygon>(landmassPoly, nonLandmassPoly), Vector2.Distance(landmassPoly.CenterPoi, nonLandmassPoly.CenterPoi));
                }
            }
            poiDistances = poiDistances.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // For the shortest connections, calculate the real shortest distances
            Dictionary<Tuple<GraphPolygon, GraphPolygon>, float> realDistances = new Dictionary<Tuple<GraphPolygon, GraphPolygon>, float>();
            float poiMargin = 3f;
            float shortestPoiDistance = poiDistances.First().Value;
            foreach(KeyValuePair<Tuple<GraphPolygon, GraphPolygon>, float> kvp in poiDistances)
            {
                if (kvp.Value > shortestPoiDistance + poiMargin) break;
                realDistances.Add(kvp.Key, PolygonMapFunctions.GetPolygonDistance(kvp.Key.Item1, kvp.Key.Item2));
            }
            realDistances = realDistances.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // Now we add the shortest real distance as a connection and all others within a margin too if they are not reachable within a margin
            float realMargin = 0.5f;
            int regionDistanceMargin = 4; // polygons need to have at least x regions between them for the connection to establish
            float shortestRealDistance = realDistances.First().Value;
            foreach (KeyValuePair<Tuple<GraphPolygon, GraphPolygon>, float> kvp in realDistances)
            {
                if (kvp.Value == shortestRealDistance) AssignWaterNeighbours(kvp.Key.Item1, kvp.Key.Item2);
                else if(kvp.Value <= shortestRealDistance + realMargin)
                {
                    if(PolygonMapFunctions.GetRegionDistance(kvp.Key.Item1, kvp.Key.Item2) >= regionDistanceMargin) AssignWaterNeighbours(kvp.Key.Item1, kvp.Key.Item2);
                }
            }
        }

        // Connect polygons within the same landmass that are really close to each other through water but a long distance through land
        float sameLandmassPoiDistanceMargin = 3f;
        float sameLandmassRealDistanceMargin = 0.8f;
        float sameLandmassRegionDistanceMargin = 5;
        foreach (List<GraphPolygon> landmassPolygons in Landmasses)
        {
            // First make a dictionary that stores the poi-distance between all landmass polygons that are eligible for a water connection (meaning they have to be far enough apart through land connections)
            Dictionary<Tuple<GraphPolygon, GraphPolygon>, float> poiDistances = new Dictionary<Tuple<GraphPolygon, GraphPolygon>, float>();

            foreach (GraphPolygon p1 in landmassPolygons.Where(x => x.AdjacentPolygons.Where(y => y.IsWater).Count() > 0))
            {
                foreach(GraphPolygon p2 in landmassPolygons.Where(x => x.AdjacentPolygons.Where(y => y.IsWater).Count() > 0))
                {
                    if(p1 != p2 && PolygonMapFunctions.GetRegionDistance(p1, p2) >= sameLandmassRegionDistanceMargin)
                    {
                        float poiDistance = Vector2.Distance(p1.CenterPoi, p2.CenterPoi);
                        if(poiDistance <= sameLandmassPoiDistanceMargin) poiDistances.Add(new Tuple<GraphPolygon, GraphPolygon>(p1, p2), poiDistance);
                    }
                }
            }
            poiDistances = poiDistances.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // Then we calculate the real distance for polygons that are close
            Dictionary<Tuple<GraphPolygon, GraphPolygon>, float> realDistances = new Dictionary<Tuple<GraphPolygon, GraphPolygon>, float>();
            foreach (KeyValuePair<Tuple<GraphPolygon, GraphPolygon>, float> kvp in poiDistances)
            {
                float realDistance = PolygonMapFunctions.GetPolygonDistance(kvp.Key.Item1, kvp.Key.Item2);
                if(realDistance <= sameLandmassRealDistanceMargin) realDistances.Add(kvp.Key, realDistance);

            }
            realDistances = realDistances.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // add water connections ordered starting from shortest if region distance is still low enough
            foreach (KeyValuePair<Tuple<GraphPolygon, GraphPolygon>, float> kvp in realDistances)
            {
                if (PolygonMapFunctions.GetRegionDistance(kvp.Key.Item1, kvp.Key.Item2) >= sameLandmassRegionDistanceMargin) AssignWaterNeighbours(kvp.Key.Item1, kvp.Key.Item2);
            }
        }


            // Connect clusters (region groups that are connected through water or land) until there is only one clutster left (so every region is reachable from every other region)
        List<List<GraphPolygon>> clusters = PolygonMapFunctions.FindClusters(Polygons.Where(x => !x.IsWater).ToList(), landConnectionsOnly: false);
        while(clusters.Count > 1)
        {
            // Connect the two closest clusters
            float shortestDistance = float.MaxValue;
            GraphPolygon shortestDistancePoly1 = null;
            GraphPolygon shortestDistancePoly2 = null;
            foreach (List<GraphPolygon> cluster1 in clusters)
            {
                foreach(List<GraphPolygon> cluster2 in clusters)
                {
                    if(cluster1 != cluster2)
                    {
                        List<GraphPolygon> closestPair = PolygonMapFunctions.FindClosestPolygons(cluster1, cluster2);
                        float distance = Vector2.Distance(closestPair[0].Centroid, closestPair[1].Centroid);
                        if(distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            shortestDistancePoly1 = closestPair[0];
                            shortestDistancePoly2 = closestPair[1];
                        }
                    }
                }
            }

            AssignWaterNeighbours(shortestDistancePoly1, shortestDistancePoly2);
            clusters = PolygonMapFunctions.FindClusters(Polygons.Where(x => !x.IsWater).ToList(), landConnectionsOnly: false);
        }
    }

    private void AssignWaterNeighbours(GraphPolygon p1, GraphPolygon p2)
    {
        p1.AddWaterNeighbour(p2);
        p2.AddWaterNeighbour(p1);
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

    public bool CanMergePolygons(GraphPolygon p1, GraphPolygon p2)
    {
        if (p1 == p2) return false;
        if (!p1.AdjacentPolygons.Contains(p2)) return false;
        if (!p2.AdjacentPolygons.Contains(p1)) return false;
        return true;
    }

    public void MergePolygons(GraphPolygon p1, GraphPolygon p2)
    {
        if (!CanMergePolygons(p1, p2)) return;

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

    public bool CanSplitPolygon(GraphPolygon p)
    {
        return p.Area > 3 * GenerationSettings.MinPolygonArea;
    }

    /// <summary>
    /// Splits the given polygon into two seperate polygons at random. Can do nothing in certain cases. If forceSplit is true, it will try until a valid split is reached.
    /// </summary>
    public void SplitPolygon(GraphPolygon p)
    {
        if (!CanSplitPolygon(p)) return;

        NumSplits++;

        // Select random node where the split start
        int splitNodeId = UnityEngine.Random.Range(0, p.Nodes.Count);

        GraphNode splitNode = p.Nodes[splitNodeId];
        GraphNode beforeNode = p.Nodes[splitNodeId == 0 ? p.Nodes.Count - 1 : splitNodeId - 1];
        GraphNode afterNode = p.Nodes[splitNodeId == p.Nodes.Count - 1 ? 0 : splitNodeId + 1];

        int beforeConnectionAngle = (int)(Vector2.SignedAngle(new Vector2(beforeNode.Vertex.x - splitNode.Vertex.x, beforeNode.Vertex.y - splitNode.Vertex.y), new Vector2(0, 1)));
        int afterConnectionAngle = (int)(Vector2.SignedAngle(new Vector2(afterNode.Vertex.x - splitNode.Vertex.x, afterNode.Vertex.y - splitNode.Vertex.y), new Vector2(0, 1)));

        beforeConnectionAngle = Mod(beforeConnectionAngle, 360);
        afterConnectionAngle = Mod(afterConnectionAngle, 360);

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
        if(LastSegmentPolygons.Any(x => x.Area < GenerationSettings.MinPolygonArea))
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
        return new Vector2(UnityEngine.Random.Range(MAX_SEGMENT_LENGTH, GenerationSettings.Width - MAX_SEGMENT_LENGTH), UnityEngine.Random.Range(MAX_SEGMENT_LENGTH, GenerationSettings.Height - MAX_SEGMENT_LENGTH));
    }
    public int RandomValidAngle(List<int> angles) // Returns an angle that is at least MIN_SPLIT_ANGLE away from an angle in the list
    {
        List<int> otherAngles = new List<int>();
        foreach(int angle in angles)
        {
            int modAngle = Mod(angle, 360);
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
