using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class District
    {
        public Random.State Seed; // Random value that was used to create this district. Value can be used to exactly recreate the district.
        public ElectionTacticsGame Game;
        public string Name;
        /// <summary>
        /// The how manyeth district this was when added. 0-indexed.
        /// </summary>
        public int Index { get; private set; }
        public Region Region;

        public bool IsActive { get; private set; }

        // Traits
        public List<GeographyTrait> Geography = new List<GeographyTrait>();
        public LanguageDef Language;
        public ReligionDef Religion;
        public DensityDef Density;
        public AgeGroupDef AgeGroup;
        public EconomicSectorDef Economy1;
        public EconomicSectorDef Economy2;
        public EconomicSectorDef Economy3;
        public List<CulturalTrait> CulturalTraits = new List<CulturalTrait>();

        private Dictionary<int, int> NumCulturalTraitWeights = new Dictionary<int, int>() // Probabilities for how many cultural traits
        {
            { 0, 15 },
            { 1, 30 },
            { 2, 36 },
            { 3, 14 },
            { 4, 5 },
        };

        // Election
        public List<DistrictElectionResult> ElectionResults = new List<DistrictElectionResult>();
        public Party CurrentWinnerParty;
        public float CurrentWinnerShare;

        public const int MinSeats = 1;
        public const int RequiredPopulationPerSeat = 40000;
        public const int RequirementIncreasePerSeat = 20000; // After each seat, the district needs this amount more population for the next seat

        public int Population;     // How many inhabitants the district has - It can vary from 32'000 to 2'400'000
        private float BasePopulationGrowthRate; // Randomized base growth rate of the district. Between -1% and 2%.
        public int Seats;          // How many seats this district has in the parliament
        public int Voters { get; set; } // How many people cast a vote (used just for calculation behind the scenes, actual voter count is based on population and voter turnout).
        public float VoterTurnout; // Value between 0 and 1 of how many people went to vote. This value is only relevant for the vote victory and is not used for calculation of the actual votes.

        public const int NumVoters = 3000; // Should lead to consistent results reflecting party popularities well with slight randomness

        public const float VoterTurnoutMin = 0.6f;
        public const float VoterTurnoutMax = 0.7f;

        public const int BasePopularity = 10;

        public List<Modifier> Modifiers = new List<Modifier>();

        // Visual
        public bool IsVisible { get; private set; }
        public UI_DistrictLabel MapLabel;

        #region Initialization

        public District(Random.State seed, ElectionTacticsGame game, Region r, string name, int index)
        {
            Seed = seed;
            Game = game;
            Region = r;
            Name = name;
            Index = index;

            Random.state = seed;

            SetGeographyTraits();

            DensityDef initialDensity = GetDensityForNewRegion();
            AgeGroup = GetAgeGroupForNewRegion();
            Language = GetLanguageForNewRegion();
            Religion = GetReligionForNewRegion();

            // Cultural Traits
            int numCulturalTraits = NumCulturalTraitWeights.GetWeightedRandomElement();
            while (CulturalTraits.Count < numCulturalTraits)
            {
                CulturalTraitDef def = Game.GetRandomAdoptableCulturalTraitDef(this);
                AddCulturalTrait(def);
            }

            // Seat allocation method
            Dictionary<SeatAllocationMethodDef, int> samCandidates = new Dictionary<SeatAllocationMethodDef, int>();
            foreach(SeatAllocationMethodDef def in DefDatabase<SeatAllocationMethodDef>.AllDefs)
            {
                samCandidates.Add(def, def.Commonness);
            }
            SeatAllocationMethodDef chosenMethod = samCandidates.GetWeightedRandomElement();

            // Check overrides through game settings
            if (Game.GameSettings.SeatDistribution == SeatDistributionGameSettingDefOf.WTA) chosenMethod = SeatAllocationMethodDefOf.WinnerTakesAll;
            if (Game.GameSettings.SeatDistribution == SeatDistributionGameSettingDefOf.Hamilton) chosenMethod = SeatAllocationMethodDefOf.HamiltonPR;
            if (Game.GameSettings.SeatDistribution == SeatDistributionGameSettingDefOf.DHondt) chosenMethod = SeatAllocationMethodDefOf.DHondtPR;

            // Apply fitting cultural traits
            if (chosenMethod == SeatAllocationMethodDefOf.HamiltonPR) AddCulturalTrait(CulturalTraitDefOf.ProportionalRepresentation);
            if (chosenMethod == SeatAllocationMethodDefOf.DHondtPR) AddCulturalTrait(CulturalTraitDefOf.MajorityBonus);

            // Population calculation
            Population = (int)(Region.Area * 1000000);
            float populationModifier = 0f;
            if (initialDensity == DensityDefOf.High) populationModifier = Random.Range(1.2f, 1.6f);
            if (initialDensity == DensityDefOf.Medium) populationModifier = Random.Range(0.8f, 1.2f);
            if (initialDensity == DensityDefOf.Low) populationModifier = Random.Range(0.4f, 0.8f);
            Population = (int)(Population * populationModifier);
            Population = (Population / 1000) * 1000;

            // Base population growth rate
            BasePopulationGrowthRate = Random.Range(ElectionTacticsGame.MIN_BASE_GROWTH_RATE, ElectionTacticsGame.MAX_BASE_GROWTH_RATE);

            // Calculations based on previous values
            RecalculateSeats();
            RecalculateDensity();

            // Economy (requires all previous attributes)
            AssignEconomicSectors();

            // Voter calculation
            Voters = NumVoters;
            VoterTurnout = Random.Range(VoterTurnoutMin, VoterTurnoutMax);

            // Set initially inactive
            IsActive = false;
        }

        public void AddCulturalTrait(CulturalTraitDef def)
        {
            CulturalTrait trait = (CulturalTrait)System.Activator.CreateInstance(def.TraitClass);
            trait.Init(def, this);
            CulturalTraits.Add(trait);
        }

        private void RecalculateSeats()
        {
            int seatsBefore = Seats;

            int tmpPop = Population;
            int tmpSeatRequirement = RequiredPopulationPerSeat;
            int tmpSeats = 1;
            while (tmpPop >= tmpSeatRequirement)
            {
                tmpSeats++;
                tmpPop -= tmpSeatRequirement;
                tmpSeatRequirement += RequirementIncreasePerSeat;
            }
            Seats = Mathf.Max(MinSeats, tmpSeats);

            int seatsAfter = Seats;

            if (seatsBefore != seatsAfter) Game.RegisterNewsEvent(new NewsEvent_DistrictSeatChange(this, seatsBefore, seatsAfter));
        }

        private void RecalculateDensity()
        {
            DensityDef densityBefore = Density;

            float populationPerArea = Population / Region.Area;

            if (populationPerArea >= 1200000f) Density = DensityDefOf.High;
            else if (populationPerArea >= 800000f) Density = DensityDefOf.Medium;
            else Density = DensityDefOf.Low;

            DensityDef densityAfter = Density;

            if (densityBefore != densityAfter) Game.RegisterNewsEvent(new NewsEvent_DistrictDensityChange(this, densityBefore, densityAfter));
        }

        private void SetGeographyTraits()
        {
            // Coastal
            if (Region.OceanCoastRatio > 0.6f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Coastal, 3));
            else if (Region.OceanCoastRatio > 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Coastal, 2));
            else if (Region.OceanCoastRatio > 0.01f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Coastal, 1));

            // Landlocked
            if (Region.CoastLength == 0 && Region.LandNeighbours.All(x => x.CoastLength == 0))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Landlocked, 3));
            else if (Region.CoastLength == 0 && Region.LandNeighbours.Where(x => x.CoastLength == 0).Count() >= 2)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Landlocked, 2));
            else if (Region.CoastLength == 0)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Landlocked, 1));

            // Island
            if (Region.Landmass.Size == 1)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Island, 3));
            else if (Region.Landmass.Size <= 4)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Island, 2));
            else if (Region.Landmass.Size <= 7)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Island, 1));

            // Tiny
            if (Region.Area <= 0.12f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Tiny, 3));
            else if (Region.Area <= 0.18f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Tiny, 2));
            else if (Region.Area <= 0.24f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Tiny, 1));

            // Large
            if (Region.Area >= 1.4f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Large, 3));
            else if (Region.Area >= 1.25f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Large, 2));
            else if (Region.Area >= 1.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Large, 1));

            // Northern
            if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.North, 3));
            else if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.North, 2));
            else if (Region.Centroid.y > Game.Map.Attributes.Height - (Game.Map.Attributes.Height * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.North, 1));

            // Southern
            if (Region.Centroid.y < Game.Map.Attributes.Height * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.South, 3));
            else if (Region.Centroid.y < Game.Map.Attributes.Height * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.South, 2));
            else if (Region.Centroid.y < Game.Map.Attributes.Height * 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.South, 1));

            // Eastern
            if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.1f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.East, 3));
            else if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.2f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.East, 2));
            else if (Region.Centroid.x > Game.Map.Attributes.Width - (Game.Map.Attributes.Width * 0.3f))
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.East, 1));

            // Western
            if (Region.Centroid.x < Game.Map.Attributes.Width * 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.West, 3));
            else if (Region.Centroid.x < Game.Map.Attributes.Width * 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.West, 2));
            else if (Region.Centroid.x < Game.Map.Attributes.Width * 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.West, 1));

            // Lakeside
            if (Region.LakeCoastRatio > 0.3f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Lake, 3));
            else if (Region.LakeCoastRatio > 0.15f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Lake, 2));
            else if (Region.LakeCoastRatio > 0.011f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Lake, 1));

            // River
            float riverBorderLength = Region.Borders.Where(x => x.River != null).Sum(x => x.Length);
            float riverBorderRatio = riverBorderLength / Region.TotalBorderLength;
            if (riverBorderRatio > 0.2f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.River, 3));
            else if (riverBorderRatio > 0.1f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.River, 2));
            else if (riverBorderRatio > 0f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.River, 1));
        }

        private DensityDef GetDensityForNewRegion() // Denisty is weighted random rural > mixed > urban
        {
            // Base weights
            int highWeight = 25;
            int mediumWeight = 33;
            int lowWeight = 42;

            // Skew based on area: smaller districts lean High, larger lean Low
            // Area roughly ranges 0.05 - 2.0. Normalize around 0.5 as neutral.
            // Tiny III districts get around a +11 high weight and -11 low weight for this
            float areaBias = (Region.Area - 0.5f) * 30f; // Negative for small, positive for large
            highWeight = Mathf.Max(5, highWeight - (int)areaBias);
            mediumWeight = Mathf.Max(5, mediumWeight);
            lowWeight = Mathf.Max(5, lowWeight + (int)areaBias);

            Dictionary<DensityDef, int> densityCandidates = new Dictionary<DensityDef, int>()
            {
                { DensityDefOf.High,   highWeight },
                { DensityDefOf.Medium, mediumWeight },
                { DensityDefOf.Low,    lowWeight },
            };

            return densityCandidates.GetWeightedRandomElement();
        }

        private AgeGroupDef GetAgeGroupForNewRegion() // Age group is fully random
        {
            Dictionary<AgeGroupDef, int> candidates = new Dictionary<AgeGroupDef, int>();
            foreach (AgeGroupDef def in DefDatabase<AgeGroupDef>.AllDefs)
            {
                candidates.Add(def, def.Commonness);
            }
            return candidates.GetWeightedRandomElement();
        }
        private LanguageDef GetLanguageForNewRegion() // Languages can spread over land borders
        {
            Dictionary<LanguageDef, int> languageCandidates = new Dictionary<LanguageDef, int>();

            // Add a random language with weight 1
            languageCandidates.Add(ElectionTacticsGame.GetRandomLanguage(), 1);

            // Add each existing land-neighbouring language with a weight 2
            foreach (Region r in Region.LandNeighbours)
            {
                District d = Game.GetDistrict(r);
                if (d != null) languageCandidates.Increment(d.Language, 2);
            }

            // Return
            return languageCandidates.GetWeightedRandomElement();
        }
        private ReligionDef GetReligionForNewRegion() // Religion can spread over land and water
        {
            Dictionary<ReligionDef, int> religionCandidates = new Dictionary<ReligionDef, int>();

            // Add a random religion with weight 2
            religionCandidates.Add(ElectionTacticsGame.GetRandomReligion(), 2);

            // Add each existing neighbouring religion (including water connections) with a weight 3
            foreach (Region r in Region.Neighbours)
            {
                District d = Game.GetDistrict(r);
                if (d != null) religionCandidates.Increment(d.Religion, 2);
            }

            // Return
            return religionCandidates.GetWeightedRandomElement();
        }

        private void AssignEconomicSectors()
        {
            List<EconomicSectorDef> chosen = new List<EconomicSectorDef>();
            for (int i = 0; i < 3; i++)
            {
                Dictionary<EconomicSectorDef, int> candidates = new Dictionary<EconomicSectorDef, int>();
                foreach (EconomicSectorDef def in DefDatabase<EconomicSectorDef>.AllDefs)
                {
                    if (chosen.Contains(def)) continue;
                    int weight = def.GetWeight(this);
                    if (weight <= 0) continue; // respects hard requirements (e.g. Fishing with no water)
                    candidates.Add(def, weight);
                }

                // Fallback: if everything got filtered out (rare), allow any unused sector at neutral weight
                if (candidates.Count == 0)
                {
                    foreach (EconomicSectorDef def in DefDatabase<EconomicSectorDef>.AllDefs)
                        if (!chosen.Contains(def)) candidates.Add(def, 100);
                }

                chosen.Add(candidates.GetWeightedRandomElement());
            }
            Economy1 = chosen[0];
            Economy2 = chosen[1];
            Economy3 = chosen[2];
        }

        public void Activate()
        {
            IsActive = true;
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

            // Add modifiers to result (as a copy)
            List<Modifier> electionModifiers = Modifiers.Where(m => parties.Contains(m.Party)).Select(m => new Modifier(m)).ToList(); 

            // Exclude parties with exclusion modifiers
            foreach (Modifier m in electionModifiers.Where(x => x.Type == ModifierType.Exclusion)) partyPopularities[m.Party] = 0;

            // Cast "calculation" votes
            for (int i = 0; i < Voters; i++)
            {
                Party votedParty = partyPopularities.GetWeightedRandomElement();
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
            SeatAllocationMethodDef allocationMethod = GetSeatAllocationMethod();
            seatsWon = allocationMethod.AllocateSeats(Seats, voterShares);

            // Create result
            Debug.Log($"Saving district election result for {Name} for cycle {Game.ElectionCycle} with {Seats} seats.");
            DistrictElectionResult result = new DistrictElectionResult
            (
                Game.ElectionCycle,
                Game.Year,
                Population,
                Seats,
                Density,
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
            // Population Growth
            float populationGrowthFactor = 1f + (GetPopulationGrowthRate() / 100f);
            Population = (int)(Population * populationGrowthFactor);

            // Modifiers
            UpdateModifiers();

            // Cultural traits
            foreach (CulturalTrait trait in CulturalTraits) trait.OnPostElection();

            // Recalculate
            RecalculateSeats();
            RecalculateDensity();
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
                if (m.Type == ModifierType.Positive) factors.Add(m.Source + " Modifier", m.Value);
                else if (m.Type == ModifierType.Negative) factors.Add(m.Source + " Modifier", -m.Value);
            }

            return factors;
        }

        /// <summary>
        /// The final calculated population growth rate in %. Applied after every cycle.
        /// </summary>
        public float GetPopulationGrowthRate()
        {
            float value = BasePopulationGrowthRate;
            value += AgeGroup.PopulationGrowthRateModifier;

            foreach (CulturalTrait ct in CulturalTraits)
            {
                value += ct.PopulationGrowthRateModifier;
            }

            return value;
        }

        /// <summary>
        /// Returns the amount of cycles remaining, until the district gains or loses a seat based on the population growth.
        /// 1 means it will gain/lose a seat after the next election. -1 means seat amount will not change.
        /// If growth rate is positive, this corresponds to a seat gain, else to a seat loss.
        /// </summary>
        /// <returns></returns>
        public int GetSeatChangeCountdown()
        {
            float growthRate = GetPopulationGrowthRate();
            if (growthRate == 0f) return -1;

            int currentSeats = Seats;
            int simulatedPopulation = Population;

            int cap = 50; // if it would take more than this amount of cycles, return as no change.

            if (growthRate > 0f)
            {
                // Find the population threshold for gaining the next seat
                // Threshold for seat N+1: sum of RequiredPopulationPerSeat + RequirementIncreasePerSeat * (0 + 1 + ... + N-1)
                // = currentSeats * RequiredPopulationPerSeat + RequirementIncreasePerSeat * (currentSeats * (currentSeats - 1) / 2)
                int nextSeatThreshold = currentSeats * RequiredPopulationPerSeat
                    + RequirementIncreasePerSeat * (currentSeats * (currentSeats - 1) / 2);

                if (simulatedPopulation >= nextSeatThreshold) return -1; // Already at threshold, RecalculateSeats handles this

                int cycles = 0;
                while (simulatedPopulation < nextSeatThreshold)
                {
                    simulatedPopulation = (int)(simulatedPopulation * (1f + growthRate / 100f));
                    cycles++;
                    if (cycles > cap) return -1; // Safety cap
                }
                return cycles;
            }
            else
            {
                // Find the population threshold below which a seat is lost
                // Threshold for losing a seat: population needed for current seat count
                // = (currentSeats - 1) * RequiredPopulationPerSeat + RequirementIncreasePerSeat * ((currentSeats-1) * (currentSeats-2) / 2)
                if (currentSeats <= MinSeats) return -1; // Can't lose seats below minimum

                int lossSeatThreshold = (currentSeats - 1) * RequiredPopulationPerSeat
                    + RequirementIncreasePerSeat * ((currentSeats - 1) * (currentSeats - 2) / 2);

                int cycles = 0;
                while (simulatedPopulation > lossSeatThreshold)
                {
                    simulatedPopulation = (int)(simulatedPopulation * (1f + growthRate / 100f));
                    cycles++;
                    if (cycles > cap) return -1; // Safety cap
                }
                return cycles;
            }
        }

        #endregion

        #region Modifiers

        private void UpdateModifiers()
        {
            foreach (Modifier modifier in Modifiers.Where(m => !m.IsPermanent)) modifier.RemainingLength--;
            Modifiers = Modifiers.Where(m => m.RemainingLength > 0 || m.IsPermanent).ToList();
        }

        #endregion

        #region Visual

        public void SetVisible(bool v)
        {
            IsVisible = v;
            MapLabel.gameObject.SetActive(v);
        }

        #endregion

        #region Getters

        public List<District> AdjacentActiveDistricts => Region.LandNeighbours.Where(r => Game.HasDistrict(r)).Select(r => Game.GetDistrict(r)).Where(d => d.IsActive).ToList();

        public CulturalTrait GetSeatDistributionTrait() => CulturalTraits.FirstOrDefault(t => t.Def.IsSeatDistributionTrait);

        public bool HasCulturalTrait(CulturalTraitDef def) => CulturalTraits.Any(t => t.Def == def);

        public SeatAllocationMethodDef GetSeatAllocationMethod()
        {
            if (HasCulturalTrait(CulturalTraitDefOf.ProportionalRepresentation)) return SeatAllocationMethodDefOf.HamiltonPR;
            if (HasCulturalTrait(CulturalTraitDefOf.MajorityBonus)) return SeatAllocationMethodDefOf.DHondtPR;
            return SeatAllocationMethodDefOf.WinnerTakesAll;
        }

        public bool IsMinorityLanguage => !Game.IsMostCommonLanguage(Language);
        public bool IsMinorityReligion => Religion != ReligionDefOf.None && !Game.IsMostCommonReligion(Religion);

        #endregion

    }
}
