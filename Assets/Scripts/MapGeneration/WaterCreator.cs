using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public static class WaterCreator
{
    private static float OUTER_OCEAN_SIZE = 10f;

    private static int MIN_BALL_OCEANS = 1;
    private static int SIZE_PER_BALL_OCEAN = 190;
    private static int SIZE_PER_BALL_OCEAN_RANGE = 60;
    private static float OCEAN_ISLAND_CHANCE = 0.15f; // Chance that a region within an ocean doesn't get transformed

    private static int MIN_CONTINENT_CUTS = 1;
    private static int SIZE_PER_CONTINENT_CUT = 190;
    private static int SIZE_PER_CONTINENT_CUT_RANGE = 60;
    private static float MIN_CONTINENT_CUT_WIDTH = 0f;
    private static float MAX_CONTINENT_CUT_WIDTH = 1.5f;
    private static float CONTINENT_SPLIT_ISLAND_CHANCE = 0.05f;

    private static float RANDOM_OCEAN_PERC = 0.04f;
    private static float EXPAND_OCEAN_PERC = 0.08f;

    private static float MIN_TOTAL_LAND_COVERAGE = 0.52f;

    public static void CreateWaters(PolygonMapGenerator PMG)
    {
        CreateOuterOcean(PMG);
        TurnEdgePolygonsToWater(PMG);
        CreateContinents(PMG);
        TurnRandomPolygonsToWater(PMG);
        ExpandOceans(PMG);
        CreateBallOceans(PMG);
        ExpandLand(PMG);
    }

    public static void HandleInput(PolygonMapGenerator PMG)
    {
        // R - Create random water
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.R))
        {
            PMG.Map.DestroyAllGameObjects();
            DoTurnRandomPolygonToWater(PMG);
            PMG.DrawMap();
        }

        // E - Expand ocean
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.E))
        {
            PMG.Map.DestroyAllGameObjects();
            DoExpandOcean(PMG);
            PMG.DrawMap();
        }

        // C - Create continent cut
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.C))
        {
            PMG.Map.DestroyAllGameObjects();
            DoCreateContinentCut(PMG);
            PMG.DrawMap();
        }

        // O - Create ocean
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.O))
        {
            PMG.Map.DestroyAllGameObjects();
            DoCreateBallOcean(PMG);
            PMG.DrawMap();
        }

        // L - Expand land
        if (PMG.Map != null && Input.GetKeyDown(KeyCode.L))
        {
            PMG.Map.DestroyAllGameObjects();
            DoExpandLand(PMG);
            PMG.DrawMap();
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
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[0], oc1, oc2, PMG.CornerNodes[1] }, new List<GraphConnection>());
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[1], oc2, oc3, PMG.CornerNodes[2] }, new List<GraphConnection>());
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[2], oc3, oc4, PMG.CornerNodes[3] }, new List<GraphConnection>());
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[3], oc4, oc1, PMG.CornerNodes[0] }, new List<GraphConnection>());
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


    private static void TurnPolygonToWater(GraphPolygon p)
    {
        if (!p.IsWater)
        {
            p.IsWater = true;
            foreach (GraphPolygon pn in p.Neighbours) pn.IsNextToWater = true;
        }
    }
    private static void TurnPolygonToLand(GraphPolygon p)
    {
        if (p.IsWater)
        {
            p.IsWater = false;
            foreach (GraphPolygon pn in p.Neighbours)
                pn.IsNextToWater = pn.Neighbours.Any(x => x.IsWater);
        }
    }

}
