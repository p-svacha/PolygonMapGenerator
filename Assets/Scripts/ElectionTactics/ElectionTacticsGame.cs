using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class ElectionTacticsGame : MonoBehaviour
    {
        public PolygonMapGenerator PMG;
        public Map Map;
        public CameraHandler CameraHandler;
        public UI_ElectionTactics UI;

        public GameState State;

        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();

        // Parties
        public List<Party> Parties = new List<Party>();
        public Party PlayerParty;
        public List<Party> OpponentParties = new List<Party>();
        public Party WinnerParty;

        // Traits
        public List<GeographyTrait> GeographyTraits = new List<GeographyTrait>();
        public List<GeographyTraitType> ActiveGeographyTraits = new List<GeographyTraitType>();

        public List<EconomyTrait> ActiveEconomyTraits = new List<EconomyTrait>();
        public List<Density> ActiveDensityTraits = new List<Density>();
        public List<AgeGroup> ActiveAgeGroupTraits = new List<AgeGroup>();
        public List<Language> ActiveLanguageTraits = new List<Language>();
        public List<Religion> ActiveReligionTraits = new List<Religion>();

        public List<Mentality> Mentalities = new List<Mentality>();

        // Rules
        private const int PlayerPolicyPointsPerCycle = 3;
        private const int MinAIPolicyPointsPerCycle = 4;
        private const int MaxAIPolicyPointsPerCycle = 7;
        private const int NumOpponents = 3;
        private const int MaxPolicyValue = 8;
        private const int ElectionsToWin = 5;

        // General Election
        public int ElectionCycle;
        public int Year;
        private float HeaderSlideTime = 1f;

        private float DistrictPanTime = 2f;
        private float PostDistrictPanTime = 1f; // Length of pause after moving to a district

        private float ModifierSlideTime = 1f; // Length of slide animation per modifier
        private float PostModifierSlideTime = 1.5f; // Length of pause after a modifier

        private float GraphAnimationTime = 6f;
        private float PostGraphPauseTime = 2f;
        private float ListAnimationTime = 1f;
        private float PostListAnimationTime = 1f;

        private const float SpeedModifier = 0.75f;

        private List<District> ElectionOrder;
        private int CurElectionDistrictIndex;
        private District CurElectionDistrict;
        private int CurModifierIndex;


        #region Initialization

        // Start is called before the first frame update
        void Start()
        {
            StartGame();
            State = GameState.Loading;
        }

        private void StartGame()
        {
            PMG.GenerateMap(10, 10, 0.08f, 1.5f, island: true, ColorManager.Colors.InactiveDistrictColor, ColorManager.Colors.WaterColor, drawRegionBorders: true, callback: OnMapGenerated);

            HeaderSlideTime *= SpeedModifier;
            DistrictPanTime *= SpeedModifier;
            PostDistrictPanTime *= SpeedModifier;
            GraphAnimationTime *= SpeedModifier;
            PostGraphPauseTime *= SpeedModifier;
            ListAnimationTime *= SpeedModifier;
            PostListAnimationTime *= SpeedModifier;
        }

        private void OnMapGenerated()
        {
            Year = 1999;
            Map = PMG.Map;
            UI.MapControls.Init(this, MapDisplayMode.NoOverlay);

            InitGeograhyTraits();
            InitMentalities();

            CreateParties();

            UI.SelectTab(Tab.Parliament);
            StartElectionCycle();
            CameraHandler.FocusDistricts(Districts.Values.ToList());

            State = GameState.Running;
        }

        private void InitGeograhyTraits()
        {
            GeographyTraits.Clear();
            foreach(GeographyTraitType t in  Enum.GetValues(typeof(GeographyTraitType)))
            {
                GeographyTraits.Add(new GeographyTrait(t, 1));
                GeographyTraits.Add(new GeographyTrait(t, 2));
                GeographyTraits.Add(new GeographyTrait(t, 3));
            }
        }

        private void InitMentalities()
        {
            Mentalities.Clear();
            foreach(MentalityType mt in Enum.GetValues(typeof(MentalityType)))
            {
                Mentalities.Add(new Mentality(mt));
            }
        }

        private void CreateParties()
        {
            List<Color> takenColors = new List<Color>();
            Color color = PartyNameGenerator.GetPartyColor(name, takenColors);
            PlayerParty = new Party(this, "Player Party", color, isAi: false);
            takenColors.Add(color);
            Parties.Add(PlayerParty);
            
            for(int i = 0; i < NumOpponents; i++)
            {
                string name = PartyNameGenerator.GetRandomPartyName(maxLength: 35);
                color = PartyNameGenerator.GetPartyColor(name, takenColors);
                takenColors.Add(color);
                Party p = new Party(this, name, color, isAi: true);
                Parties.Add(p);
                OpponentParties.Add(p);
            }

            UI.Standings.Init(Parties);
        }

        #endregion

        #region Game Flow

        // Update is called once per frame
        void Update()
        {
            switch(State)
            {
                case GameState.Running:
                if (Input.GetMouseButtonDown(0)) // Click
                {
                    bool uiClick = EventSystem.current.IsPointerOverGameObject();

                    if (!uiClick)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            Region mouseRegion = hit.transform.gameObject.GetComponent<Region>();
                            if (Districts.ContainsKey(mouseRegion)) UI.SelectDistrict(Districts[mouseRegion]);
                            else UI.SelectDistrict(null);
                        }
                    }
                }
                break;
            }


            /*
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(UI.SelectedDistrict != null)
                {
                    UI.SelectedDistrict.RunElection(PlayerParty, Parties);
                    UI.DistrictInfo.Init(UI.SelectedDistrict);
                }
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                AddRandomDistrict();
            }
            if(Input.GetKeyDown(KeyCode.S))
            {
                Parties[UnityEngine.Random.Range(0, Parties.Count)].Seats += UnityEngine.Random.Range(1, 11);
                UI.Parliament.PartyList.MovePositions(Parties, 1f);
            }
            */
        }

        private void StartElectionCycle()
        {
            // Start next cycle
            ElectionCycle++;
            Year++;
            AddRandomDistrict();
            AddPolicyPoints(PlayerParty, PlayerPolicyPointsPerCycle);
            foreach (Party p in OpponentParties)
            {
                int addedPolicyPoints = UnityEngine.Random.Range(MinAIPolicyPointsPerCycle, MaxAIPolicyPointsPerCycle + 1);
                AddPolicyPoints(p, addedPolicyPoints);
            }

            State = GameState.Running;
        }

        /// <summary>
        /// Checks if any party reached a win condition and returns true if one has.
        /// </summary>
        private bool HandleWinConditions()
        {
            if(Parties.Where(x => x.GamePoints >= ElectionsToWin).Count() > 0 && Parties.Where(x => x.GamePoints == Parties.Max(p => p.GamePoints)).Count() == 1)
            {
                WinnerParty = Parties.First(x => x.GamePoints == Parties.Max(p => p.GamePoints));
                UI.PostGameScreen.Init(this);
                return true;
            }
            return false;
        }

        #endregion

        #region Map Evolution

        private District AddDistrict(Region r)
        {
            Density density = GetDensityForRegion(r);
            AgeGroup ageGroup = GetAgeGroupForRegion(r);
            Language language = GetLanguageForRegion(r);
            Religion religion = GetReligionForRegion(r);
            District newDistrict = new District(this, r, density, ageGroup, language, religion);
            UI_DistrictLabel label = Instantiate(UI.DistrictLabelPrefab);
            label.Init(newDistrict);
            newDistrict.MapLabel = label;
            newDistrict.OrderId = Districts.Count;

            Districts.Add(r, newDistrict);
            UI.MapControls.UpdateMapDisplay();

            UpdateDistrictAges();
            UpdateActivePolicies();
            
            return newDistrict;
        }

        public void AddRandomDistrict()
        {
            if (Districts.Count == 0)
            {
                Region randomRegion = Map.LandRegions[UnityEngine.Random.Range(0, Map.LandRegions.Count)];
                AddDistrict(randomRegion);
                return;
            }
            else
            {
                // 1. Find regions which have the highest ratio of (neighbouring active districts : neighbouring inactive districts)
                List<Region> candidates = new List<Region>();
                float highestRatio = 0;
                foreach(Region r in Map.LandRegions.Where(x => !Districts.Keys.Contains(x)))
                {
                    int activeNeighbours = r.Neighbours.Where(x => Districts.Keys.Contains(x)).Count();
                    int totalNeighbours = r.Neighbours.Count;
                    float ratio = 1f * activeNeighbours / totalNeighbours;
                    if (ratio == highestRatio) candidates.Add(r);
                    else if(ratio > highestRatio)
                    {
                        candidates.Clear();
                        candidates.Add(r);
                        highestRatio = ratio;
                    }
                }

                // 2. Chose random candidate and calculate attributes
                Region chosenRegion = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                District addedDistrict = AddDistrict(chosenRegion);
            }
        }

        private Density GetDensityForRegion(Region region) // Denisty is weighted random rural > mixed > urban
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.25f) return Density.Urban; // Urban - 25% chance
            else if (rng < 0.25f + 0.33f) return Density.Mixed; // Mixed - 33% chance
            else return Density.Rural; // Rural  - 42 % chance
        }
        private AgeGroup GetAgeGroupForRegion(Region region) // Age group is fully random
        {
            return GetRandomAgeGroup();
        }
        private Language GetLanguageForRegion(Region region) // Languages can spread over land borders
        {
            List<Language> languageChances = new List<Language>();
            languageChances.Add(GetRandomLanguage());
            foreach(Region r in region.LandNeighbours)
            {
                if (Districts.ContainsKey(r)) languageChances.Add(Districts[r].Language);
            }
            return languageChances[UnityEngine.Random.Range(0, languageChances.Count)];
        }
        private Religion GetReligionForRegion(Region region) // Religion can spread over land and water
        {
            List<Religion> religionChances = new List<Religion>();
            foreach (Region r in region.Neighbours)
            {
                Religion religion;
                if (Districts.ContainsKey(r)) religion = Districts[r].Religion;
                else religion = GetRandomReligion();

                religionChances.Add(religion);
            }
            return religionChances[UnityEngine.Random.Range(0, religionChances.Count)];
        }

        public Mentality GetMentalityFor(District d)
        {
            List<Mentality> candidates = new List<Mentality>();
            foreach (Mentality m in Mentalities)
            {
                if (m.CanAdoptMentality(d)) candidates.Add(m);
            }
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private void UpdateActivePolicies()
        {
            foreach (District d in Districts.Values)
            {
                foreach (GeographyTraitType t in d.Geography.Select(x => x.Type))
                {
                    if (!ActiveGeographyTraits.Contains(t))
                    {
                        ActiveGeographyTraits.Add(t);
                        foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(p, t, MaxPolicyValue));
                    }
                }

                if (!ActiveEconomyTraits.Contains(d.Economy1))
                {
                    ActiveEconomyTraits.Add(d.Economy1);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy1, MaxPolicyValue));
                }
                if (!ActiveEconomyTraits.Contains(d.Economy2))
                {
                    ActiveEconomyTraits.Add(d.Economy2);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy2, MaxPolicyValue));
                }
                if (!ActiveEconomyTraits.Contains(d.Economy3))
                {
                    ActiveEconomyTraits.Add(d.Economy3);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy3, MaxPolicyValue));
                }

                if (!ActiveDensityTraits.Contains(d.Density))
                {
                    ActiveDensityTraits.Add(d.Density);
                    foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(p, d.Density, MaxPolicyValue));
                }
                if (!ActiveAgeGroupTraits.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupTraits.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(p, d.AgeGroup, MaxPolicyValue));
                }
                if (!ActiveLanguageTraits.Contains(d.Language))
                {
                    ActiveLanguageTraits.Add(d.Language);
                    foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(p, d.Language, MaxPolicyValue));
                }
                if (!ActiveReligionTraits.Contains(d.Religion) && d.Religion != Religion.None)
                {
                    ActiveReligionTraits.Add(d.Religion);
                    foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(p, d.Religion, MaxPolicyValue));
                }
            }
        }

        private void UpdateDistrictAges()
        {
            foreach(District d in Districts.Values)
            {
                GeographyTrait coreTrait = d.Geography.FirstOrDefault(x => x.Type == GeographyTraitType.Core);
                if (coreTrait != null) d.Geography.Remove(coreTrait);
                GeographyTrait newTrait = d.Geography.FirstOrDefault(x => x.Type == GeographyTraitType.New);
                if (newTrait != null) d.Geography.Remove(newTrait);

                if (d.OrderId < 2) d.Geography.Add(GetGeographyTrait(GeographyTraitType.Core, 3));
                else if (d.OrderId < 4) d.Geography.Add(GetGeographyTrait(GeographyTraitType.Core, 2));
                else if (d.OrderId < 6) d.Geography.Add(GetGeographyTrait(GeographyTraitType.Core, 1));

                int numDistricts = Districts.Count;
                if(numDistricts - d.OrderId - 1 < 2) d.Geography.Add(GetGeographyTrait(GeographyTraitType.New, 3));
                else if(numDistricts - d.OrderId - 1 < 4) d.Geography.Add(GetGeographyTrait(GeographyTraitType.New, 2));
                else if(numDistricts - d.OrderId - 1 < 6) d.Geography.Add(GetGeographyTrait(GeographyTraitType.New, 1));
            }
        }

    #endregion

        #region Game Commands

    public void AddPolicyPoints(Party p, int amount)
        {
            p.PolicyPoints += amount;
            UI.UpdatePolicyPointDisplay();
        }

        public void IncreasePolicy(Policy p)
        {
            if (p.Party.PolicyPoints == 0 || p.Value == p.MaxValue) return;
            p.Party.PolicyPoints--;
            p.IncreaseValue();
            UI.UpdatePolicyPointDisplay();
        }
        public void DecreasePolicy(Policy p)
        {
            if (p.Value == p.LockedValue) return;
            p.Party.PolicyPoints++;
            p.DecreaseValue();
            UI.UpdatePolicyPointDisplay();
        }

        public void AddGamePoint(Party party)
        {
            party.GamePoints++;
            UI.Standings.Init(Parties);
        }

        public void AddModifier(District d, Modifier mod)
        {
            d.Modifiers.Add(mod);
            mod.District = d;
        }

        #endregion

        #region Election

        public void RunGeneralElection()
        {
            if (State == GameState.GeneralElection) return;
            State = GameState.GeneralElection;

            // AI policies
            foreach (Party p in Parties.Where(p => p != PlayerParty))
                p.AI.DistributePolicyPoints();

            UI.SelectTab(Tab.Parliament);
            UI.MapControls.SetMapDisplayMode(MapDisplayMode.NoOverlay);

            // Lock policies
            foreach(Party party in Parties)
                foreach (Policy policy in party.Policies) policy.LockValue();

            // Reset seats
            foreach(Party p in Parties)
            {
                p.Seats = 0;
                UI.Parliament.PartyList.Init(Parties);
            }

            ElectionOrder = Districts.Values.OrderBy(x => x.Population).ToList();
            CurElectionDistrictIndex = 0;

            UI.SlideOutHeader(HeaderSlideTime);
            Invoke(nameof(MoveToNextElectionDistrict), HeaderSlideTime);
        }

        private void MoveToNextElectionDistrict()
        {
            UI.Parliament.CurrentElectionGraph.ClearGraph();
            UI.Parliament.ModifierSliderContainer.ClearContainer();

            if (CurElectionDistrict != null) CurElectionDistrict.Region.Unhighlight();

            if (CurElectionDistrictIndex < ElectionOrder.Count)
            {
                // Update current district
                CurElectionDistrict = ElectionOrder[CurElectionDistrictIndex];
                CurElectionDistrict.Region.Highlight(ColorManager.Colors.SelectedDistrictColor);

                // Update current election graph header
                if (CurElectionDistrict.ElectionResults.Count > 0)
                {
                    UI.Parliament.CurrentElectionMarginText.gameObject.SetActive(true);
                    UI.Parliament.LastElectionWinnerKnob.gameObject.SetActive(true);
                    UI.Parliament.CurrentElectionMarginText.text = (CurElectionDistrict.CurrentMargin > 0 ? "+" : "") + CurElectionDistrict.CurrentMargin.ToString("0.0") + " %";
                    UI.Parliament.LastElectionWinnerKnob.color = CurElectionDistrict.CurrentWinnerParty.Color;
                }
                else
                {
                    UI.Parliament.CurrentElectionMarginText.gameObject.SetActive(false);
                    UI.Parliament.LastElectionWinnerKnob.gameObject.SetActive(false);
                }

                // Get election results
                ElectionResult result = CurElectionDistrict.RunElection(PlayerParty, Parties);
                CurElectionDistrict.CurrentWinnerParty.Seats += CurElectionDistrict.Seats;

                List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
                foreach (KeyValuePair<Party, float> kvp in result.VoteShare)
                    dataPoints.Add(new GraphDataPoint(kvp.Key.Acronym, kvp.Value, kvp.Key.Color));
                int yMax = (((int)result.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;

                // Update current election graph
                UI.Parliament.CurrentElectionContainer.SetActive(true);
                UI.Parliament.CurrentElectionTitle.text = CurElectionDistrict.Name;
                UI.Parliament.CurrentElectionSeatsText.text = CurElectionDistrict.Seats.ToString();
                UI.Parliament.CurrentElectionSeatsIcon.gameObject.SetActive(true);
                UI.Parliament.CurrentElectionGraph.InitAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, UI.Font, GraphAnimationTime, startAnimation: false);
                
                CameraHandler.MoveToFocusDistricts(new List<District>() { CurElectionDistrict }, DistrictPanTime);
                Invoke(nameof(RunCurrentDistrictElection), DistrictPanTime + PostDistrictPanTime);
            }
            else
            {
                UI.Parliament.CurrentElectionContainer.SetActive(false);
                Invoke(nameof(EndElection), PostDistrictPanTime);
            }
        }

        private void RunCurrentDistrictElection()
        {
            CurModifierIndex = 0;

            float totalModifierDelay = CurElectionDistrict.Modifiers.Count * (ModifierSlideTime + PostModifierSlideTime);

            for(int i = 0; i < CurElectionDistrict.Modifiers.Count; i++)
            {
                float delay = i * (ModifierSlideTime + PostModifierSlideTime);
                Invoke(nameof(SlideInNextModifier), delay);
            }            

            CurElectionDistrictIndex++;
            Invoke(nameof(StartGraphAnimation), totalModifierDelay);
            Invoke(nameof(UpdatePartyList), totalModifierDelay + GraphAnimationTime + PostGraphPauseTime);
            Invoke(nameof(UnhighlightPartyList), totalModifierDelay + GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + 0.1f);
            Invoke(nameof(MoveToNextElectionDistrict), totalModifierDelay + GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + PostListAnimationTime);
        }

        private void UpdatePartyList()
        {
            UI.Parliament.PartyList.HighlightParty(CurElectionDistrict.CurrentWinnerParty);
            UI.Parliament.PartyList.MovePositions(Parties, ListAnimationTime);
        }

        private void UnhighlightPartyList()
        {
            UI.Parliament.PartyList.UnhighlightParty(CurElectionDistrict.CurrentWinnerParty);
        }

        private void StartGraphAnimation()
        {
            UI.Parliament.CurrentElectionGraph.StartInitAnimation();
        }

        private void SlideInNextModifier()
        {
            UI.Parliament.ModifierSliderContainer.SlideInModifier(CurElectionDistrict.Modifiers[CurModifierIndex], ModifierSlideTime);
            CurModifierIndex++;
        }

        private void EndElection()
        {
            // District actions
            foreach (District d in Districts.Values) d.OnElectionEnd();

            // Distribute Game points
            List<Party> winnerParties = Parties.Where(x => x.Seats == Parties.Max(y => y.Seats)).ToList();
            foreach (Party p in winnerParties) AddGamePoint(p);

            // Check win condition
            if (HandleWinConditions()) return;

            StartElectionCycle();
            CameraHandler.MoveToFocusDistricts(Districts.Values.ToList(), DistrictPanTime);
            UI.SlideInHeader(DistrictPanTime);
        }

        #endregion

        #region Random Values

        public static Density GetRandomDensity()
        {
            Array values = Enum.GetValues(typeof(Density));
            return (Density)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static EconomyTrait GetRandomEconomyTrait()
        {
            Array values = Enum.GetValues(typeof(EconomyTrait));
            return (EconomyTrait)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static Language GetRandomLanguage()
        {
            Array values = Enum.GetValues(typeof(Language));
            return (Language)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static Religion GetRandomReligion()
        {
            Array values = Enum.GetValues(typeof(Religion));
            return (Religion)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static AgeGroup GetRandomAgeGroup()
        {
            Array values = Enum.GetValues(typeof(AgeGroup));
            return (AgeGroup)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        #endregion

        #region Getters

        public GeographyTrait GetGeographyTrait(GeographyTraitType type, int category)
        {
            return GeographyTraits.First(x => x.Type == type && x.Category == category);
        }

        #endregion
    }
}
