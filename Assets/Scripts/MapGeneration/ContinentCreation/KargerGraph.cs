using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration.ContinentCreation
{
    public class KargerGraph
    {
        public List<KargerGraphVertex> Vertices = new List<KargerGraphVertex>();
        public List<KargerGraphEdge> Edges = new List<KargerGraphEdge>();

        public int CutSize;
    }
}
