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

        // Parties
        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();
        public List<Party> Parties = new List<Party>();
        public Party PlayerParty;
        public List<Party> OpponentParties = new List<Party>();
        public Party WinnerParty;

        // Policies
        public List<GeographyTrait> ActiveGeographyPolicies = new List<GeographyTrait>();
        public List<EconomyTrait> ActiveEconomyPolicies = new List<EconomyTrait>();
        public List<Density> ActiveDensityPolicies = new List<Density>();
        public List<AgeGroup> ActiveAgeGroupPolicies = new List<AgeGroup>();
        public List<Language> ActiveLanguagePolicies = new List<Language>();
        public List<Religion> ActiveReligionPolicies = new List<Religion>();

        // Rules
        private const int PlayerPolicyPointsPerCycle = 2;
        private const int AIPolicyPointsPerCycle = 4;
        private const int NumOpponents = 3;
        private const int MaxPolicyValue = 8;
        private const int ElectionsToWin = 1;

        // General Election
        public int ElectionCycle;
        private float HeaderSlideTime = 1f;
        private float DistrictPanTime = 2f;
        private float PostDistrictPanTime = 1f;
        private float GraphAnimationTime = 6f;
        private float PostGraphPauseTime = 2.5f;
        private float ListAnimationTime = 1f;
        private float PostListAnimationTime = 1f;

        private List<District> ElectionOrder;
        private int CurElectionDistrictIndex;
        private District CurElectionDistrict;


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
        }

        private void OnMapGenerated()
        {
            Map = PMG.Map;
            UI.MapControls.Init(this, MapDisplayMode.NoOverlay);

            CreateParties();

            UI.SelectTab(Tab.Parliament);
            StartElectionCycle();
            CameraHandler.FocusDistricts(Districts.Values.ToList());

            State = GameState.Running;
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
            AddRandomDistrict();
            AddPolicyPoints(PlayerParty, PlayerPolicyPointsPerCycle);
            foreach (Party p in OpponentParties) AddPolicyPoints(p, AIPolicyPointsPerCycle);

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

        #region Update Values

        private void UpdateActivePolicies()
        {
            foreach(District d in Districts.Values)
            {
                foreach(GeographyTrait t in d.Geography)
                {
                    if(!ActiveGeographyPolicies.Contains(t))
                    {
                        ActiveGeographyPolicies.Add(t);
                        foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(p, t, MaxPolicyValue));
                    }
                }

                if (!ActiveEconomyPolicies.Contains(d.Economy1))
                {
                    ActiveEconomyPolicies.Add(d.Economy1);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy1, MaxPolicyValue));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy2))
                {
                    ActiveEconomyPolicies.Add(d.Economy2);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy2, MaxPolicyValue));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy3))
                {
                    ActiveEconomyPolicies.Add(d.Economy3);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy3, MaxPolicyValue));
                }

                if (!ActiveDensityPolicies.Contains(d.Density))
                {
                    ActiveDensityPolicies.Add(d.Density);
                    foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(p, d.Density, MaxPolicyValue));
                }
                if (!ActiveAgeGroupPolicies.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupPolicies.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(p, d.AgeGroup, MaxPolicyValue));
                }
                if (!ActiveLanguagePolicies.Contains(d.Language))
                {
                    ActiveLanguagePolicies.Add(d.Language);
                    foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(p, d.Language, MaxPolicyValue));
                }
                if (!ActiveReligionPolicies.Contains(d.Religion))
                {
                    ActiveReligionPolicies.Add(d.Religion);
                    foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(p, d.Religion, MaxPolicyValue));
                }
            }
        }

        private District AddDistrict(Region r)
        {
            Density density = GetDensityForRegion(r);
            AgeGroup ageGroup = GetAgeGroupForRegion(r);
            Language language = GetLanguageForRegion(r);
            Religion religion = GetReligionForRegion(r);
            District newDistrict = new District(r, density, ageGroup, language, religion);
            
            Districts.Add(r, newDistrict);
            UI.MapControls.UpdateMapDisplay();
            UpdateActivePolicies();
            return newDistrict;
        }

        #endregion

        #region Map Evolution

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
            if (region.LandNeighbours.Count == 0) return GetRandomLanguage();

            List<Language> languageChances = new List<Language>();
            foreach(Region r in region.LandNeighbours)
            {
                Language l;
                if (Districts.ContainsKey(r)) l = Districts[r].Language;
                else l = GetRandomLanguage();

                languageChances.Add(l);
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

            if (CurElectionDistrict != null) CurElectionDistrict.Region.Unhighlight();

            if (CurElectionDistrictIndex < ElectionOrder.Count)
            {
                CurElectionDistrict = ElectionOrder[CurElectionDistrictIndex];
                CurElectionDistrict.Region.Highlight(ColorManager.Colors.SelectedDistrictColor);

                UI.Parliament.CurrentElectionContainer.SetActive(true);
                UI.Parliament.CurrentElectionTitle.text = CurElectionDistrict.Name;
                UI.Parliament.CurrentElectionSeatsText.text = CurElectionDistrict.Seats.ToString();
                UI.Parliament.CurrentElectionSeatsIcon.gameObject.SetActive(true);
                if(CurElectionDistrict.LastElectionResult != null)
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
            CurElectionDistrict.RunElection(PlayerParty, Parties);
            CurElectionDistrict.CurrentWinnerParty.Seats += CurElectionDistrict.Seats;

            List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
            foreach (KeyValuePair<Party, float> kvp in CurElectionDistrict.LastElectionResult.VoteShare)
                dataPoints.Add(new GraphDataPoint(kvp.Key.Acronym, kvp.Value, kvp.Key.Color));
            int yMax = (((int)CurElectionDistrict.LastElectionResult.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;

            UI.Parliament.CurrentElectionGraph.ShowAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, UI.Font, GraphAnimationTime);
            CurElectionDistrictIndex++;
            Invoke(nameof(UpdatePartyList), GraphAnimationTime + PostGraphPauseTime);
            Invoke(nameof(UnhighlightPartyList), GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + 0.1f);
            Invoke(nameof(MoveToNextElectionDistrict), GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + PostListAnimationTime);
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

        private void EndElection()
        {
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

        public static Mentality GetRandomCulture()
        {
            Array values = Enum.GetValues(typeof(Mentality));
            return (Mentality)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
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
    }
}
