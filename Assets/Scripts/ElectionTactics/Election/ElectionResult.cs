using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ElectionResult
    {
        public int ElectionCycle;
        public int Year;
        public District District;
        public Dictionary<Party, float> VoteShare = new Dictionary<Party, float>();
        public List<Modifier> Modifiers = new List<Modifier>();
    }
}
