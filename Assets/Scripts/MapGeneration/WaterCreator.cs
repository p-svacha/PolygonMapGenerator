using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions.Must;

public static class WaterCreator
{
    private static float OUTER_OCEAN_SIZE = 10f;

    private static int MIN_BALL_OCEANS = 1;
    private static int SIZE_PER_BALL_OCEAN = 180;
    private static int SIZE_PER_BALL_OCEAN_RANGE = 60;
    private static float OCEAN_ISLAND_CHANCE = 0.15f; // Chance that a region within an ocean doesn't get transformed

    private static int MIN_CONTINENT_CUTS = 1;
    private static int SIZE_PER_CONTINENT_CUT = 180;
    private static int SIZE_PER_CONTINENT_CUT_RANGE = 60;
    private static float MIN_CONTINENT_CUT_WIDTH = 0f;
    private static float MAX_CONTINENT_CUT_WIDTH = 1.5f;
    private static float CONTINENT_SPLIT_ISLAND_CHANCE = 0.05f;

    private static float RANDOM_OCEAN_PERC = 0.04f;
    private static float EXPAND_OCEAN_PERC = 0.08f;

    private static float MIN_TOTAL_LAND_COVERAGE = 0.52f;

    private static float START_RIVER_SIZE = 2f;
    private static int SIZE_PER_RIVER = 23;
    private static int SIZE_PER_RIVER_RANGE = 6;
    private static float MIN_RIVER_EXPANSION_RATE = 0.1f;
    private static float MAX_RIVER_EXPANSION_RATE = 0.4f;
    private static float MIN_RIVER_MAX_WIDTH = 6.5f;
    private static float MAX_RIVER_MAX_WIDTH = 11f;

    public static void CreateWaters(PolygonMapGenerator PMG)
    {
        CreateOuterOcean(PMG);
        TurnEdgePolygonsToWater(PMG);
        CreateContinents(PMG);
        TurnRandomPolygonsToWater(PMG);
        ExpandOceans(PMG);
        CreateBallOceans(PMG);
        ExpandLand(PMG);
        CreateRivers(PMG);
    }

