using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    [System.Serializable]
    public class DistrictElectionResult
    {
        public int ElectionCycle; // Cycle of the election
        public int Year;          // Year of the election
        public int Seats;         // How many seats the district was worth at the time of the election

        public int DistrictId;
        [System.NonSerialized] public District District; // Reference to the district

        public Dictionary<int, int> PartyPointsById = new Dictionary<int, int>();
        [System.NonSerialized] public Dictionary<Party, int> PartyPoints = new Dictionary<Party, int>();

        public Dictionary<int, int> VotesById = new Dictionary<int, int>();
        [System.NonSerialized] public Dictionary<Party, int> Votes = new Dictionary<Party, int>();

        public Dictionary<int, float> VoteShareById = new Dictionary<int, float>();
        [System.NonSerialized] public Dictionary<Party, float> VoteShare = new Dictionary<Party, float>();

        public int WinnerPartyId;
        [System.NonSerialized] public Party Winner;

        public List<Modifier> Modifiers = new List<Modifier>();

        public DistrictElectionResult(int electionCycle, int year, int seats, District district, Dictionary<Party, int> partyPoints, Dictionary<Party, int> votes, Dictionary<Party, float> voteShare, Party winner, List<Modifier> modifiers)
        {
            ElectionCycle = electionCycle;
            Year = year;
            Seats = seats;
            District = district;
            DistrictId = district.Region.Id;
            PartyPoints = partyPoints;
            PartyPointsById = partyPoints.ToDictionary(x => x.Key.Id, x => x.Value);
            Votes = votes;
            VotesById = votes.ToDictionary(x => x.Key.Id, x => x.Value);
            VoteShare = voteShare;
            VoteShareById = voteShare.ToDictionary(x => x.Key.Id, x => x.Value);
            Winner = winner;
            WinnerPartyId = winner.Id;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Needs to be called when deserialized to get the references of non-serialized fields
        /// </summary>
        public void InitReferences(ElectionTacticsGame game)
        {
            District = game.Districts.First(x => x.Key.Id == DistrictId).Value;
            PartyPoints = PartyPointsById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            Votes = VotesById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            VoteShare = VoteShareById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            Winner = game.Parties.First(x => x.Id == WinnerPartyId);
        }

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

        /// <summary>
        /// Applies the district result to the game, meaning that it will get added to the districts history and victory points will be awarded.
        /// </summary>
        public void Apply(ElectionTacticsGame game)
        {
            // Add result to district
            District.ElectionResults.Add(this);
            District.CurrentWinnerParty = VoteShare.First(x => x.Value == VoteShare.Max(y => y.Value)).Key;
            District.CurrentWinnerShare = VoteShare.First(x => x.Value == VoteShare.Max(y => y.Value)).Value;

            // Award victory points
            Winner.Seats += Seats;
            Winner.TotalSeatsWon += Seats;
            Winner.TotalDistrictsWon++;
            foreach (Party p in game.Parties) p.TotalVotes += Votes[p];
        }
    }
}
