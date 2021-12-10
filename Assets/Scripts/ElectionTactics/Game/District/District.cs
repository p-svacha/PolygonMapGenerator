﻿using System.Collections;
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
        public List<DistrictElectionResult> ElectionResults = new List<DistrictElectionResult>();
        public Party CurrentWinnerParty;
        public float CurrentWinnerShare;

        public const int MinSeats = 1;
        public const int RequiredPopulationPerSeat = 40000;
        public const int RequirementIncreasePerSeat = 20000; // After each seat, the district needs this amount more population for the next seat

        public int Population;     // How many inhabitants the district has - It can vary from 32'000 to 2'400'000
        public int Seats;          // How many seats this district has in the parliament
        public int Voters;         // How many people cast a vote (used just for calculation behind the scenes, actual voter count is based on population and voter turnout).
        public float VoterTurnout; // Value between 0 and 1 of how many people went to vote. This value is only relevant for the vote victory and is not used for calculation of the actual votes.

        public const int NumVotersUnpredictableMin = 150;
        public const int NumVotersUnpredictableMax = 200;
        public const int NumVotersDefaultMin = 450;
        public const int NumVotersDefaultMax = 550;
        public const int NumVotersPredictableMin = 1900;
        public const int NumVotersPredictableMax = 2100;

        public const float VoterTurnoutLowMin = 0.3f;
        public const float VoterTurnoutLowMax = 0.4f;
        public const float VoterTurnoutDefaultMin = 0.6f;
        public const float VoterTurnoutDefaultMax = 0.7f;
        public const float VoterTurnoutHighMin = 0.9f;
        public const float VoterTurnoutHighMax = 1f;

        public const int BasePopularity = 20;
        public const int UndecidedBasePopularity = 40;
        public const int DecidedBasePopularity = 5;

        public const int LowImpactPopularity = 3;
        public const int MediumImpactPopularity = 5;
        public const int HighImpactPopularity = 7;

        public const int PositiveModifierImpact = 30;
        public const int NegativeModifierImpact = 30;

        public List<Modifier> Modifiers = new List<Modifier>();

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
            while (Economy2 == Economy1) Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while (Economy3 == Economy2 || Economy3 == Economy1) Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();

            int numMentalities = Random.Range(1, 4);
            while (Mentalities.Count < numMentalities)
            {
                Mentality m = Game.GetMentalityFor(this);
                Mentalities.Add(m);
                MentalityTypes.Add(m.Type);
            }

            // Population calculation
            Population = (int)(Region.Area * 1000000);
            float populationModifier = 0f;
            if (Density == Density.Urban) populationModifier = Random.Range(1.2f, 1.6f);
            if (Density == Density.Mixed) populationModifier = Random.Range(0.8f, 1.2f);
            if (Density == Density.Rural) populationModifier = Random.Range(0.4f, 0.8f);
            Population = (int)(Population * populationModifier);
            Population = (Population / 1000) * 1000;

            // Seat calculation
            int tmpPop = Population;
            int tmpSeatRequirement = RequiredPopulationPerSeat;
            int tmpSeats = 0;
            while (tmpPop >= tmpSeatRequirement)
            {
                tmpSeats++;
                tmpPop -= tmpSeatRequirement;
                tmpSeatRequirement += RequirementIncreasePerSeat;
            }
            Seats = Mathf.Max(MinSeats, tmpSeats);

            // Voter calculation
            if (MentalityTypes.Contains(MentalityType.Predictable)) Voters = Random.Range(NumVotersPredictableMin, NumVotersPredictableMax + 1);
            else if (MentalityTypes.Contains(MentalityType.Unpredictable)) Voters = Random.Range(NumVotersUnpredictableMin, NumVotersUnpredictableMax + 1);
            else Voters = Random.Range(NumVotersDefaultMin, NumVotersDefaultMax + 1);

            // Voter turnour
            if (MentalityTypes.Contains(MentalityType.LowVoterTurnout)) VoterTurnout = Random.Range(VoterTurnoutLowMin, VoterTurnoutLowMax);
            else if (MentalityTypes.Contains(MentalityType.HighVoterTurnout)) VoterTurnout = Random.Range(VoterTurnoutHighMin, VoterTurnoutHighMax);
            else VoterTurnout = Random.Range(VoterTurnoutDefaultMin, VoterTurnoutDefaultMax);
        }

        private void SetGeographyTraits()
        {
            // Coastal
            if (Region.OceanCoastRatio > 0.6f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 3));
            else if (Region.OceanCoastRatio > 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 2));
            else if (Region.OceanCoastRatio > 0.05f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Coastal, 1));

            // Landlocked
            if (Region.CoastLength == 0 && Region.LandNeighbours.All(x => x.CoastLength == 0))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 3));
            else if (Region.CoastLength == 0 && Region.LandNeighbours.Where(x => x.CoastLength == 0).Count() >= 2)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 2));
            else if (Region.CoastLength == 0)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Landlocked, 1));

            // Island
            if (Region.Landmass.Size == 1)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 3));
            else if (Region.Landmass.Size <= 4)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 2));
            else if (Region.Landmass.Size <= 7)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Island, 1));

            // Tiny
            if (Region.Area <= 0.12f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 3));
            else if (Region.Area <= 0.18f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 2));
            else if (Region.Area <= 0.24f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Tiny, 1));

            // Large
            if (Region.Area >= 1.4f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 3));
            else if (Region.Area >= 1.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 2));
            else if (Region.Area >= 1.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 1));

            // Northern
            if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 3));
            else if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 2));
            else if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Northern, 1));

            // Southern
            if (Region.Centroid.y < Game.Map.Attributes.Height * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 3));
            else if (Region.Centroid.y < Game.Map.Attributes.Height * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 2));
            else if (Region.Centroid.y < Game.Map.Attributes.Height * 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Southern, 1));

            // Eastern
            if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 3));
            else if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 2));
            else if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Eastern, 1));

            // Western
            if (Region.Centroid.x < Game.Map.Attributes.Width * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Western, 3));
            else if (Region.Centroid.x < Game.Map.Attributes.Width * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Western, 2));
            else if (Region.Centroid.x < Game.Map.Attributes.Width * 0.3f)
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
        public DistrictElectionResult RunElection(List<Party> parties)
        {
            // Get party popularities
            Dictionary<Party, float> voterShares = new Dictionary<Party, float>();
            Dictionary<Party, int> partyPoints = new Dictionary<Party, int>();
            Dictionary<Party, int> partyVotes = new Dictionary<Party, int>();
            foreach (Party p in parties)
            {
                partyPoints.Add(p, GetPartyPopularity(p));
                partyVotes.Add(p, 0);
            }

            // Apply modifiers
            List<Modifier> electionModifiers = new List<Modifier>(); // Copy is created so that the modifiers in the election result don't get changed later
            foreach (Modifier m in Modifiers) electionModifiers.Add(m);
            foreach (Modifier m in electionModifiers)
            {
                if (m.Type == ModifierType.Positive) partyPoints[m.Party] += PositiveModifierImpact;
                else if (m.Type == ModifierType.Negative) partyPoints[m.Party] -= NegativeModifierImpact;
                else if (m.Type == ModifierType.Exclusion) partyPoints[m.Party] = 0;
            }

            // Cast "calculation" votes
            for (int i = 0; i < Voters; i++)
            {
                Party votedParty = GetSingleVoterResult(partyPoints);
                partyVotes[votedParty]++;
            }
            foreach (Party p in parties)
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
            Party winner = voterShares.First(x => x.Value == voterShares.Max(y => y.Value)).Key;

            // Calculate number of "game" votes based on voter turnout
            foreach(Party p in parties)
            {
                partyVotes[p] = (int)((Population * VoterTurnout) * voterShares[p] / 100f);
            }

            // Create result
            DistrictElectionResult result = new DistrictElectionResult()
            {
                ElectionCycle = Game.ElectionCycle,
                Year = Game.Year,
                Seats = Seats,
                District = this,
                Votes = partyVotes,
                VoteShare = voterShares,
                Winner = winner,
                Modifiers = electionModifiers,
                PartyPoints = partyPoints
            };

            ElectionResults.Add(result);
            CurrentWinnerParty = result.VoteShare.First(x => x.Value == result.VoteShare.Max(y => y.Value)).Key;
            CurrentWinnerShare = result.VoteShare.First(x => x.Value == result.VoteShare.Max(y => y.Value)).Value;

            return result;
        }

        public void OnElectionEnd()
        {
            UpdateModifiers();
            AddMentalityModifiers();
        }

        private int GetPartyPopularity(Party p)
        {
            int points = BasePopularity;
            if (HasMentality(MentalityType.Decided)) points = DecidedBasePopularity;
            if (HasMentality(MentalityType.Undecided)) points = UndecidedBasePopularity;

            foreach (GeographyTrait t in Geography)
            {
                points += p.GetPolicyValueFor(t.Type) * GetImpactPointsFor(t);
            }

            points += p.GetPolicyValueFor(Economy1) * HighImpactPopularity;
            points += p.GetPolicyValueFor(Economy2) * MediumImpactPopularity;
            points += p.GetPolicyValueFor(Economy3) * LowImpactPopularity;

            points += p.GetPolicyValueFor(Density) * MediumImpactPopularity;

            points += p.GetPolicyValueFor(AgeGroup) * MediumImpactPopularity;

            int languagePoints = p.GetPolicyValueFor(Language) * MediumImpactPopularity;
            if (HasMentality(MentalityType.Linguistic)) languagePoints *= 2;
            if (HasMentality(MentalityType.Nonlinguistic)) languagePoints /= 2;
            points += languagePoints;

            int religionPoints = p.GetPolicyValueFor(Religion) * MediumImpactPopularity;
            if (HasMentality(MentalityType.Religious)) religionPoints *= 2;
            if (HasMentality(MentalityType.Secular)) religionPoints /= 2;
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
            if (t.Category == 3) return HighImpactPopularity;
            if (t.Category == 2) return MediumImpactPopularity;
            if (t.Category == 1) return LowImpactPopularity;
            throw new System.Exception("Geography traits with a category outside 1,2,3 is not allowed.");
        }

        /// <summary>
        /// Returns the latest election result. With the offset value earlier election result can be gotten; offset = 1 returns the penultimate result.
        /// </summary>
        public DistrictElectionResult GetLatestElectionResult(int offset = 0)
        {
            if (ElectionResults.Count > offset) return ElectionResults[ElectionResults.Count - 1 - offset];
            else return null;
        }

        #endregion

        #region Modifiers

        private void UpdateModifiers()
        {
            foreach(Modifier modifier in Modifiers) modifier.RemainingLength--;
            Modifiers = Modifiers.Where(x => x.RemainingLength > 0).ToList();
        }

        private void AddMentalityModifiers()
        {
            if (HasMentality(MentalityType.Stable) && CurrentWinnerParty != null)
                Game.AddModifier(this, new Modifier(ModifierType.Positive, CurrentWinnerParty, 1, "Bonus for winning last election", "Stable Mentality"));

            if (HasMentality(MentalityType.Rebellious) && CurrentWinnerParty != null) 
                Game.AddModifier(this, new Modifier(ModifierType.Negative, CurrentWinnerParty, 1, "Malus for winning last election", "Rebellious Mentality"));

            if (HasMentality(MentalityType.Revolutionary) && CurrentWinnerParty != null) 
                Game.AddModifier(this, new Modifier(ModifierType.Exclusion, CurrentWinnerParty, 1, "Excluded for winning last election", "Revolutionary Mentality"));
        }

        private bool HasMentality(MentalityType t)
        {
            return MentalityTypes.Contains(t);
        }

        #endregion


    }
}