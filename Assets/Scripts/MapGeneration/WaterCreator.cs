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

    private static float MIN_TOTAL_LAND_COVERAGE = 0.5f;

    public static void CreateWaters(PolygonMapGenerator PMG)
    {
        switch(PMG.GenerationSettings.MapType)
        {
            case MapType.Regional:
                PerformContinentCuts(PMG);
                TurnRandomPolygonsToWater(PMG);
                ExpandOceans(PMG);
                CreateBallOceans(PMG);
                ExpandLand(PMG);
                break;

            case MapType.Island:
                CreateOuterOcean(PMG);
                TurnEdgePolygonsToWater(PMG);
                PerformContinentCuts(PMG);
                TurnRandomPolygonsToWater(PMG);
                ExpandOceans(PMG);
                CreateBallOceans(PMG);
                ExpandLand(PMG);
                break;

            case MapType.FractalNoise:
                CreateOuterOcean(PMG);
                TurnEdgePolygonsToWater(PMG);
                CreateContinentsWithNoise(PMG);
                break;

            case MapType.BigOceans:
                CreateOuterOcean(PMG);
                TurnEdgePolygonsToWater(PMG);
                TurnBigPolygonsToWater(PMG);
                break;
        }

        IdentifyLandmasses(PMG);
        IdentifyWaterBodies(PMG);

        foreach (GraphPolygon p in PMG.Polygons) p.UpdateNeighbours();
    }

    private static void CreateOuterOcean(PolygonMapGenerator PMG)
    {
        // Create outer edge nodes
        GraphNode oc1 = new GraphNode(new Vector2(-OUTER_OCEAN_SIZE, -OUTER_OCEAN_SIZE), PMG);
        GraphNode oc2 = new GraphNode(new Vector2(PMG.GenerationSettings.Width + OUTER_OCEAN_SIZE, -OUTER_OCEAN_SIZE), PMG);
        GraphNode oc3 = new GraphNode(new Vector2(PMG.GenerationSettings.Width + OUTER_OCEAN_SIZE, PMG.GenerationSettings.Height + OUTER_OCEAN_SIZE), PMG);
        GraphNode oc4 = new GraphNode(new Vector2(-OUTER_OCEAN_SIZE, PMG.GenerationSettings.Height + OUTER_OCEAN_SIZE), PMG);

        // Adding new water polygons
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[0], oc1, oc2, PMG.CornerNodes[1] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[1], oc2, oc3, PMG.CornerNodes[2] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[2], oc3, oc4, PMG.CornerNodes[3] }, new List<GraphConnection>(), outerPolygon: true);
        PMG.AddPolygon(new List<GraphNode>() { PMG.CornerNodes[3], oc4, oc1, PMG.CornerNodes[0] }, new List<GraphConnection>(), outerPolygon: true);
    }

    private static void TurnEdgePolygonsToWater(PolygonMapGenerator PMG)
    {
        foreach (GraphPolygon p in PMG.Polygons.Where(x => (x.IsEdgePolygon || x.AdjacentPolygons.Any(y => y.IsEdgePolygon)))) TurnPolygonToWater(p);
    }

    private static void CreateBallOceans(PolygonMapGenerator PMG)
    {
        int sizePerBallOcean = UnityEngine.Random.Range(SIZE_PER_BALL_OCEAN - SIZE_PER_BALL_OCEAN_RANGE, SIZE_PER_BALL_OCEAN + SIZE_PER_BALL_OCEAN_RANGE);
        int numBallOceans = Math.Max(PMG.GenerationSettings.Area / sizePerBallOcean, MIN_BALL_OCEANS);
        Debug.Log("Creating " + numBallOceans + " round oceans.");
        for (int i = 0; i < numBallOceans; i++) DoCreateBallOcean(PMG);
    }
    private static void DoCreateBallOcean(PolygonMapGenerator PMG)
    {

        int avgMapEdgeSize = (int)(Math.Sqrt(PMG.GenerationSettings.Area));

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
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, PMG.GenerationSettings.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, maxInnerDistance);
                break;
            case 1: // Right side
                circleCenterX = UnityEngine.Random.Range(PMG.GenerationSettings.Width - maxInnerDistance, PMG.GenerationSettings.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, PMG.GenerationSettings.Height + maxOuterDistance);
                break;
            case 2: // Top side
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, PMG.GenerationSettings.Width + maxOuterDistance);
                circleCenterY = UnityEngine.Random.Range(PMG.GenerationSettings.Height - maxInnerDistance, PMG.GenerationSettings.Height + maxOuterDistance);
                break;
            case 3: // Left side
                circleCenterX = UnityEngine.Random.Range(-maxOuterDistance, maxInnerDistance);
                circleCenterY = UnityEngine.Random.Range(-maxOuterDistance, PMG.GenerationSettings.Height + maxOuterDistance);
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

    private static void PerformContinentCuts(PolygonMapGenerator PMG)
    {
        int sizePerContinentCuts = UnityEngine.Random.Range(SIZE_PER_CONTINENT_CUT - SIZE_PER_CONTINENT_CUT_RANGE, SIZE_PER_CONTINENT_CUT + SIZE_PER_CONTINENT_CUT_RANGE);
        int numContinentCuts = Math.Max(PMG.GenerationSettings.Area / sizePerContinentCuts, MIN_CONTINENT_CUTS);
        Debug.Log("Performing " + numContinentCuts + " continent cuts.");
        for (int i = 0; i < numContinentCuts; i++) DoCreateContinentCut(PMG);
    }
    public static void DoCreateContinentCut(PolygonMapGenerator PMG)
    {
        // Define random cut circle
        int avgMapEdgeSize = (int)(Math.Sqrt(PMG.GenerationSettings.Area));
        float minDistanceFromMap = avgMapEdgeSize * 0.1f;
        float maxDistanceFromMap = avgMapEdgeSize * 0.7f;

        float cutCircleCenterX = 0, cutCircleCenterY = 0;
        switch(UnityEngine.Random.Range(0,4))
        {
            case 0: // Bottom side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, PMG.GenerationSettings.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-maxDistanceFromMap, -minDistanceFromMap);
                break;
            case 1: // Right side
                cutCircleCenterX = UnityEngine.Random.Range(PMG.GenerationSettings.Width + minDistanceFromMap, PMG.GenerationSettings.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-minDistanceFromMap, PMG.GenerationSettings.Height + maxDistanceFromMap);
                break;
            case 2: // Top side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, PMG.GenerationSettings.Width + maxDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(PMG.GenerationSettings.Height + minDistanceFromMap, PMG.GenerationSettings.Height + maxDistanceFromMap);
                break;
            case 3: // Left side
                cutCircleCenterX = UnityEngine.Random.Range(-maxDistanceFromMap, -minDistanceFromMap);
                cutCircleCenterY = UnityEngine.Random.Range(-minDistanceFromMap, PMG.GenerationSettings.Height + maxDistanceFromMap);
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
        if (shorePolygons.Count == 0) return;
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
        if (landPolygons.Count == 0) return;
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
        List<GraphPolygon> waterShorePolygons = PMG.Polygons.Where(x => !x.IsEdgePolygon && !x.AdjacentPolygons.Any(y => y.IsEdgePolygon) && x.IsWater && x.IsNextToLand()).ToList();

        // When there is no land yet, just convert a random polygon to land
        if (waterShorePolygons.Count == 0) waterShorePolygons = PMG.Polygons.Where(x => !x.IsEdgePolygon).ToList();

        GraphPolygon newLand = waterShorePolygons[UnityEngine.Random.Range(0, waterShorePolygons.Count)];
        TurnPolygonToLand(newLand);
    }

    private static void TurnPolygonToWater(GraphPolygon p)
    {
        if (!p.IsWater)
        {
            p.IsWater = true;
            foreach (GraphNode n in p.Nodes) n.SetType();
            foreach (GraphConnection c in p.Connections) c.SetType();
            foreach (GraphPolygon pn in p.AdjacentPolygons) pn.IsNextToWater = true;
        }
    }
    private static void TurnPolygonToLand(GraphPolygon p)
    {
        if (p.IsWater)
        {
            p.IsWater = false;
            foreach (GraphNode n in p.Nodes) n.SetType();
            foreach (GraphConnection c in p.Connections) c.SetType();
            foreach (GraphPolygon pn in p.AdjacentPolygons) pn.IsNextToWater = pn.AdjacentPolygons.Any(x => x.IsWater);
        }
    }

    private static void CreateContinentsWithNoise(PolygonMapGenerator PMG)
    {
        ContinentNoise noise = new ContinentNoise((1f / PMG.GenerationSettings.ContinentSizeScaleFactor) * (100f / PMG.GenerationSettings.Area));

        foreach (GraphPolygon polygon in PMG.Polygons.Where(x => (!x.IsEdgePolygon && !x.AdjacentPolygons.Any(y => y.IsEdgePolygon))))
        {
            float noiseValue = noise.GetValue(polygon.CenterPoi.x, polygon.CenterPoi.y, PMG.GenerationSettings);
            if (noiseValue < 0.2f) TurnPolygonToWater(polygon);
        }
    }

    /// <summary>
    /// Turns polygons that are bigger than the max allowed area into water
    /// </summary>
    private static void TurnBigPolygonsToWater(PolygonMapGenerator PMG)
    {
        foreach(GraphPolygon polygon in PMG.Polygons)
        {
            if (polygon.Area > PMG.GenerationSettings.MaxPolygonArea) TurnPolygonToWater(polygon);
        }
    }

    #region Landmass and Waterbody identification

    private static void IdentifyLandmasses(PolygonMapGenerator PMG)
    {
        // Identify landmasses
        PMG.Landmasses = new List<List<GraphPolygon>>();

        List<GraphPolygon> polygonsWithoutLandmass = new List<GraphPolygon>();
        polygonsWithoutLandmass.AddRange(PMG.Polygons.Where(x => !x.IsWater));

        while (polygonsWithoutLandmass.Count > 0)
        {
            List<GraphPolygon> landmassPolygons = new List<GraphPolygon>();
            Queue<GraphPolygon> polygonsToAdd = new Queue<GraphPolygon>();
            polygonsToAdd.Enqueue(polygonsWithoutLandmass[0]);
            while (polygonsToAdd.Count > 0)
            {
                GraphPolygon polygonToAdd = polygonsToAdd.Dequeue();
                landmassPolygons.Add(polygonToAdd);
                foreach (GraphPolygon neighbourPolygon in polygonToAdd.AdjacentPolygons.Where(x => !x.IsWater))
                    if (!landmassPolygons.Contains(neighbourPolygon) && !polygonsToAdd.Contains(neighbourPolygon))
                        polygonsToAdd.Enqueue(neighbourPolygon);
            }
            PMG.Landmasses.Add(landmassPolygons);
            foreach (GraphPolygon poly in landmassPolygons)
            {
                polygonsWithoutLandmass.Remove(poly);
            }
        }

        foreach(List<GraphPolygon> landmass in PMG.Landmasses)
        {
            foreach(GraphPolygon polygon in landmass)
            {
                polygon.Landmass = landmass;
            }
        }
    }

    private static void IdentifyWaterBodies(PolygonMapGenerator PMG)
    {
        PMG.WaterBodies = new List<List<GraphPolygon>>();

        List<GraphPolygon> polygonsWithoutWaterBody = new List<GraphPolygon>();
        polygonsWithoutWaterBody.AddRange(PMG.Polygons.Where(x => x.IsWater && !x.IsOuterPolygon));

        while (polygonsWithoutWaterBody.Count > 0)
        {
            List<GraphPolygon> waterBodyPolygons = new List<GraphPolygon>();
            Queue<GraphPolygon> polygonsToAdd = new Queue<GraphPolygon>();
            polygonsToAdd.Enqueue(polygonsWithoutWaterBody[0]);
            while (polygonsToAdd.Count > 0)
            {
                GraphPolygon polygonToAdd = polygonsToAdd.Dequeue();
                waterBodyPolygons.Add(polygonToAdd);
                foreach (GraphPolygon neighbourPolygon in polygonToAdd.AdjacentPolygons.Where(x => x.IsWater && !x.IsOuterPolygon))
                    if (!waterBodyPolygons.Contains(neighbourPolygon) && !polygonsToAdd.Contains(neighbourPolygon))
                        polygonsToAdd.Enqueue(neighbourPolygon);
            }
            bool isLake = waterBodyPolygons.Count < 5;
            PMG.WaterBodies.Add(waterBodyPolygons);

            // Add outer ocean to ocean
            if (!isLake)
            {
                foreach (GraphPolygon poly in PMG.Polygons.Where(x => x.IsOuterPolygon))
                {
                    waterBodyPolygons.Add(poly);
                }
            }

            foreach (GraphPolygon poly in waterBodyPolygons)
            {
                polygonsWithoutWaterBody.Remove(poly);
            }
        }
    }

    #endregion
}
