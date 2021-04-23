using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration.ContinentCreation
{
    public class KargerGraphEdge
    {
        public KargerGraphVertex FromVertex;
        public KargerGraphVertex ToVertex;

        public KargerGraphEdge(KargerGraphVertex fromVertex, KargerGraphVertex toVertex)
        {
            FromVertex = fromVertex;
            ToVertex = toVertex;
        }
    }
}
