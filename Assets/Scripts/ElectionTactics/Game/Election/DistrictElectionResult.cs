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

        public int DistrictId;
        public District District; // Reference to the district

        public List<Party> Parties; // Parties that participated in the election

        public Dictionary<int, int> PartyPopularitiesById = new Dictionary<int, int>();
        public Dictionary<Party, int> PartyPopularities = new Dictionary<Party, int>();

        public Dictionary<int, int> VotesById = new Dictionary<int, int>();
        public Dictionary<Party, int> Votes = new Dictionary<Party, int>();

        public Dictionary<int, float> VoteShareById = new Dictionary<int, float>();
        public Dictionary<Party, float> VoteShare = new Dictionary<Party, float>();

        public Dictionary<Party, int> SeatsWon = new Dictionary<Party, int>(); // Which party has won how many seats in the election

        public int WinnerPartyId;
        public Party WinnerParty;
        public List<Party> NonWinners => PartyPopularities.Keys.Where(p => p != WinnerParty).ToList();

        public List<Modifier> Modifiers = new List<Modifier>();

        public DistrictElectionResult(int electionCycle, int year, int seats, List<Party> parties, District district, Dictionary<Party, int> partyPoints, Dictionary<Party, int> votes, Dictionary<Party, float> voteShare, Dictionary<Party, int> seatsWon, Party winner, List<Modifier> modifiers)
        {
            ElectionCycle = electionCycle;
            Year = year;
            Seats = seats;
            Parties = parties;
            District = district;
            DistrictId = district.Region.Id;
            PartyPopularities = partyPoints;
            PartyPopularitiesById = partyPoints.ToDictionary(x => x.Key.Id, x => x.Value);
            Votes = votes;
            VotesById = votes.ToDictionary(x => x.Key.Id, x => x.Value);
            VoteShare = voteShare;
            VoteShareById = voteShare.ToDictionary(x => x.Key.Id, x => x.Value);
            SeatsWon = seatsWon;
            WinnerParty = winner;
            WinnerPartyId = winner.Id;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Needs to be called when deserialized to get the references of non-serialized fields
        /// </summary>
        public void InitReferences(ElectionTacticsGame game)
        {
            District = game.Districts.First(x => x.Key.Id == DistrictId).Value;
            PartyPopularities = PartyPopularitiesById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            Votes = VotesById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            VoteShare = VoteShareById.ToDictionary(x => game.Parties.First(p => p.Id == x.Key), x => x.Value);
            WinnerParty = game.Parties.First(x => x.Id == WinnerPartyId);
        }

        /// <summary>
        /// Returns the margin of the given party to the winner party as a string. If the given party is the winner, then the margin to the second-placed party is returned.
        /// </summary>
        public string GetMargin(Party p)
        {
            float partyShare = VoteShare.First(x => x.Key == p).Value;
            float winnerShare = VoteShare.First(x => x.Value == VoteShare.Max(y => y.Value)).Value;
            if (WinnerParty == p)
            {
                float secondHighest = VoteShare.Values.OrderByDescending(x => x).ToList()[1];
                return "+" + (winnerShare - secondHighest).ToString("0.0") + "%";
            }
            else return (partyShare - winnerShare).ToString("0.0") + "%";
        }

        /// <summary>
        /// Applies the district result to the game, meaning that it will get added to the districts history and victory points will be awarded.
        /// </summary>
        public void Apply(ElectionTacticsGame game)
        {
            // Add result to district
            District.ElectionResults.Add(this);
            District.CurrentWinnerParty = WinnerParty;
            District.CurrentWinnerShare = VoteShare[WinnerParty];

            // Award victory points
            WinnerParty.TotalDistrictsWon++;
            foreach (Party p in Parties) p.Seats += SeatsWon[p];
            foreach (Party p in Parties) p.TotalSeatsWon += SeatsWon[p];
            foreach (Party p in Parties) p.TotalVotes += Votes[p];

            // Substract legitimacy for non-won seats (relevant for battle royale)
            foreach (Party p in Parties) p.Legitimacy -= (Seats - SeatsWon[p]);
        }
    }
}
