using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MapGeneration
{
    public class GraphNode
    {
        public static int idCounter = 0;
        public int Id;

        public Vector2 Vertex;

        public List<GraphNode> ConnectedNodes = new List<GraphNode>();
        public List<GraphConnection> Connections = new List<GraphConnection>();
        public List<GraphPolygon> Polygons = new List<GraphPolygon>();

        public GraphPath River;
        public float RiverWidth;

        public List<GraphNode> VisitedNeighbours = new List<GraphNode>(); // Used for performant polygon finding

        public BorderPointType Type;
        public int DistanceFromNearestOcean;

        public BorderPoint BorderPoint; // corresponding visual border point on the map

        public GraphNode(Vector2 v, PolygonMapGenerator PMG)
        {
            Id = idCounter++;
            Vertex = v;
            Type = (v.x == 0 || v.x == PMG.GenerationSettings.Width || v.y == 0 || v.y == PMG.GenerationSettings.Height) ? BorderPointType.Edge : BorderPointType.Inland;
        }
        public GraphNode(int id, float x, float y, float riverWidth, int distanceFromNearestOcean)
        {
            Id = id;
            Vertex = new Vector2(x, y);
            RiverWidth = riverWidth;
            DistanceFromNearestOcean = distanceFromNearestOcean;
        }

        public void SetType()
        {
            if (Type == BorderPointType.Edge) return;
            else if (Polygons.All(x => x.IsWater)) Type = BorderPointType.Water;
            else if (Polygons.Any(x => x.IsWater)) Type = BorderPointType.Shore;
            else Type = BorderPointType.Inland;
        }

        /// <summary>
        /// Returns a list containing all angles of connections of this node
        /// </summary>
        /// <returns></returns>
        public List<int> GetNodeAngles()
        {
            List<int> nodeAngles = new List<int>();
            foreach (GraphNode n in ConnectedNodes)
            {
                float angle = Vector2.SignedAngle(new Vector2(n.Vertex.x - Vertex.x, n.Vertex.y - Vertex.y), new Vector2(0, 1));
                nodeAngles.Add((int)angle);
            }
            return nodeAngles;
        }

        public GraphConnection GetConnectionTo(GraphNode n)
        {
            return Connections.Where(x => (x.StartNode == this && x.EndNode == n) || (x.StartNode == n && x.EndNode == this)).First();
        }

        public bool IsWaterNode()
        {
            return Polygons.All(x => x.IsWater);
        }

        public override string ToString()
        {
            return "Id: " + Id + ", " + Vertex.ToString();
        }
    }
}
