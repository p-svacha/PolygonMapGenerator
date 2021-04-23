using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration.ContinentCreation
{
    public class KargerGraphVertex
    {
        public List<GraphPolygon> ContainedPolygons = new List<GraphPolygon>();
        public List<KargerGraphEdge> Edges = new List<KargerGraphEdge>();

        public KargerGraphVertex(GraphPolygon poly)
        {
            ContainedPolygons.Add(poly);
        }
    }
}
