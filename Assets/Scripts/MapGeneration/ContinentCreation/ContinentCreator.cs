using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapGeneration.ContinentCreation
{
    /// <summary>
    /// The continent creator takes all land polygons from a map and partitions them into continents, whereas each region will be assigned exactly one continent
    /// </summary>
    public static class ContinentCreator
    {
        public static void CreateContinents(PolygonMapGenerator PMG)
        {
            PMG.Continents = new List<List<GraphPolygon>>();

            // 1. Create one continent per landmass
            foreach (List<GraphPolygon> landmass in PMG.Landmasses)
            {
                List<GraphPolygon> continentPolygons = new List<GraphPolygon>();
                continentPolygons.AddRange(landmass);
                foreach (GraphPolygon landmassPoly in landmass) landmassPoly.Continent = continentPolygons;
                PMG.Continents.Add(continentPolygons);
            }

            // 2. Split big landmasses into mutliple continents
            List<GraphPolygon> biggestContinent = PMG.Continents.OrderByDescending(x => x.Count).First();
            while(biggestContinent.Count > PMG.GenerationSettings.MaxContinentSize)
            {
                int minCuts = int.MaxValue;
                KargerGraph bestGraph = null;
                int cyclesWithoutImprovement = 0;
                while(cyclesWithoutImprovement < 50 || bestGraph == null)
                {
                    cyclesWithoutImprovement++;
                    KargerGraph graph = SplitClusterOnce(biggestContinent);
                    if(graph.Vertices[0].ContainedPolygons.Count >= PMG.GenerationSettings.MinContinentSize && graph.Vertices[1].ContainedPolygons.Count >= PMG.GenerationSettings.MinContinentSize && graph.CutSize < minCuts)
                    {
                        minCuts = graph.CutSize;
                        bestGraph = graph;
                        cyclesWithoutImprovement = 0;
                    }
                }

                List<GraphPolygon> newContinent = new List<GraphPolygon>();
                foreach (GraphPolygon splittedPolygon in bestGraph.Vertices[0].ContainedPolygons)
                {
                    newContinent.Add(splittedPolygon);
                    biggestContinent.Remove(splittedPolygon);
                    splittedPolygon.Continent = newContinent;
                }
                PMG.Continents.Add(newContinent);

                biggestContinent = PMG.Continents.OrderByDescending(x => x.Count).First();
            }

            
            // 3. Assign islands that are too small to the nearest continent
            List<GraphPolygon> smallestContinent = PMG.Continents.OrderBy(x => x.Count).First();
            while(smallestContinent.Count < PMG.GenerationSettings.MinContinentSize)
            {
                float shortestDistance = float.MaxValue;
                GraphPolygon shortestDistancePolygon = null;
                foreach(GraphPolygon continentPoly in smallestContinent)
                {
                    foreach(GraphPolygon waterNeighbour in continentPoly.WaterNeighbours)
                    {
                        if(!smallestContinent.Contains(waterNeighbour))
                        {
                            float distance = PolygonMapFunctions.GetPolygonDistance(continentPoly, waterNeighbour);
                            if (distance < shortestDistance)
                            {
                                shortestDistance = distance;
                                shortestDistancePolygon = waterNeighbour;
                            }
                        }
                    }
                }

                foreach (GraphPolygon continentPoly in smallestContinent)
                {
                    continentPoly.Continent = shortestDistancePolygon.Continent;
                    shortestDistancePolygon.Continent.Add(continentPoly);
                }
                smallestContinent.Clear();
                PMG.Continents.Remove(smallestContinent);

                smallestContinent = PMG.Continents.OrderBy(x => x.Count).First();
            }
            
        }

        /// <summary>
        /// Runs Karger's algorithm on a graph (that respresents a list of polygons) once, which splits a cluster of polygons into two random parts. https://en.wikipedia.org/wiki/Karger%27s_algorithm
        /// Returns two new lists with the splitted continents
        /// </summary>
        private static KargerGraph SplitClusterOnce(List<GraphPolygon> cluster)
        {
            // First we need to turn the cluster into a Karger graph (which is just a regular unweighted undirected graph), where the nodes contain information about which polygons they contain
            KargerGraph graph = new KargerGraph();
            Dictionary<GraphPolygon, KargerGraphVertex> vertices = new Dictionary<GraphPolygon, KargerGraphVertex>();
            foreach (GraphPolygon poly in cluster)
            {
                KargerGraphVertex vertex = new KargerGraphVertex(poly);
                graph.Vertices.Add(vertex);
                vertices.Add(poly, vertex);
            }
            List<GraphPolygon> visitedPolygons = new List<GraphPolygon>();
            foreach(GraphPolygon poly in cluster)
            {
                visitedPolygons.Add(poly);
                foreach(GraphPolygon neighbourPoly in poly.LandNeighbours.Where(x => cluster.Contains(x) && !visitedPolygons.Contains(x)))
                {
                    KargerGraphEdge edge = new KargerGraphEdge(vertices[poly], vertices[neighbourPoly]);
                    graph.Edges.Add(edge);
                    vertices[poly].Edges.Add(edge);
                    vertices[neighbourPoly].Edges.Add(edge);
                }
            }

            // Debug graph
            /*
            foreach(KargerGraphEdge edge in graph.Edges)
            {
                Vector2 from = edge.FromVertex.ContainedPolygons[0].CenterPoi;
                Vector2 to = edge.ToVertex.ContainedPolygons[0].CenterPoi;
                Debug.DrawLine(new Vector3(from.x, 0f, from.y), new Vector3(to.x, 0f, to.y), Color.red, 30);
            }
            */

            // Then collapse edges in the graph until only 2 vertices are left
            while(graph.Vertices.Count > 2)
            {
                KargerGraphEdge randomEdge = graph.Edges[Random.Range(0, graph.Edges.Count)];
                CollapseEdge(graph, randomEdge);
            }

            graph.CutSize = graph.Vertices[0].Edges.Count;

            return graph;
        }

        private static void CollapseEdge(KargerGraph graph, KargerGraphEdge edge)
        {
            // Remove the edge
            graph.Edges.Remove(edge);
            edge.FromVertex.Edges.Remove(edge);
            edge.ToVertex.Edges.Remove(edge);

            // Merge the two vertices of the edge
            edge.FromVertex.ContainedPolygons.AddRange(edge.ToVertex.ContainedPolygons);
            graph.Vertices.Remove(edge.ToVertex);

            foreach (KargerGraphEdge toEdge in edge.ToVertex.Edges)
            {
                if(toEdge != edge)
                {
                    edge.FromVertex.Edges.Add(toEdge);

                    if (toEdge.FromVertex == edge.ToVertex)
                    {
                        toEdge.FromVertex = edge.FromVertex;
                    }

                    else if (toEdge.ToVertex == edge.ToVertex)
                    {
                        toEdge.ToVertex = edge.FromVertex;
                    }
                }
            }

            // Remove self-loops
            List<KargerGraphEdge> edgesToRemove = new List<KargerGraphEdge>();
            foreach(KargerGraphEdge newEdge in edge.FromVertex.Edges)
            {
                if(newEdge.FromVertex == newEdge.ToVertex)
                {
                    edgesToRemove.Add(newEdge);
                    
                }
            }
            foreach(KargerGraphEdge edgeToRemove in edgesToRemove)
            {
                edge.FromVertex.Edges.Remove(edgeToRemove);
                graph.Edges.Remove(edgeToRemove);
            }
        }
    }
}
