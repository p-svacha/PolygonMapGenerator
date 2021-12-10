using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MapGeneration
{
    public class GraphPolygon
    {
        public static int idCounter = 0;
        public int Id;

        public List<GraphNode> Nodes = new List<GraphNode>();
        public List<GraphConnection> Connections = new List<GraphConnection>();

        public List<GraphPolygon> AdjacentPolygons = new List<GraphPolygon>();
        public List<GraphPolygon> Neighbours = new List<GraphPolygon>();
        public List<GraphPolygon> LandNeighbours = new List<GraphPolygon>();
        public List<GraphPolygon> WaterNeighbours = new List<GraphPolygon>();

        public List<GraphPolygon> Landmass;
        public List<GraphPolygon> Continent = new List<GraphPolygon>();
        public List<GraphPath> Rivers = new List<GraphPath>();


        // Attributes
        public bool IsOuterPolygon;
        public bool IsEdgePolygon;

        public float Width;
        public float Height;
        public float Area;
        public float Jaggedness; // How close the shape is to a perfect rectangle

        public bool IsWater;
        public bool IsNextToWater;
        public bool HasRiver;
        public int DistanceFromNearestWater;

        public int Temperature;
        public int Precipitation;
        public Biome Biome;

        public Vector2 CenterPoi; // Point of inaccessability - this is the point with the biggest distance to any edge
        public Vector2 Centroid; // Average point of all border points

        public Region Region;

        public GraphPolygon(List<GraphNode> nodes, List<GraphConnection> connections)
        {
            Id = idCounter++;
            Nodes = nodes;
            Connections = connections;
            Init();
        }
        public GraphPolygon(int id, List<GraphNode> nodes, List<GraphConnection> connections)
        {
            Id = id;
            Nodes = nodes;
            Connections = connections;
            Init();
        }

        private void Init()
        {
            Area = GeometryFunctions.GetPolygonArea(Nodes.Select(x => x.Vertex).ToList());

            IsEdgePolygon = Nodes.Any(x => x.Type == BorderPointType.Edge);

            Width = Nodes.Max(x => x.Vertex.x) - Nodes.Min(x => x.Vertex.x);
            Height = Nodes.Max(x => x.Vertex.y) - Nodes.Min(x => x.Vertex.y);
            Jaggedness = 1f - (Area / (Width * Height));

            Centroid = new Vector2(Nodes.Average(x => x.Vertex.x), Nodes.Average(x => x.Vertex.y));
            float[] poi = PolygonCenterFinder.GetPolyLabel(ConvertPolygonToFloatArray(), precision: 0.05f);
            CenterPoi = new Vector2(poi[0], poi[1]);
        }

        public void AddAdjacentPolygon(GraphPolygon adj)
        {
            if (!AdjacentPolygons.Contains(adj))
            {
                AdjacentPolygons.Add(adj);
                UpdateNeighbours();
            }
        }

        public void AddWaterNeighbour(GraphPolygon wn)
        {
            if (!WaterNeighbours.Contains(wn))
            {
                WaterNeighbours.Add(wn);
                UpdateNeighbours();
            }
        }

        public void UpdateNeighbours()
        {
            LandNeighbours = AdjacentPolygons.Where(x => !x.IsWater).ToList();
            Neighbours = LandNeighbours.Concat(WaterNeighbours).ToList();
        }

        public void FindRivers()
        {
            HasRiver = Connections.Any(x => x.River != null);
        }

        public bool IsNextToLand()
        {
            return AdjacentPolygons.Any(x => !x.IsWater);
        }

        /// <summary>
        /// Returns true if two polygons have the same nodes.
        /// </summary>
        public bool HasSameNodesAs(GraphPolygon otherPoly)
        {
            int[] polyNodeIds = Nodes.OrderBy(x => x.Id).Select(x => x.Id).ToArray();
            int[] otherPolyNodeIds = otherPoly.Nodes.OrderBy(x => x.Id).Select(x => x.Id).ToArray();
            return (Enumerable.SequenceEqual(polyNodeIds, otherPolyNodeIds));
        }

        public float TotalBorderLength
        {
            get
            {
                return Connections.Where(x => x.Type != BorderType.Edge).Sum(x => Vector2.Distance(x.StartNode.Vertex, x.EndNode.Vertex));
            }
        }
        public float InlandBorderLength
        {
            get
            {
                return Connections.Where(x => x.Type == BorderType.Inland).Sum(x => Vector2.Distance(x.StartNode.Vertex, x.EndNode.Vertex));
            }
        }
        public float Coastline
        {
            get
            {
                return Connections.Where(x => x.Type == BorderType.Shore).Sum(x => Vector2.Distance(x.StartNode.Vertex, x.EndNode.Vertex));
            }
        }

        private float[][][] ConvertPolygonToFloatArray()
        {
            // this is a float[][]

            var polygon = new float[1][][];
            polygon[0] = new float[Nodes.Count][];

            var pointCount = 0;
            foreach (Vector2 point in Nodes.Select(x => x.Vertex))
            {
                polygon[0][pointCount] = new float[2];
                polygon[0][pointCount][0] = point.x;
                polygon[0][pointCount][1] = point.y;
                pointCount++;
            }

            return polygon;
        }
    }
}
