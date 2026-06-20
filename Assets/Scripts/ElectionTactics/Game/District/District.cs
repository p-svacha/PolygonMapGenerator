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
        public List<CulturalTrait> CulturalTraits { get; set; } = new List<CulturalTrait>();
        public List<CulturalTrait> ActiveCulturalTraits => CulturalTraits.Where(t => t.IsActive).ToList();

        private Dictionary<int, int> NumCulturalTraitWeights = new Dictionary<int, int>() // Probabilities for how many cultural traits
        {
            { 0, 15 },
            { 1, 37 },
            { 2, 32 },
            { 3, 12 },
            { 4, 4 },
        };

        // Election
        public List<DistrictElectionResult> ElectionResults = new List<DistrictElectionResult>();
        public Party CurrentWinnerParty;
        public float CurrentWinnerShare;

        public const int MIN_SEATS = 1;
        public const int RequiredPopulationPerSeat = 40000;
        public const int RequirementIncreasePerSeat = 20000; // After each seat, the district needs this amount more population for the next seat

        public int Population;     // How many inhabitants the district has - It can vary from 32'000 to 2'400'000
        private float BasePopulationGrowthRate; // Randomized base growth rate of the district. Between -1% and 2%.
        private int SeatsFromPopulation { get; set; }          // Base amount of seats this district is worth
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
            foreach (SeatAllocationMethodDef def in DefDatabase<SeatAllocationMethodDef>.AllDefs)
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
            RecalculateSeatsFromPopulation();
            RecalculateDensity();

            // Economy (requires all previous attributes)
            AssignEconomicSectors();

            // Voter calculation
            Voters = NumVoters;
            VoterTurnout = Random.Range(VoterTurnoutMin, VoterTurnoutMax);

            // Set initially inactive
            IsActive = false;
        }

        public CulturalTrait AddCulturalTrait(CulturalTraitDef def, bool skipOnInit = false)
        {
            CulturalTrait trait = (CulturalTrait)System.Activator.CreateInstance(def.TraitClass);
            trait.Init(def, this, skipOnInit);
            CulturalTraits.Add(trait);
            return trait;
        }

        public void RemoveCulturalTrait(CulturalTraitDef def)
        {
            CulturalTrait toRemove = CulturalTraits.FirstOrDefault(t => t.Def == def);
            if (toRemove != null) RemoveCulturalTrait(toRemove);
        }

        public void RemoveCulturalTrait(CulturalTrait trait)
        {
            CulturalTraits.Remove(trait);
            trait.OnRemoved();
        }

        private void RecalculateSeatsFromPopulation()
        {
            int totalSeatsBefore = GetSeats();

            int tmpPop = Population;
            int tmpSeatRequirement = RequiredPopulationPerSeat;
            int tmpSeats = 1;
            while (tmpPop >= tmpSeatRequirement)
            {
                tmpSeats++;
                tmpPop -= tmpSeatRequirement;
                tmpSeatRequirement += RequirementIncreasePerSeat;
            }
            SeatsFromPopulation = Mathf.Max(MIN_SEATS, tmpSeats);

            int seatsAfter = GetSeats();

            if (totalSeatsBefore != seatsAfter) Game.RegisterNewsEvent(new NewsEvent_DistrictSeatChange(this, totalSeatsBefore, seatsAfter));
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
            if (Region.OceanCoastRatio > 0.65f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Coastal, 3));
            else if (Region.OceanCoastRatio > 0.35f)
                Geography.Add(Game.GetGeographyTrait(GeographyTraitDefOf.Coastal, 2));
            else if (Region.OceanCoastRatio > 0.1f)
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
            // Check if any active adjacent district has linguistic imperialism
            List<LanguageDef> forcedLanguages = new List<LanguageDef>();
            foreach (District d in Region.LandNeighbours.Where(r => Game.HasDistrict(r)).Select(r => Game.GetDistrict(r)))
            {
                foreach(CulturalTrait trait in d.ActiveCulturalTraits)
                {
                    if (trait is CT_Imperialistic impTrait && impTrait.PolicyType == PolicyType.Language)
                    {
                        forcedLanguages.Add(d.Language);
                    }
                }
            }
            if (forcedLanguages.Count > 0) return forcedLanguages.RandomElement();

            // Choose semi-randomly
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
            // Check if any active adjacent district has religious imperialism
            List<ReligionDef> forcedReligions = new List<ReligionDef>();
            foreach (District d in Region.LandNeighbours.Where(r => Game.HasDistrict(r)).Select(r => Game.GetDistrict(r)))
            {
                foreach (CulturalTrait trait in d.ActiveCulturalTraits)
                {
                    if (trait is CT_Imperialistic impTrait && impTrait.PolicyType == PolicyType.Religion)
                    {
                        forcedReligions.Add(d.Religion);
                    }
                }
            }
            if (forcedReligions.Count > 0) return forcedReligions.RandomElement();

            // Choose semi-randomly
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
            foreach (CulturalTrait trait in CulturalTraits) trait.OnDistrictActivated();
        }
        public void Deactivate()
        {
            IsActive = false;
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

            // Apply ad-hoc modifiers from cultural traits
            foreach (CulturalTrait trait in ActiveCulturalTraits)
            {
                trait.OnPreElection();
            }

            // Calculate party popularities
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
            int totalPopularity = partyPopularities.Values.Sum();
            for (int i = 0; i < Voters; i++)
            {
                Party votedParty;
                if (totalPopularity <= 0)
                    votedParty = parties[Random.Range(0, parties.Count)]; // Fully random when no party has any popularity
                else
                    votedParty = partyPopularities.GetWeightedRandomElement(); // Usually weighted random according to party popularity

                partyVotes[votedParty]++;
            }

            // Calculate raw vote shares
            Dictionary<Party, float> rawShares = new Dictionary<Party, float>();
            foreach (Party p in parties) rawShares.Add(p, 100f * partyVotes[p] / Voters);

            // Normalize vote shares: one decimal, unique, sums to 100.0
            voterShares = NormalizeVoteShares(rawShares);

            // Identify single winner
            Party winner = voterShares.OrderByDescending(x => x.Value).First().Key;

            // Calculate number of "game" votes based on voter turnout
            foreach (Party p in parties)
            {
                partyVotes[p] = (int)((Population * VoterTurnout) * voterShares[p] / 100f);
            }

            // Calculate number of seats won for each party
            SeatAllocationMethodDef allocationMethod = GetSeatAllocationMethod();
            int numTotalSeats = GetSeats();
            seatsWon = allocationMethod.AllocateSeats(numTotalSeats, voterShares);

            // Create result
            Debug.Log($"Saving district election result for {Name} for cycle {Game.ElectionCycle} with {numTotalSeats} seats.");
            DistrictElectionResult result = new DistrictElectionResult
            (
                Game.ElectionCycle,
                Game.Year,
                Population,
                numTotalSeats,
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

        /// <summary>
        /// Normalizes raw vote shares so that: every share has exactly one decimal,
        /// no two parties share the exact same value, and the total is exactly 100.0.
        /// </summary>
        private Dictionary<Party, float> NormalizeVoteShares(Dictionary<Party, float> rawShares)
        {
            // Parties with 0 raw votes are always exactly 0.0% — exempt from the uniqueness rule.
            var zeroParties = rawShares.Where(x => x.Value <= 0f).Select(x => x.Key).ToList();
            var nonZeroShares = rawShares.Where(x => x.Value > 0f).ToDictionary(x => x.Key, x => x.Value);

            Dictionary<Party, float> result = new Dictionary<Party, float>();

            if (nonZeroShares.Count > 0)
            {
                // 1. Round every non-zero share to one decimal.
                Dictionary<Party, float> shares = nonZeroShares.ToDictionary(
                    x => x.Key,
                    x => Mathf.Round(x.Value * 10f) / 10f
                );

                // 2. Break exact ties among non-zero shares only.
                List<Party> ordered = shares.Keys.OrderByDescending(p => nonZeroShares[p]).ToList();
                HashSet<float> used = new HashSet<float>();
                foreach (Party p in ordered)
                {
                    float v = shares[p];
                    while (used.Contains(v)) v = Mathf.Round((v + 0.1f) * 10f) / 10f;
                    shares[p] = v;
                    used.Add(v);
                }

                // 3. Correct rounding drift so non-zero shares sum to exactly 100.0.
                float sum = shares.Values.Sum();
                float diff = Mathf.Round((100f - sum) * 10f) / 10f;
                if (diff != 0f)
                {
                    Party largest = shares.OrderByDescending(x => x.Value).First().Key;
                    shares[largest] = Mathf.Round((shares[largest] + diff) * 10f) / 10f;
                }

                foreach (var kvp in shares) result[kvp.Key] = kvp.Value;
            }

            // Zero parties always get exactly 0.0, regardless of how many there are.
            foreach (Party p in zeroParties) result[p] = 0f;

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
            foreach (CulturalTrait trait in ActiveCulturalTraits) trait.OnPostElection();

            // Recalculate
            RecalculateSeatsFromPopulation();
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
        /// Returns the popularity a party has in this district.
        /// <br/>If includeOtherDistrictPopularityInfluence is false, all popularity impacts based on the popularity of other districts are excluded. Used to prevent circular/chained popularity impacts.
        /// </summary>
        public int GetPartyPopularity(Party party, bool includeOtherDistrictPopularityInfluence = true)
        {
            int popularity = GetPartyPopularityBreakdown(party, includeOtherDistrictPopularityInfluence).Sum(x => x.Value);
            if (popularity < 0) popularity = 0;
            return popularity;
        }

        public List<(string Label, int Value)> GetPartyPopularityBreakdown(Party party, bool includeOtherDistrictPopularityInfluence)
        {
            var factors = new List<(string Label, int Value)>();

            // Base popularity
            factors.Add(("Base Popularity", BasePopularity));

            // Policies
            foreach (Policy policy in party.ActivePolicies)
                factors.Add(($"{policy.Name} Policy ({policy.Value})", policy.GetCurrentImpactOn(this)));

            // Positive & Negative Modifiers
            foreach (Modifier m in Modifiers.Where(m => m.Party == party))
            {
                if (m.Type == ModifierType.Positive) factors.Add((m.Description, m.Value));
                else if (m.Type == ModifierType.Negative) factors.Add((m.Description, -m.Value));
            }

            // Modifiers from cultural traits
            foreach (CulturalTrait trait in ActiveCulturalTraits)
                factors.AddRange(trait.GetPopularityChange(party));

            // Modifiers from the popularity of other districts
            if (includeOtherDistrictPopularityInfluence)
            {
                foreach (CulturalTrait trait in ActiveCulturalTraits)
                    factors.AddRange(trait.GetPopularityChangeFromOtherDistrictPopularities(party));

                foreach (District neighbour in ActiveLandNeighbours)
                    foreach (CulturalTrait trait in neighbour.ActiveCulturalTraits)
                        factors.AddRange(trait.GetPopularityChangeInNeighbours(party));
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

            foreach (CulturalTrait ct in ActiveCulturalTraits)
            {
                value += ct.Def.PopulationGrowthRateModifier;
            }
            foreach (District neighbour in ActiveLandNeighbours)
            {
                foreach (CulturalTrait ct in neighbour.ActiveCulturalTraits)
                {
                    value += ct.Def.NeighbourPopulationGrowthModifier;
                }
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

            int currentSeats = SeatsFromPopulation;
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

                if (cycles == 0) cycles = 1; // Safety for when exactly at threshold
                return cycles;
            }
            else
            {
                // Find the population threshold below which a seat is lost
                // Threshold for losing a seat: population needed for current seat count
                // = (currentSeats - 1) * RequiredPopulationPerSeat + RequirementIncreasePerSeat * ((currentSeats-1) * (currentSeats-2) / 2)
                if (currentSeats <= MIN_SEATS) return -1; // Can't lose seats below minimum

                int lossSeatThreshold = (currentSeats - 1) * RequiredPopulationPerSeat
                    + RequirementIncreasePerSeat * ((currentSeats - 1) * (currentSeats - 2) / 2);

                int cycles = 0;
                while (simulatedPopulation > lossSeatThreshold)
                {
                    simulatedPopulation = (int)(simulatedPopulation * (1f + growthRate / 100f));
                    cycles++;
                    if (cycles > cap) return -1; // Safety cap
                }

                if (cycles == 0) cycles = 1; // Safety for when exactly at threshold
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

        /// <summary>
        /// Returns the actual amount of seats this district is worth, including all modifiers.
        /// </summary>
        /// <returns></returns>
        public int GetSeats()
        {
            int numSeats = SeatsFromPopulation;

            foreach (CulturalTrait ct in ActiveCulturalTraits) numSeats += ct.Def.SeatModifier;

            if (numSeats < MIN_SEATS) numSeats = MIN_SEATS;
            return numSeats;
        }

        public List<District> ActiveLandNeighbours => Region.LandNeighbours.Where(r => Game.HasDistrict(r)).Select(r => Game.GetDistrict(r)).Where(d => d.IsActive).ToList();

        public CulturalTrait GetSeatDistributionTrait() => CulturalTraits.FirstOrDefault(t => t.Def.IsSeatDistributionTrait);

        public bool HasCulturalTrait(CulturalTraitDef def) => CulturalTraits.Any(t => t.Def == def);

        public bool HasReligion => Religion != ReligionDefOf.None;

        public SeatAllocationMethodDef GetSeatAllocationMethod()
        {
            if (HasCulturalTrait(CulturalTraitDefOf.ProportionalRepresentation)) return SeatAllocationMethodDefOf.HamiltonPR;
            if (HasCulturalTrait(CulturalTraitDefOf.MajorityBonus)) return SeatAllocationMethodDefOf.DHondtPR;
            return SeatAllocationMethodDefOf.WinnerTakesAll;
        }

        public bool IsMinorityLanguage => !Game.IsMostCommonLanguage(Language);
        public bool IsMinorityReligion => Religion != ReligionDefOf.None && !Game.IsMostCommonReligion(Religion);


        /// <summary>
        /// Returns a descriptive label based on the attributes and traits of this district.
        /// </summary>
        public string GetDescripiveLabel()
        {
            List<string> adjectiveCandidates = new List<string>();
            List<string> describerCandidates = new List<string>();

            foreach (GeographyTrait trait in Geography)
            {
                if (trait.Category > 1)
                {
                    if (Random.value < 0.5f) adjectiveCandidates.Add($"{trait.Def.Adjective}");
                    else describerCandidates.Add($"{trait.Def.Describer}");
                }
            }

            if (Random.value < 0.5f) adjectiveCandidates.Add($"{Language.Label.ToLower()}-speaking");
            else describerCandidates.Add($"with a {Language.Label.ToLower()}-speaking community");

            if (HasReligion)
            {
                if (Random.value < 0.5f) adjectiveCandidates.Add($"{Religion.Label.ToLower()}");
                else describerCandidates.Add($"following {Religion.Noun}");
            }
            else
            {
                if (Random.value < 0.5f) adjectiveCandidates.Add($"atheistic");
                else describerCandidates.Add($"following no religion");
            }

            if (Random.value < 0.5f) adjectiveCandidates.Add($"{Density.Label.ToLower()}-density");
            else describerCandidates.Add($"with a {Density.Label.ToLower()} density");

            if (Random.value < 0.5f) adjectiveCandidates.Add($"{AgeGroup.Label} dominated");
            else describerCandidates.Add($"with a {AgeGroup.Adjective} population");

            if (Random.value < 0.5f) adjectiveCandidates.Add($"{Economy1.Label}-focussed");
            else describerCandidates.Add($"with a big {Economy1.Label} industry");

            foreach (CulturalTrait trait in ActiveCulturalTraits)
            {
                bool hasAdjective = !string.IsNullOrEmpty(trait.Adjective);
                bool hasDescriber = !string.IsNullOrEmpty(trait.Describer);

                if (hasAdjective && hasDescriber)
                {
                    if (Random.value < 0.5f) adjectiveCandidates.Add($"{trait.Def.Adjective}");
                    else describerCandidates.Add($"{trait.Def.Describer}");
                }

                else if (hasAdjective) adjectiveCandidates.Add($"{trait.Def.Adjective}");
                else if (hasDescriber) describerCandidates.Add($"{trait.Def.Describer}");

                else continue;
            }

            bool showAdjective = Random.value < 0.8f;
            bool showDescriber = Random.value < 0.8f;

            string adjective = showAdjective ? adjectiveCandidates.RandomElement() + " " : "";
            string describer = showDescriber ? " " + describerCandidates.RandomElement() : "";

            return $"{adjective}district{describer}";
        }

        #endregion

    }
}
