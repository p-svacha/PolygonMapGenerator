using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace ElectionTactics
{
    public class District
    {
        public Random.State Seed; // Random value that was used to create this district. Value can be used to exactly recreate the district.
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
        public List<MentalityTrait> MentalityTraits = new List<MentalityTrait>();

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

        public const int NumVotersMin = 1500;
        public const int NumVotersMax = 1700;

        public const float VoterTurnoutMin = 0.6f;
        public const float VoterTurnoutMax = 0.7f;

        public const int BasePopularity = 10;

        public const int PositiveModifierImpact = 30;
        public const int NegativeModifierImpact = 30;

        public List<Modifier> Modifiers = new List<Modifier>();

        // Visual
        public bool IsVisible { get; private set; }
        public UI_DistrictLabel MapLabel;

        #region Initialization

        public District(Random.State seed, ElectionTacticsGame game, Region r, string name)
        {
            Seed = seed;
            Game = game;
            Region = r;
            Name = name;

            Random.state = seed;

            SetGeographyTraits();

            Density = GetDensityForNewRegion();
            AgeGroup = GetAgeGroupForNewRegion();
            Language = GetLanguageForNewRegion();
            Religion = GetReligionForNewRegion();

            Economy1 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            while (Economy2 == Economy1) Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while (Economy3 == Economy2 || Economy3 == Economy1) Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();

            int numMentalities = Random.Range(1, 4);
            while (MentalityTraits.Count < numMentalities)
            {
                MentalityTraitDef def = Game.GetRandomAdoptableMentalityTraitDef(this);
                MentalityTrait trait = (MentalityTrait)System.Activator.CreateInstance(def.TraitClass);
                trait.Init(def, this);
                MentalityTraits.Add(trait);
            }

            // Population calculation
            Population = (int)(Region.Area * 1000000);
            float populationModifier = 0f;
            if (Density == Density.Urban) populationModifier = Random.Range(1.2f, 1.6f);
            if (Density == Density.Suburban) populationModifier = Random.Range(0.8f, 1.2f);
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
            Voters = Random.Range(NumVotersMin, NumVotersMax + 1);
            VoterTurnout = Random.Range(VoterTurnoutMin, VoterTurnoutMax);
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
            else if (Region.Area >= 1.25f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitType.Large, 2));
            else if (Region.Area >= 1.1f)
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

        private Density GetDensityForNewRegion() // Denisty is weighted random rural > mixed > urban
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.25f) return Density.Urban; // Urban - 25% chance
            else if (rng < 0.25f + 0.33f) return Density.Suburban; // Mixed - 33% chance
            else return Density.Rural; // Rural  - 42 % chance
        }
        private AgeGroup GetAgeGroupForNewRegion() // Age group is fully random
        {
            return ElectionTacticsGame.GetRandomAgeGroup();
        }
        private Language GetLanguageForNewRegion() // Languages can spread over land borders
        {
            List<Language> languageChances = new List<Language>();
            languageChances.Add(ElectionTacticsGame.GetRandomLanguage());
            foreach (Region r in Region.LandNeighbours)
            {
                if (Game.VisibleDistricts.ContainsKey(r)) languageChances.Add(Game.VisibleDistricts[r].Language);
            }
            return languageChances[Random.Range(0, languageChances.Count)];
        }
        private Religion GetReligionForNewRegion() // Religion can spread over land and water
        {
            List<Religion> religionChances = new List<Religion>();
            foreach (Region r in Region.Neighbours)
            {
                Religion religion;
                if (Game.VisibleDistricts.ContainsKey(r)) religion = Game.VisibleDistricts[r].Religion;
                else religion = ElectionTacticsGame.GetRandomReligion();

                religionChances.Add(religion);
            }
            return religionChances[UnityEngine.Random.Range(0, religionChances.Count)];
        }

        #endregion

        #region Election

        /// <summary>
        /// This function calculates the election results of an election between the given parties.
        /// A specified amount of single voters will vote, whereas their vote will be decided by weighted random based on party points.
        /// Each party has x base points. On top of that points are added for policies that match the district traits, modifiers and mentality.
        /// There will always be a single winner party.
        /// This function only returns a result, but does not yet add it to the district/game. To add it call AddElectionResult().
        /// </summary>
        public DistrictElectionResult RunElection(List<Party> parties)
        {
            // Get party popularities
            Dictionary<Party, float> voterShares = new Dictionary<Party, float>();
            Dictionary<Party, int> partyPopularities = new Dictionary<Party, int>();
            Dictionary<Party, int> partyVotes = new Dictionary<Party, int>();
            Dictionary<Party, int> seatsWon = new Dictionary<Party, int>();

            foreach (Party p in parties)
            {
                partyPopularities.Add(p, GetPartyPopularity(p));
                partyVotes.Add(p, 0);
            }

            // Add modifiers to result
            List<Modifier> electionModifiers = new List<Modifier>(); // Copy is created so that the modifiers in the election result don't get changed later
            foreach (Modifier m in Modifiers) electionModifiers.Add(m);

            // Exclude parties with exclusion modifiers
            foreach(Modifier m in Modifiers.Where(x => x.Type == ModifierType.Exclusion)) partyPopularities[m.Party] = 0;

            // Cast "calculation" votes
            for (int i = 0; i < Voters; i++)
            {
                Party votedParty = GetSingleVoterResult(partyPopularities);
                partyVotes[votedParty]++;
            }
            foreach (Party p in parties)
            {
                //Debug.Log(p.Name + " got " + partyVotes[p] + " votes.");
                voterShares.Add(p, 100f * partyVotes[p] / Voters);
            }

            // Guarantee that there is only one winner (by having winner have 0.1% more share than others)
            List<Party> winnerParties = voterShares.Where(x => x.Value == voterShares.Values.Max(v => v)).Select(x => x.Key).ToList();
            if (winnerParties.Count > 1)
            {
                Party singleWinnerParty = winnerParties[Random.Range(0, winnerParties.Count)];
                voterShares[singleWinnerParty] += 0.1f;
            }
            Party winner = voterShares.OrderByDescending(x => x.Value).First().Key;

            // Calculate number of "game" votes based on voter turnout
            foreach(Party p in parties)
            {
                partyVotes[p] = (int)((Population * VoterTurnout) * voterShares[p] / 100f);
            }

            // Calculate number of seats won for each party
            foreach (Party p in parties)
            {
                // Winner takes it all
                if (p == winner) seatsWon[p] = Seats;
                else seatsWon[p] = 0;
            }

            // Create result
            DistrictElectionResult result = new DistrictElectionResult
            (
                Game.ElectionCycle,
                Game.Year,
                Seats,
                new List<Party>(parties),
                this,
                partyPopularities,
                partyVotes,
                voterShares,
                seatsWon,
                winner,
                electionModifiers
            );

            return result;
        }

        public void OnElectionEnd()
        {
            UpdateModifiers();
            foreach (MentalityTrait trait in MentalityTraits) trait.OnPostElection();
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
            throw new System.Exception("Couldn't find single voter result: sum=" + sum + ", rng=" + rng);
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

        #region Popularity Calculations

        /// <summary>
        /// Returns the popularity a party has in this district
        /// </summary>
        public int GetPartyPopularity(Party party)
        {
            int popularity = GetPartyPopularityBreakdown(party).Sum(x => x.Value);
            if (popularity < 0) popularity = 0;
            return popularity;
        }

        /// <summary>
        /// Returns all factors that affect the party popularity in this district. The sum all factors equals the absolute popularity.
        /// </summary>
        public Dictionary<string, int> GetPartyPopularityBreakdown(Party party)
        {
            Dictionary<string, int> factors = new Dictionary<string, int>();

            // Base popularity
            int basePopularity = BasePopularity;
            factors.Add("Base Popularity", basePopularity);

            // Policies
            foreach (Policy policy in party.ActivePolicies) factors.Add($"{policy.Name } Policy ({policy.Value})", policy.GetCurrentImpactOn(this) );

            // Positive & Negative Modifiers
            foreach (Modifier m in Modifiers.Where(x => x.Party == party))
            {
                if (m.Type == ModifierType.Positive) factors.Add(m.Source + " Modifier", PositiveModifierImpact);
                else if (m.Type == ModifierType.Negative) factors.Add(m.Source + " Modifier", -NegativeModifierImpact);
            }

            return factors;
        }

        #endregion

        #region Modifiers

        private void UpdateModifiers()
        {
            foreach(Modifier modifier in Modifiers) modifier.RemainingLength--;
            Modifiers = Modifiers.Where(x => x.RemainingLength > 0).ToList();
        }

        private bool HasMentality(MentalityTraitDef def)
        {
            return MentalityTraits.Any(m => m.Def == def);
        }

        #endregion

        #region Visual

        public void SetVisible(bool v)
        {
            IsVisible = v;
            MapLabel.gameObject.SetActive(v);
        }

        #endregion

    }
}
