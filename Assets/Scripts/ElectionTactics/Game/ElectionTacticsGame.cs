using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class ElectionTacticsGame : MonoBehaviour
    {
        private GameSettings GameSettings;  // Static rules of the game (only server needs this to initialize the game)
        private GameType GameType;          // Singleplayer, Host, Client

        public PolygonMapGenerator PMG;
        public Map Map;
        public CameraHandler CameraHandler;
        public UI_ElectionTactics UI;
        public MenuNavigator MenuNavigator;

        public GameState State;

        // Constitution
        public Constitution Constitution;

        // Districts
        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();   // Contains districts that will be added after the election animation

        // Parties
        public List<Party> Parties = new List<Party>();
        public Party LocalPlayerParty;
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
        // private const int NumOpponents = 3; // Defined by game settings
        private const int MaxPolicyValue = 8;

        // General Election
        public int ElectionCycle;
        public int Year;
        private List<GeneralElectionResult> ElectionResults = new List<GeneralElectionResult>();
        public ElectionAnimationHandler ElectionAnimationHandler;

        #region Initialization

        void Start()
        {
            State = GameState.Inactive;
        }

        public void InitNewGame(GameSettings gameSettings, GameType type)
        {
            if (State != GameState.Inactive) return;
            State = GameState.Loading;
            UI.LoadingScreen.gameObject.SetActive(true);
            GameSettings = gameSettings;
            GameType = type;
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            MapGenerationSettings settings = new MapGenerationSettings(seed, 10, 10, 0.08f, 1.5f, 5, 30, MapType.Island);
            if (GameType == GameType.MultiplayerHost) NetworkPlayer.Server.StartGameServerRpc(seed);
            PMG.GenerateMap(settings, callback: OnMapGenerationDone);
        }

        public void InitJoinGame()
        {
            State = GameState.Loading;
            UI.LoadingScreen.gameObject.SetActive(true);
            GameSettings = new GameSettings(MenuNavigator.Lobby.Slots);
            GameType = GameType.MultiplayerClient;
        }

        public void StartGameAsClient(int seed)
        {
            MapGenerationSettings settings = new MapGenerationSettings(seed, 10, 10, 0.08f, 1.5f, 5, 30, MapType.Island);
            PMG.GenerateMap(settings, callback: OnMapGenerationDone);
        }

        private void OnMapGenerationDone(Map map)
        {
            UI.LoadingScreen.gameObject.SetActive(false);
            Year = 1999;
            Map = map;
            Map.InitializeMap(showRegionBorders: true, showShorelineBorders: true, showContinentBorders: false, showWaterConnections: false, MapDrawMode.Basic);
            UI.MapControls.Init(this, MapDisplayMode.NoOverlay);

            InitGeograhyTraits();
            InitMentalities();
            InitParties();

            if (GameType != GameType.MultiplayerClient)
            {
                MarkovChainWordGenerator.Init();
                Constitution = new Constitution(this);
                UI.Constitution.Init(Constitution);
                StartNextElectionCycle(); // Start first cycle
                SetAllDistrictsVisible();
                CameraHandler.FocusDistricts(VisibleDistricts.Values.ToList());
            }

            UI.SelectTab(Tab.Parliament);
            ElectionAnimationHandler = new ElectionAnimationHandler(this);

            State = GameState.PreparationPhase;
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

        private void InitParties()
        {
            foreach(LobbySlot slot in GameSettings.Slots)
            {
                if (slot.SlotType == LobbySlotType.Free || slot.SlotType == LobbySlotType.Inactive) continue;
                Party party = new Party(this, slot.Name, slot.GetColor(), isAi: slot.SlotType == LobbySlotType.Bot);
                if (slot.SlotType == LobbySlotType.LocalPlayer) LocalPlayerParty = party;
                Parties.Add(party);
            }
        }

        #endregion

        #region Game Flow

        // Update is called once per frame
        void Update()
        {
            switch(State)
            {
                case GameState.PreparationPhase:
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
                                if (VisibleDistricts.ContainsKey(mouseRegion)) UI.SelectDistrict(VisibleDistricts[mouseRegion]);
                                else UI.SelectDistrict(null);
                            }
                        }
                    }
                    break;

                case GameState.Election:
                    ElectionAnimationHandler.Update();
                    break;
            }
        }

        /// <summary>
        /// Serverside function. Handles everything related to starting a new election cycle:
        /// - Incrementing year/cycle
        /// - Adding new district
        /// - Distributing policy and campaign points to players for the next phase
        /// </summary>
        private void StartNextElectionCycle()
        {
            // Go to next year
            ElectionCycle++;
            Year++;

            // Add new district
            AddRandomDistrict();

            // Add policy points to all players
            AddPolicyPoints(LocalPlayerParty, PlayerPolicyPointsPerCycle);
            foreach (Party p in Parties.Where(x => x.AI != null))
            {
                int addedPolicyPoints = UnityEngine.Random.Range(MinAIPolicyPointsPerCycle, MaxAIPolicyPointsPerCycle + 1);
                AddPolicyPoints(p, addedPolicyPoints);
            }

            if (GameType == GameType.MultiplayerHost) NetworkPlayer.Server.InitCycleServerRpc();
        }

        /// <summary>
        /// Clientside function to start an election cycle. All data comes from the server.
        /// </summary>
        public void StartNextElectionCycleClient(UnityEngine.Random.State districtSeed, string districtName, int regionId)
        {
            CreateDistrict(districtSeed, districtName, Map.Regions.First(x => x.Id == regionId));
            UI.MapControls.RefreshMapDisplay();
            SetAllDistrictsVisible();
            CameraHandler.FocusDistricts(VisibleDistricts.Values.ToList());
        }

        /// <summary>
        /// Checks if any party reached a win condition and returns true if one has.
        /// </summary>
        private bool HandleWinConditions()
        {
            Party winner = Constitution.WinCondition.GetWinner();
            if (winner != null)
            {
                WinnerParty = winner;
                UI.PostGameScreen.Init(this);
                return true;
            }
            return false;
        }

        #endregion

        #region Map Evolution

        /// <summary>
        /// Server-side function. Creates a new random district in the given region.
        /// </summary>
        private District CreateDistrict(Region r)
        {
            string name = GetRandomDistrictName();
            UnityEngine.Random.State seed = UnityEngine.Random.state;
            return CreateDistrict(seed, name, r);
        }

        /// <summary>
        /// Creates an invisible district given the seed, name and region on the map. To make it visible call SetAllDistrictsVisible().
        /// </summary>
        public District CreateDistrict(UnityEngine.Random.State seed, string name, Region r)
        {
            District newDistrict = new District(seed, this, r, name);
            UI_DistrictLabel label = Instantiate(UI.DistrictLabelPrefab);
            label.gameObject.SetActive(false);
            label.Init(newDistrict);
            newDistrict.MapLabel = label;
            newDistrict.OrderId = Districts.Count;
            newDistrict.SetVisible(false);

            Districts.Add(r, newDistrict);

            UpdateDistrictAges();
            UpdateActivePolicies();

            return newDistrict;
        }

        public void SetAllDistrictsVisible()
        {
            foreach (District d in Districts.Values) d.SetVisible(true);
            UI.MapControls.RefreshMapDisplay();
        }

        public void AddRandomDistrict()
        {
            if (Districts.Count == 0)
            {
                Region randomRegion = Map.LandRegions[UnityEngine.Random.Range(0, Map.LandRegions.Count)];
                CreateDistrict(randomRegion);
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
                CreateDistrict(chosenRegion);
            }
        }

        public Mentality GetRandomAdoptableMentality(District d)
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
            UI.SidePanelHeader.UpdateValues(this);
        }

        public void IncreasePolicy(Policy p)
        {
            if (p.Party.PolicyPoints == 0 || p.Value == p.MaxValue) return;
            p.Party.PolicyPoints--;
            p.IncreaseValue();
            UI.SidePanelHeader.UpdateValues(this);
        }
        public void DecreasePolicy(Policy p)
        {
            if (p.Value == p.LockedValue) return;
            p.Party.PolicyPoints++;
            p.DecreaseValue();
            UI.SidePanelHeader.UpdateValues(this);
        }

        public void AddModifier(District d, Modifier mod)
        {
            d.Modifiers.Add(mod);
            mod.District = d;
        }

        #endregion

        #region Election

        /// <summary>
        /// This is a server-side function. Concludes the preparation phase and handles everything that happens:
        /// - AI actions
        /// - Locking player actions
        /// - Election result
        /// - Updating party values (seats, total seats, etc.)
        /// - Handling district effects
        /// - Starting next election cycle
        /// </summary>
        public void ConcludePreparationPhase()
        {
            if (State != GameState.PreparationPhase) return;
            State = GameState.Election;

            // AI policies
            foreach (Party p in Parties.Where(p => p != LocalPlayerParty))
                p.AI.DistributePolicyPoints();

            // Lock policies
            foreach (Party party in Parties)
                foreach (Policy policy in party.Policies) policy.LockValue();

            // Reset seats
            foreach (Party p in Parties) p.Seats = 0;

            // Election result
            List<DistrictElectionResult> districtResults = new List<DistrictElectionResult>();
            foreach (District district in VisibleDistricts.Values)
            {
                DistrictElectionResult districtResult = district.RunElection(Parties);

                districtResult.Winner.Seats += district.Seats;
                districtResult.Winner.TotalSeatsWon += district.Seats;
                districtResult.Winner.TotalDistrictsWon++;
                foreach (Party p in Parties) p.TotalVotes += districtResult.Votes[p];
                districtResults.Add(districtResult);
            }

            GeneralElectionResult electionResult = new GeneralElectionResult(ElectionCycle, Year, districtResults);
            ElectionResults.Add(electionResult);

            List<Party> winnerParties = Parties.Where(x => x.Seats == Parties.Max(y => y.Seats)).ToList();
            foreach (Party p in winnerParties) p.TotalElectionsWon++;

            // District effects
            foreach (District d in VisibleDistricts.Values) d.OnElectionEnd();

            // Handle next election start
            StartNextElectionCycle();

            ElectionAnimationHandler.StartAnimation(electionResult);
        }

        public void OnElectionAnimationDone()
        {
            // Check win condition
            if (HandleWinConditions()) return;

            State = GameState.PreparationPhase;
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
        public static string GetRandomDistrictName()
        {
            return MarkovChainWordGenerator.GenerateWord("Province", 4, 10);
        }

        #endregion

        #region Getters

        public GeographyTrait GetGeographyTrait(GeographyTraitType type, int category)
        {
            return GeographyTraits.First(x => x.Type == type && x.Category == category);
        }

        public Dictionary<Region, District> VisibleDistricts { get { return Districts.Where(x => x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }
        public Dictionary<Region, District> InvisibleDistricts { get { return Districts.Where(x => !x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }

        #endregion
    }
}
