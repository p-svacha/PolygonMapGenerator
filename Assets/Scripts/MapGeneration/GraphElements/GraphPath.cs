using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPath
{
    public List<GraphNode> Nodes = new List<GraphNode>();
    public List<GraphConnection> Connections = new List<GraphConnection>();
    public List<GraphPolygon> Polygons = new List<GraphPolygon>();

    public River River;
}
