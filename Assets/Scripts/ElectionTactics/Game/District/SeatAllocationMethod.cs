using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ElectionTactics
{
    public abstract class SeatAllocationMethodDef : Def
    {
        /// <summary>
        /// Calculates and returns the amount of seats each party gets.
        /// </summary>
        public abstract Dictionary<Party, int> AllocateSeats(int numSeats, Dictionary<Party, float> voterShares);

        /// <summary>
        /// How likely the method is to appear in a district (relative to each other).
        /// </summary>
        public int Commonness { get; init; }
    }

    public class SAM_WinnerTakesAll : SeatAllocationMethodDef
    {
        public override Dictionary<Party, int> AllocateSeats(int numSeats, Dictionary<Party, float> voterShares)
        {
            Dictionary<Party, int> seatsWon = new Dictionary<Party, int>();
            Party winner = voterShares.OrderByDescending(x => x.Value).First().Key;

            foreach (Party p in voterShares.Keys)
            {
                if (p == winner) seatsWon[p] = numSeats;
                else seatsWon[p] = 0;
            }

            return seatsWon;
        }
    }

    public class SAM_HamiltonPR : SeatAllocationMethodDef
    {
        public override Dictionary<Party, int> AllocateSeats(int numSeats, Dictionary<Party, float> voterShares)
        {
            Dictionary<Party, int> seatsWon = new Dictionary<Party, int>();
            foreach (Party p in voterShares.Keys) seatsWon[p] = 0;

            float totalShare = voterShares.Values.Sum();
            if (totalShare <= 0)
            {
                // Fallback: give all seats to highest share to avoid division by zero
                Party fallbackWinner = voterShares.OrderByDescending(x => x.Value).First().Key;
                seatsWon[fallbackWinner] = numSeats;
                return seatsWon;
            }

            // Calculate exact quota for each party and assign the floor
            Dictionary<Party, float> remainders = new Dictionary<Party, float>();
            int seatsAssigned = 0;
            foreach (Party p in voterShares.Keys)
            {
                float exactSeats = (voterShares[p] / totalShare) * numSeats;
                int floorSeats = Mathf.FloorToInt(exactSeats);
                seatsWon[p] = floorSeats;
                remainders[p] = exactSeats - floorSeats;
                seatsAssigned += floorSeats;
            }

            // Distribute leftover seats to the largest remainders
            int remainingSeats = numSeats - seatsAssigned;
            foreach (Party p in remainders.OrderByDescending(x => x.Value).Select(x => x.Key))
            {
                if (remainingSeats <= 0) break;
                seatsWon[p]++;
                remainingSeats--;
            }

            return seatsWon;
        }
    }

    public class SAM_DHondtPR : SeatAllocationMethodDef
    {
        public override Dictionary<Party, int> AllocateSeats(int numSeats, Dictionary<Party, float> voterShares)
        {
            Dictionary<Party, int> seatsWon = new Dictionary<Party, int>();
            foreach (Party p in voterShares.Keys) seatsWon[p] = 0;

            // Award each seat one at a time to the party with the highest quotient (votes / (seatsWon + 1))
            for (int i = 0; i < numSeats; i++)
            {
                Party best = null;
                float bestQuotient = -1f;
                foreach (Party p in voterShares.Keys)
                {
                    float quotient = voterShares[p] / (seatsWon[p] + 1);
                    if (quotient > bestQuotient)
                    {
                        bestQuotient = quotient;
                        best = p;
                    }
                }
                seatsWon[best]++;
            }

            return seatsWon;
        }
    }

    public static class SeatAllocationMethodDefs
    {
        public static List<SeatAllocationMethodDef> Defs => new List<SeatAllocationMethodDef>()
        {
            new SAM_WinnerTakesAll()
            {
                DefName = "WinnerTakesAll",
                Label = "Winner Takes All",
                Description = "The party with the most votes receives every seat.",
                Commonness = 65,
            },

            new SAM_HamiltonPR()
            {
                DefName = "HamiltonPR",
                Label = "Proportional Representation",
                Description = "Seats are distributed proportionally to each party's vote share.",
                Commonness = 25,
            },

            new SAM_DHondtPR()
            {
                DefName = "DHondtPR",
                Label = "Majority Representation",
                Description = "Seats are distributed proportionally to each party's vote share, with an advantage for larger parties.",
                Commonness = 10,
            }
        };
    }

    [DefOf]
    public static class SeatAllocationMethodDefOf
    {
        public static SAM_WinnerTakesAll WinnerTakesAll;
        public static SAM_HamiltonPR HamiltonPR;
        public static SAM_DHondtPR DHondtPR;
    }
}
