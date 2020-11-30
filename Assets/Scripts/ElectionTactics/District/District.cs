using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class District
    {
        public ElectionTacticsGame Game;
        public string Name;
        public int OrderId;
        public Region Region;

        // Traits
        public List<GeographyTrait> Geography = new List<GeographyTrait>();
        public Language Language;
        public Religion Religion;
        public Density Density;
        public AgeGroup AgeGroup;
        public EconomyTrait Economy1;
        public EconomyTrait Economy2;
        public EconomyTrait Economy3;
        public List<Mentality> Mentalities = new List<Mentality>();
        public List<MentalityType> MentalityTypes = new List<MentalityType>();

        // Election
        public ElectionResult LastElectionResult;
        public Party CurrentWinnerParty;
        public float PlayerPartyShare;
        public float CurrentWinnerShare;
        public float CurrentMargin;

        public const int MinSeats = 1;
        public const int RequiredPopulationPerSeat = 40000;
        public const int RequirementIncreasePerSeat = 20000; // After each seat, the district needs this amount more population for the next seat

        public int Population;
        public int Seats;
        public int Voters;

        public int BasePoints = 20;
        public int UndecidedBasePoints = 40;
        public int DecidedBasePoints = 0;

        public int LowImpactPoints = 3;
        public int MediumImpactPoints = 5;
        public int HighImpactPoints = 7;

        // Visual
        public UI_DistrictLabel MapLabel;

        #region Initialization

        public District(ElectionTacticsGame game, Region r, Density density, AgeGroup ageGroup, Language language, Religion religion)
        {
            Game = game;
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
                Mentality m = Game.GetMentalityFor(this);
                Mentalities.Add(m);
                MentalityTypes.Add(m.Type);
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
            if (MentalityTypes.Contains(MentalityType.Predictable)) Voters = Random.Range(400, 500);
            else if (MentalityTypes.Contains(MentalityType.Unpredictable)) Voters = Random.Range(100, 150);
            else Voters = Random.Range(200, 300);
        }

        private void SetGeographyTraits()
        {
            // Coastal
            if (Region.OceanCoastRatio > 0.7f) 
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 3));
            else if (Region.OceanCoastRatio > 0.4f) 
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 2));
            else if (Region.OceanCoastRatio > 0.1f) 
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 1));

            // Landlocked
            if(Region.CoastLength == 0 && Region.LandNeighbours.All(x => x.CoastLength == 0))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 3));
            else if (Region.CoastLength == 0 && Region.LandNeighbours.Where(x => x.CoastLength == 0).Count() >= 2)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 2));
            else if (Region.CoastLength == 0)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 1));

            // Island
            if(Region.Landmass.Size == 1)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 3));
            else if(Region.Landmass.Size <= 4)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 2));
            else if(Region.Landmass.Size <= 7)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 1));

            // Tiny
            if(Region.Area <= 0.12f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 3));
            else if(Region.Area <= 0.18f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 2));
            else if(Region.Area <= 0.24f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 1));

            // Large
            if(Region.Area >= 1.4f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 3));
            else if (Region.Area >= 1.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 2));
            else if (Region.Area >= 1.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 1));

            // Northern
            if(Region.Center.y > Game.Map.Height - (Game.Map.Height * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 3));
            else if (Region.Center.y > Game.Map.Height - (Game.Map.Height * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 2));
            else if (Region.Center.y > Game.Map.Height - (Game.Map.Height * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 1));

            // Southern
            if (Region.Center.y < Game.Map.Height * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 3));
            else if (Region.Center.y < Game.Map.Height * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 2));
            else if (Region.Center.y < Game.Map.Height * 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 1));

            // Eastern
            if (Region.Center.x > Game.Map.Width - (Game.Map.Width * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 3));
            else if (Region.Center.x > Game.Map.Width - (Game.Map.Width * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 2));
            else if (Region.Center.x > Game.Map.Width - (Game.Map.Width * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 1));

            // Western
            if (Region.Center.x < Game.Map.Width * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Western, 3));
            else if (Region.Center.x < Game.Map.Width * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Western, 2));
            else if (Region.Center.x < Game.Map.Width * 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Western, 1));

            // Lakeside
            if (Region.LakeCoastRatio > 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Lakeside, 3));
            else if (Region.LakeCoastRatio > 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Lakeside, 2));
            else if (Region.LakeCoastRatio > 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Lakeside, 1));
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
            if (MentalityTypes.Contains(MentalityType.Decided)) points = DecidedBasePoints;
            if (MentalityTypes.Contains(MentalityType.Undecided)) points = UndecidedBasePoints;

            foreach (GeographyTrait t in Geography)
            {
                points += p.GetPolicyValueFor(t.Type) * GetImpactPointsFor(t);
            }

            points += p.GetPolicyValueFor(Economy1) * HighImpactPoints;
            points += p.GetPolicyValueFor(Economy2) * MediumImpactPoints;
            points += p.GetPolicyValueFor(Economy3) * LowImpactPoints;

            points += p.GetPolicyValueFor(Density) * MediumImpactPoints;

            points += p.GetPolicyValueFor(AgeGroup) * MediumImpactPoints;

            int languagePoints = p.GetPolicyValueFor(Language) * MediumImpactPoints;
            if (MentalityTypes.Contains(MentalityType.Linguistic)) languagePoints *= 2;
            if (MentalityTypes.Contains(MentalityType.Nonlinguistic)) languagePoints /= 2;
            points += languagePoints;

            int religionPoints = p.GetPolicyValueFor(Religion) * MediumImpactPoints;
            if (MentalityTypes.Contains(MentalityType.Religious)) religionPoints *= 2;
            if (MentalityTypes.Contains(MentalityType.Secular)) religionPoints /= 2;
            points += religionPoints;

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

        private int GetImpactPointsFor(GeographyTrait t)
        {
            if (t.Category == 3) return HighImpactPoints;
            if (t.Category == 2) return MediumImpactPoints;
            if (t.Category == 1) return LowImpactPoints;
            throw new System.Exception("Geography traits with a category outside 1,2,3 is not allowed.");
        }

        #endregion
    }
}
