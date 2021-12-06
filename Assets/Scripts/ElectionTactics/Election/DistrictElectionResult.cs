using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class DistrictElectionResult
    {
        public int ElectionCycle; // Cycle of the election
        public int Year;          // Year of the election
        public int Seats;         // How many seats the district was worth at the time of the election
        public District District; // Reference to the district
        public Dictionary<Party, int> PartyPoints = new Dictionary<Party, int>();
        public Dictionary<Party, int> Votes = new Dictionary<Party, int>();
        public Dictionary<Party, float> VoteShare = new Dictionary<Party, float>();
        public Party Winner;
        public List<Modifier> Modifiers = new List<Modifier>();

        /// <summary>
        /// Returns the margin of the given party to the winner party. If the given party is the winner, then the margin to the second-placed party is returned.
        /// </summary>
        public float GetMargin(Party p)
        {
            float partyShare = VoteShare.First(x => x.Key == p).Value;
            float winnerShare = VoteShare.First(x => x.Value == VoteShare.Max(y => y.Value)).Value;
            if (Winner == p)
            {
                float secondHighest = VoteShare.Values.OrderByDescending(x => x).ToList()[1];
                return winnerShare - secondHighest;
            }
            else return partyShare - winnerShare;
        }
    }
}
