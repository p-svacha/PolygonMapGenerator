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
        public List<Mentality> Mentality = new List<Mentality>();

        public ElectionResult LastElectionResult;

        public const int PopulationPerSeat = 48000;

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

        public District(Region r)
        {
            Region = r;
            Name = MarkovChainWordGenerator.GetRandomName(10);
            SetGeographyTraits();
            Language = ElectionTacticsGame.GetRandomLanguage();
            Religion = ElectionTacticsGame.GetRandomReligion();
            Density = ElectionTacticsGame.GetRandomDensity();
            AgeGroup = ElectionTacticsGame.GetRandomAgeGroup();
            Economy1 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy2 == Economy1) Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy3 == Economy2 || Economy3 == Economy1) Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Mentality.Count < 3)
            {
                Mentality c = ElectionTacticsGame.GetRandomCulture();
                if (!Mentality.Contains(c)) Mentality.Add(c);
            }

            Population = (int)(Region.Area * 1000000);
            if (Density == Density.Urban) Population = (int)(Population * 1.4f);
            if (Density == Density.Rural) Population = (int)(Population * 0.6f);
            Population = (Population / 1000) * 1000;
            Seats = Population / PopulationPerSeat;
            Voters = Random.Range(101, 200);
        }

        private void SetGeographyTraits()
        {
            if (Region.IsNextToWater) Geography.Add(GeographyTrait.Coastal);
            else Geography.Add(GeographyTrait.Landlocked);
            if (Region.Area < 0.18f) Geography.Add(GeographyTrait.Tiny);
            if (Region.Area > 1f) Geography.Add(GeographyTrait.Vast);
        }

        #endregion

        #region Election

        /// <summary>
        /// This function calculates the election results of an election between the given parties.
        /// A specified amount of single voters will vote, whereas their vote will be decided by weighted random based on party points.
        /// Each party has x base points. On top of that points are added for policies that match the district traits, modifiers and mentality.
        /// </summary>
        public ElectionResult RunElection(List<Party> parties)
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

            ElectionResult result = new ElectionResult()
            {
                District = this,
                VoteShare = voterShares
            };

            LastElectionResult = result;
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
