using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration
{
    public class GraphPath
    {
        public static int idCounter = 0;
        public int Id;

        public List<GraphNode> Nodes = new List<GraphNode>();
        public List<GraphConnection> Connections = new List<GraphConnection>();
        public List<GraphPolygon> Polygons = new List<GraphPolygon>();

        public River River;

        public GraphPath()
        {
            Id = idCounter++;
        }

        public GraphPath(int id, List<GraphNode> nodes, List<GraphConnection> connections, List<GraphPolygon> polygons)
        {
            Id = id;
            Nodes = nodes;
            Connections = connections;
            Polygons = polygons;
        }
    }
}
