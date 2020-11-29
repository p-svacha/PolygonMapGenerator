using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class District
    {
        public string Name;
        public Region Region;

        public List<GeographyTrait> Geography = new List<GeographyTrait>();
        public Language Language;
        public Religion Religion;
        public Density Density;
        public AgeGroup AgeGroup;
        public EconomyTrait Economy1;
        public EconomyTrait Economy2;
        public EconomyTrait Economy3;
        public List<Mentality> Mentalities = new List<Mentality>();

        public ElectionResult LastElectionResult;
        public Party CurrentWinnerParty;
        public float PlayerPartyShare;
        public float CurrentWinnerShare;
        public float CurrentMargin;

        public const int MinSeats = 3;
        public const int RequiredPopulationPerSeat = 48000;
        public const int RequirementIncreasePerSeat = 10000; // After each seat, the district needs this amount more population for the next seat

        public int Population;
        public int Seats;
        public int Voters;

        public int BasePoints = 20;
        public int PointsPerGeographyPolicy = 5;
        public int PointsPerEconomy1Policy = 7;
        public int PointsPerEconomy2Policy = 5;
        public int PointsPerEconomy3Policy = 3;
        public int PointsPerDensityPolicy = 5;
        public int PointsPerAgeGroupPolicy = 5;
        public int PointsPerLanguagePolicy = 5;
        public int PointsPerReligionPolicy = 5;

        #region Initialization

        public District(Region r, Density density, AgeGroup ageGroup, Language language, Religion religion)
        {
            Region = r;
            Name = MarkovChainWordGenerator.GetRandomName(11);

            SetGeographyTraits();

            Density = density;
            AgeGroup = ageGroup;
            Language = language;
            Religion = religion;
            
            Economy1 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy2 == Economy1) Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy3 == Economy2 || Economy3 == Economy1) Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();

            while(Mentalities.Count < 3)
            {
                Mentality c = ElectionTacticsGame.GetRandomCulture();
                if (!Mentalities.Contains(c)) Mentalities.Add(c);
            }

            // Population calculation
            Population = (int)(Region.Area * 1000000);
            if (Density == Density.Urban) Population = (int)(Population * 1.4f);
            if (Density == Density.Rural) Population = (int)(Population * 0.6f);
            Population = (Population / 1000) * 1000;

            // Seat calculation
            int tmpPop = Population;
            int tmpSeatRequirement = RequiredPopulationPerSeat;
            int tmpSeats = 0;
            while(tmpPop >= tmpSeatRequirement)
            {
                tmpSeats++;
                tmpPop -= tmpSeatRequirement;
                tmpSeatRequirement += RequirementIncreasePerSeat;
            }
            Seats = Mathf.Max(MinSeats, tmpSeats);

            // Voter calculation
            if (Mentalities.Contains(Mentality.Predictable)) Voters = Random.Range(400, 500);
            else if (Mentalities.Contains(Mentality.Unpredictable)) Voters = Random.Range(100, 150);
            else Voters = Random.Range(200, 300);
        }

        private void SetGeographyTraits()
        {
            if (Region.CoastRatio > 0.8f) Geography.Add(GeographyTrait.Peninsula);
            if (Region.CoastRatio > 0.3f) Geography.Add(GeographyTrait.Coastal);
            if (Region.CoastLength == 0f) Geography.Add(GeographyTrait.Landlocked);
            if (Region.Area < 0.18f) Geography.Add(GeographyTrait.Tiny);
            if (Region.Landmass.Size < 5) Geography.Add(GeographyTrait.Island);
        }

        #endregion

        #region Election

        /// <summary>
        /// This function calculates the election results of an election between the given parties.
        /// A specified amount of single voters will vote, whereas their vote will be decided by weighted random based on party points.
        /// Each party has x base points. On top of that points are added for policies that match the district traits, modifiers and mentality.
        /// There will always be a single winner party.
        /// </summary>
        public ElectionResult RunElection(Party playerParty, List<Party> parties)
        {
            Dictionary<Party, float> voterShares = new Dictionary<Party, float>();
            Dictionary<Party, int> partyPoints = new Dictionary<Party, int>();
            Dictionary<Party, int> partyVotes = new Dictionary<Party, int>();
            foreach (Party p in parties)
            {
                partyPoints.Add(p, GetPartyPointsFor(p));
                partyVotes.Add(p, 0);
            }
            for(int i = 0; i < Voters; i++)
            {
                Party votedParty = GetSingleVoterResult(partyPoints);
                partyVotes[votedParty]++;
            }
            foreach(Party p in parties)
            {
                //Debug.Log(p.Name + " got " + partyVotes[p] + " votes.");
                voterShares.Add(p, 100f * partyVotes[p] / Voters);
            }

            // Guarantee that there is only one winner
            List<Party> winnerParties = voterShares.Where(x => x.Value == voterShares.Values.Max(v => v)).Select(x => x.Key).ToList();
            if (winnerParties.Count > 1)
            {
                Party singleWinnerParty = winnerParties[Random.Range(0, winnerParties.Count)];
                voterShares[singleWinnerParty] += 0.1f;
            }

            ElectionResult result = new ElectionResult()
            {
                District = this,
                VoteShare = voterShares
            };

            LastElectionResult = result;
            CurrentWinnerParty = LastElectionResult.VoteShare.First(x => x.Value == LastElectionResult.VoteShare.Max(y => y.Value)).Key;
            CurrentWinnerShare = LastElectionResult.VoteShare.First(x => x.Value == LastElectionResult.VoteShare.Max(y => y.Value)).Value;
            PlayerPartyShare = LastElectionResult.VoteShare.First(x => x.Key == playerParty).Value;
            if (CurrentWinnerParty == playerParty)
            {
                float secondHighest = LastElectionResult.VoteShare.Values.OrderByDescending(x => x).ToList()[1];
                CurrentMargin = CurrentWinnerShare - secondHighest;
            }
            else CurrentMargin = PlayerPartyShare - CurrentWinnerShare;
            return result;
        }

        private int GetPartyPointsFor(Party p)
        {
            int points = BasePoints;

            foreach(GeographyTrait t in Geography)
            {
                points += p.GetPolicyValueFor(t) * PointsPerGeographyPolicy;
            }
            points += p.GetPolicyValueFor(Economy1) * PointsPerEconomy1Policy;
            points += p.GetPolicyValueFor(Economy2) * PointsPerEconomy2Policy;
            points += p.GetPolicyValueFor(Economy3) * PointsPerEconomy3Policy;
            points += p.GetPolicyValueFor(Density) * PointsPerDensityPolicy;
            points += p.GetPolicyValueFor(AgeGroup) * PointsPerAgeGroupPolicy;
            points += p.GetPolicyValueFor(Language) * PointsPerLanguagePolicy;
            points += p.GetPolicyValueFor(Religion) * PointsPerReligionPolicy;

            return points;
        }

        private Party GetSingleVoterResult(Dictionary<Party, int> partyPoints)
        {
            int sum = partyPoints.Values.Sum(x => x);
            int rng = Random.Range(0, sum);
            int tmpSum = 0;
            foreach(KeyValuePair<Party, int> kvp in partyPoints)
            {
                tmpSum += kvp.Value;
                if (rng < tmpSum) return kvp.Key;
            }
            return null;
        }

        #endregion
    }
}