    public static void HandleInput(PolygonMapGenerator PMG)
    {
        // W - Create random water
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.W))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoTurnRandomPolygonToWater(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }

        // E - Expand ocean
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.E))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoExpandOcean(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }

        // C - Create continent cut
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.C))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoCreateContinentCut(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }

        // O - Create ocean
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.O))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoCreateBallOcean(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }

        // L - Expand land
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.L))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoExpandLand(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }

        // V - Create River
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.V))
        {
            PMG.Map.DestroyAllGameObjects();
            PMG.Map = null;
            DoCreateRiver(PMG);
            PMG.GenerationState = MapGenerationState.DrawMap;
        }
    }

    private static void CreateOuterOcean(PolygonMapGenerator PMG)
    {
        // Create outer edge nodes
        GraphNode oc1 = new GraphNode(new Vector2(-OUTER_OCEAN_SIZE, -OUTER_OCEAN_SIZE), PMG);
        GraphNode oc2 = new GraphNode(new Vector2(PMG.Width + OUTER_OCEAN_SIZE, -OUTER_OCEAN_SIZE), PMG);
        GraphNode oc3 = new GraphNode(new Vector2(PMG.Width + OUTER_OCEAN_SIZE, PMG.Height + OUTER_OCEAN_SIZE), PMG);
        GraphNode oc4 = new GraphNode(new Vector2(-OUTER_OCEAN_SIZE, PMG.Height + OUTER_OCEAN_SIZE), PMG);

        // Adding new water polygons
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[0], oc1, oc2, PMG.CornerNodes[1] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[1], oc2, oc3, PMG.CornerNodes[2] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[2], oc3, oc4, PMG.CornerNodes[3] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[3], oc4, oc1, PMG.CornerNodes[0] }, new List<GraphConnection>(), outerPolygon: true);
    }

    private static void TurnEdgePolygonsToWater(PolygonMapGenerator PMG)
    {
        foreach (GraphPolygon p in PMG.Polygons.Where(x => x.IsEdgePolygon)) TurnPolygonToWater(p);
    }

    private static void CreateBallOceans(PolygonMapGenerator PMG)
    {
        int sizePerBallOcean = UnityEngine.Random.Range(SIZE_PER_BALL_OCEAN - SIZE_PER_BALL_OCEAN_RANGE, SIZE_PER_BALL_OCEAN + SIZE_PER_BALL_OCEAN_RANGE);
        int numBallOceans = Math.Max(PMG.MapSize / sizePerBallOcean, MIN_BALL_OCEANS);
        Debug.Log("Creating " + numBallOceans + " round oceans.");
        for (int i = 0; i < numBallOceans; i++) DoCreateBallOcean(PMG);
    }
    private static void DoCreateBallOcean(PolygonMapGenerator PMG)
    {

        int avgMapEdgeSize = (int)(Math.Sqrt(PMG.MapSize));

        // Get radius
        float minRadius = avgMapEdgeSize * 0.1f;
        float maxRadius = avgMapEdgeSize * 0.3f;
        float radius = UnityEngine.Random.Range(minRadius, maxRadius);

        float maxInnerDistance = radius / 4;
        float maxOuterDistance = radius / 2;

        float circleCenterX = 0, circleCenterY = 0;
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0: // Bottom side
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, PMG.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, maxInnerDistance);
                break;
            case 1: // Right side
                circleCenterX = UnityEngine.Random.Range(PMG.Width - maxInnerDistance, PMG.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, PMG.Height + maxOuterDistance);
                break;
            case 2: // Top side
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, PMG.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(PMG.Height - maxInnerDistance, PMG.Height + maxOuterDistance);
                break;
            case 3: // Left side
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, maxInnerDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, PMG.Height + maxOuterDistance);
                break;
        }
        Vector2 circleCenter = new Vector2(circleCenterX, circleCenterY);
        
        Debug.Log("Center at " + circleCenter.ToString() + " with radius " + radius);
        List<GraphPolygon> newOceanPolygons = new List<GraphPolygon>();
        foreach (GraphNode n in PMG.Nodes.Where(x => !x.IsWaterNode()))
        {
            float distance = Vector2.Distance(n.Vertex, circleCenter);
            if (distance < maxRadius)
            {
                foreach (GraphPolygon p in n.Polygons)
                    if (!newOceanPolygons.Contains(p)) 
                        newOceanPolygons.Add(p);
            }
        }
        foreach (GraphPolygon p in newOceanPolygons)
            if (UnityEngine.Random.Range(0f, 1f) > OCEAN_ISLAND_CHANCE) 
                TurnPolygonToWater(p);

    }

    private static void CreateContinents(PolygonMapGenerator PMG)
    {
        int sizePerContinentCuts = UnityEngine.Random.Range(SIZE_PER_CONTINENT_CUT - SIZE_PER_CONTINENT_CUT_RANGE, SIZE_PER_CONTINENT_CUT + SIZE_PER_CONTINENT_CUT_RANGE);
        int numContinentCuts = Math.Max(PMG.MapSize / sizePerContinentCuts, MIN_CONTINENT_CUTS);
        Debug.Log("Performing " + numContinentCuts + " continent cuts.");
        for (int i = 0; i < numContinentCuts; i++) DoCreateContinentCut(PMG);
    }
    public static void DoCreateContinentCut(PolygonMapGenerator PMG)
    {
        // Define random cut circle
        int avgMapEdgeSize = (int)(Math.Sqrt(PMG.MapSize));
        float minDistanceFromMap = avgMapEdgeSize * 0.1f;
        float maxDistanceFromMap = avgMapEdgeSize * 0.7f;

        float cutCircleCenterX = 0, cutCircleCenterY = 0;
        switch(UnityEngine.Random.Range(0,4))
        {
            case 0: // Bottom side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, PMG.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-maxDistanceFromMap, -minDistanceFromMap);
                break;
            case 1: // Right side
                cutCircleCenterX = UnityEngine.Random.Range(PMG.Width + minDistanceFromMap, PMG.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-minDistanceFromMap, PMG.Height + maxDistanceFromMap);
                break;
            case 2: // Top side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, PMG.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(PMG.Height + minDistanceFromMap, PMG.Height + maxDistanceFromMap);
                break;
            case 3: // Left side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, -minDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-minDistanceFromMap, PMG.Height + maxDistanceFromMap);
                break;
        }
        Vector2 cutCircleCenter = new Vector2(cutCircleCenterX, cutCircleCenterY);
        int cutCircleRadius = avgMapEdgeSize;

        // Check for each node if it is close to cut circle 
        float distanceMargin = UnityEngine.Random.Range(MIN_CONTINENT_CUT_WIDTH, MAX_CONTINENT_CUT_WIDTH);

        List<GraphPolygon> newOceanPolygons = new List<GraphPolygon>();
        foreach (GraphNode n in PMG.Nodes.Where(x => !x.IsWaterNode()))
        {
            float distance = Vector2.Distance(n.Vertex, cutCircleCenter);
            if(distance >= cutCircleRadius - distanceMargin && distance <= cutCircleRadius + distanceMargin)
            {
                foreach (GraphPolygon p in n.Polygons)
                    if (!newOceanPolygons.Contains(p))
                        newOceanPolygons.Add(p);
            }
        }
        foreach (GraphPolygon p in newOceanPolygons)
            if (UnityEngine.Random.Range(0f, 1f) > CONTINENT_SPLIT_ISLAND_CHANCE)
                TurnPolygonToWater(p);
    }

    

    public static void ExpandOceans(PolygonMapGenerator PMG)
    {
        int numExpansions = (int)(PMG.Polygons.Count * EXPAND_OCEAN_PERC);
        Debug.Log("Expanding " + numExpansions + "x the ocean.");
        for (int i = 0; i < numExpansions; i++) DoExpandOcean(PMG);
    }
    public static void DoExpandOcean(PolygonMapGenerator PMG)
    {
        List<GraphPolygon> shorePolygons = PMG.Polygons.Where(x => !x.IsWater && x.IsNextToWater).ToList();
        GraphPolygon newWater = shorePolygons[UnityEngine.Random.Range(0, shorePolygons.Count)];
        TurnPolygonToWater(newWater);
    }

    private static void TurnRandomPolygonsToWater(PolygonMapGenerator PMG)
    {
        int numWaters = (int)(PMG.Polygons.Count * RANDOM_OCEAN_PERC);
        Debug.Log("Creating " + numWaters + " random waters.");
        for (int i = 0; i < numWaters; i++) DoTurnRandomPolygonToWater(PMG);
    }
    public static void DoTurnRandomPolygonToWater(PolygonMapGenerator PMG)
    {
        List<GraphPolygon> landPolygons = PMG.Polygons.Where(x => !x.IsWater).ToList();
        GraphPolygon newWater = landPolygons[UnityEngine.Random.Range(0, landPolygons.Count)];
        TurnPolygonToWater(newWater);
    }

    public static void ExpandLand(PolygonMapGenerator PMG)
    {
        int targetAmount = (int)(PMG.Polygons.Count * MIN_TOTAL_LAND_COVERAGE);
        int landPolygons = PMG.Polygons.Where(x => !x.IsWater).Count();
        int numExpansions = targetAmount - landPolygons;
        Debug.Log("Expanding " + numExpansions + "x the land.");
        for (int i = 0; i < numExpansions; i++) DoExpandLand(PMG);
    }
    public static void DoExpandLand(PolygonMapGenerator PMG)
    {
        List<GraphPolygon> waterShorePolygons = PMG.Polygons.Where(x => !x.IsEdgePolygon && x.IsWater && x.IsNextToLand()).ToList();
        if (waterShorePolygons.Count == 0) return;
        GraphPolygon newLand = waterShorePolygons[UnityEngine.Random.Range(0, waterShorePolygons.Count)];
        TurnPolygonToLand(newLand);
    }

    public static void CreateRivers(PolygonMapGenerator PMG)
    {
        int sizePerRiver = UnityEngine.Random.Range(SIZE_PER_RIVER - SIZE_PER_RIVER_RANGE, SIZE_PER_RIVER + SIZE_PER_RIVER_RANGE);
        int numRivers = PMG.MapSize / sizePerRiver;
        Debug.Log("Creating " + numRivers + " rivers.");
        for (int i = 0; i < numRivers; i++) DoCreateRiver(PMG);
    }
    public static void DoCreateRiver(PolygonMapGenerator PMG)
    {
        List<GraphConnection> inlandConnections = PMG.InGraphConnections.Where(x => x.River == null && x.Type == BorderType.Inland && x.StartNode.River == null && x.EndNode.River == null).ToList();
        if (inlandConnections.Count == 0) return;

        GraphPath river = new GraphPath();

        List<GraphConnection> forbiddenConnections = new List<GraphConnection>();

        GraphConnection lastSegment = inlandConnections[UnityEngine.Random.Range(0, inlandConnections.Count)];
        GraphNode currentEndPoint;
        GraphNode lastEndPoint;
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            currentEndPoint = lastSegment.StartNode;
            lastEndPoint = lastSegment.EndNode;
        }
        else
        {
            currentEndPoint = lastSegment.EndNode;
            lastEndPoint = lastSegment.StartNode;
        }
        forbiddenConnections.AddRange(lastEndPoint.Connections);
        river.Nodes.Add(lastEndPoint);
        river.Nodes.Add(currentEndPoint);
        river.Connections.Add(lastSegment);

        bool endRiver = (currentEndPoint.Type == BorderPointType.Shore || currentEndPoint.River != null);
        float expansionRate = UnityEngine.Random.Range(MIN_RIVER_EXPANSION_RATE, MAX_RIVER_EXPANSION_RATE);
        float riverWidth = START_RIVER_SIZE;
        float maxWidth = UnityEngine.Random.Range(MIN_RIVER_MAX_WIDTH, MAX_RIVER_MAX_WIDTH);
        TurnConnectionToRiver(lastSegment, river, riverWidth);

        while (!endRiver)
        {
            riverWidth += expansionRate;
            if (riverWidth > maxWidth) riverWidth = maxWidth;
            List<GraphConnection> candidates = currentEndPoint.Connections.Where(x => x.River == null && x.Type != BorderType.Shore && !forbiddenConnections.Contains(x) && (x.StartNode.River == null || x.EndNode.River == null)).ToList();
            if (candidates.Count == 0)
            {
                
                TurnRiverToLand(lastSegment);
                river.Connections.Remove(lastSegment);
                river.Nodes.Remove(currentEndPoint);

                currentEndPoint = lastSegment.EndNode == currentEndPoint ? lastSegment.StartNode : lastSegment.EndNode;
                foreach (GraphConnection c in currentEndPoint.Connections) if(forbiddenConnections.Contains(c)) forbiddenConnections.Remove(c);
                forbiddenConnections.Add(lastSegment);
                lastSegment = currentEndPoint.Connections.FirstOrDefault(x => x.River != null);
                if (lastSegment == null) return;
            }
            else
            {
                lastSegment = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                river.Connections.Add(lastSegment);

                lastEndPoint = currentEndPoint;
                forbiddenConnections.AddRange(lastEndPoint.Connections);

                currentEndPoint = lastSegment.StartNode == currentEndPoint ? lastSegment.EndNode : lastSegment.StartNode;
                river.Nodes.Add(currentEndPoint);

                endRiver = (currentEndPoint.Type == BorderPointType.Shore || (currentEndPoint.River != null && currentEndPoint.Connections.Where(x => x.River != null).Min(x => x.RiverWidth) > riverWidth));
                TurnConnectionToRiver(lastSegment, river, riverWidth);
            }
        }

        // Add polygons to river
        foreach(GraphConnection c in river.Connections)
        {
            foreach (GraphPolygon p in c.Polygons)
            {
                if (!river.Polygons.Contains(p)) river.Polygons.Add(p);
                p.Rivers.Add(river);
            }
        }
        PMG.RiverPaths.Add(river);
    }


    private static void TurnPolygonToWater(GraphPolygon p)
    {
        if (!p.IsWater)
        {
            p.IsWater = true;
            foreach (GraphNode n in p.Nodes) n.SetType();
            foreach (GraphConnection c in p.Connections) c.SetType();
            foreach (GraphPolygon pn in p.Neighbours) pn.IsNextToWater = true;
        }
    }
    private static void TurnPolygonToLand(GraphPolygon p)
    {
        if (p.IsWater)
        {
            p.IsWater = false;
            foreach (GraphNode n in p.Nodes) n.SetType();
            foreach (GraphConnection c in p.Connections) c.SetType();
            foreach (GraphPolygon pn in p.Neighbours) pn.IsNextToWater = pn.Neighbours.Any(x => x.IsWater);
        }
    }
    private static void TurnConnectionToRiver(GraphConnection c, GraphPath river, float width)
    {
        c.River = river;
        c.StartNode.River = river;
        c.EndNode.River = river;
        c.RiverWidth = width;
        foreach (GraphPolygon p in c.Polygons) p.FindRivers();
    }
    private static void TurnRiverToLand(GraphConnection c)
    {
        c.River = null;
        c.StartNode.River = null;
        c.EndNode.River = null;
        c.RiverWidth = 0;
        foreach (GraphPolygon p in c.Polygons) p.FindRivers();
    }

}
