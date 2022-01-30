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
        public GameType GameType;           // Singleplayer, Host, Client

        public PolygonMapGenerator PMG;
        public Map Map;
        public CameraHandler CameraHandler;
        public UI_ElectionTactics UI;
        public MenuNavigator MenuNavigator;

        private int StartGameSeed;
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

        #region Rules
        // Base
        private const int PlayerPolicyPointsPerCycle = 3;
        private const int MinAIPolicyPointsPerCycle = 4;
        private const int MaxAIPolicyPointsPerCycle = 7;
        private const int MaxPolicyValue = 8;

        // Turn Length
        private int BaseTimePerTurn;       // Time in seconds that players in multiplayer have time for their turn
        private int CumulativeTimePerTurn; // Additional time players have that increases each turn by this amount

        private Dictionary<GameSettings.TurnLengthOptions, int> BaseTimes = new Dictionary<GameSettings.TurnLengthOptions, int>()
        {
            {GameSettings.TurnLengthOptions.Slow, 160 },
            {GameSettings.TurnLengthOptions.Medium, 75 },
            {GameSettings.TurnLengthOptions.Fast, 50 },
        };
        private Dictionary<GameSettings.TurnLengthOptions, int> CumulativeTimes = new Dictionary<GameSettings.TurnLengthOptions, int>()
        {
            {GameSettings.TurnLengthOptions.Slow, 20 },
            {GameSettings.TurnLengthOptions.Medium, 15 },
            {GameSettings.TurnLengthOptions.Fast, 10 },
        };

        #endregion

        // Global game values
        public float TurnTime;      // How much time players have this turn for their actions
        public float RemainingTime; // How much time is remaining this turn

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

        /// <summary>
        /// Creating a new singleplayer game or multiplayer game as host
        /// </summary>
        public void InitNewGame(GameSettings gameSettings, GameType type)
        {
            if (State != GameState.Inactive) return;
            InitGame();

            GameSettings = gameSettings;
            ApplyGameSettings();

            GameType = type;
            int mapSeed = GetRandomSeed();
            StartGameSeed = GetRandomSeed();
            MapGenerationSettings settings = new MapGenerationSettings(mapSeed, 10, 10, 0.08f, 1.5f, 5, 30, MapType.Island);
            if (GameType == GameType.MultiplayerHost) NetworkPlayer.Server.GenerateMapServerRpc(mapSeed, StartGameSeed);
            PMG.GenerateMap(settings, callback: OnMapGenerationDone);
        }

        /// <summary>
        /// Joining a new game as a client
        /// </summary>
        public void InitJoinGame()
        {
            InitGame();

            GameSettings = new GameSettings(MenuNavigator.Lobby.Slots, MenuNavigator.Lobby.Rules.Select(x => x.value).ToList());
            ApplyGameSettings();

            GameType = GameType.MultiplayerClient;
        }

        /// <summary>
        /// Changes game rules variables according to the game settings
        /// </summary>
        private void ApplyGameSettings()
        {
            BaseTimePerTurn = BaseTimes[GameSettings.TurnLength];
            CumulativeTimePerTurn = CumulativeTimes[GameSettings.TurnLength];
        }

        /// <summary>
        /// Initialization independent of single/multiplayer
        /// </summary>
        private void InitGame()
        {
            State = GameState.Loading;
            UI.Init(this);
            UI.LoadingScreen.gameObject.SetActive(true);
        }

        public void StartGameAsClient(int mapSeed, int startGameSeed)
        {
            StartGameSeed = startGameSeed;
            MapGenerationSettings settings = new MapGenerationSettings(mapSeed, 10, 10, 0.08f, 1.5f, 5, 30, MapType.Island);
            PMG.GenerateMap(settings, callback: OnMapGenerationDone);
        }

        private void OnMapGenerationDone(Map map)
        {
            Map = map;
            StartGame(StartGameSeed);
        }

        /// <summary>
        /// Sets up all traits, mentalities, parties, policies etc and starts the game. This happens after map generation for all clients.
        /// </summary>
        /// <param name="seed">Seed is used to synchronize in multiplayer</param>
        public void StartGame(int seed)
        {
            UnityEngine.Random.InitState(seed);

            UI.LoadingScreen.gameObject.SetActive(false);
            Year = 1999;
            
            Map.InitializeMap(showRegionBorders: true, showShorelineBorders: true, showContinentBorders: false, showWaterConnections: false, MapColorMode.Basic, MapTextureMode.None);
            UI.MapControls.Init(this, MapDisplayMode.NoOverlay);

            InitGeograhyTraits();
            InitMentalities();
            InitParties();
            InitPolicies();
            UI.SidePanelFooter.Init(this);

            MarkovChainWordGenerator.Init();
            Constitution = new Constitution(this);
            UI.Constitution.Init(Constitution);
            StartNextElectionCycle(); // Start first cycle
            SetAllDistrictsVisible();
            CameraHandler.FocusDistricts(VisibleDistricts.Values.ToList());
            State = GameState.PreparationPhase;

            UI.SelectTab(Tab.Parliament);
            ElectionAnimationHandler = new ElectionAnimationHandler(this);
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
            int id = 0;
            foreach(LobbySlot slot in GameSettings.Slots)
            {
                if (slot.SlotType == LobbySlotType.Free || slot.SlotType == LobbySlotType.Inactive) continue;
                Party party = new Party(this, id++, slot.Name, slot.GetColor(), isAi: slot.SlotType == LobbySlotType.Bot);
                if (slot.SlotType == LobbySlotType.Human && slot.ClientId == NetworkPlayer.LocalClientId) LocalPlayerParty = party;
                Parties.Add(party);
            }
            Debug.Log("Local player id is " + LocalPlayerParty.Id);
        }

        /// <summary>
        /// Creates all possible policies for all parties.
        /// They are all initially inactive and get set to active once a district containing a trait matching that policy appears.
        /// </summary>
        private void InitPolicies()
        {
            int policyId = 0;

            foreach (GeographyTraitType t in Enum.GetValues(typeof(GeographyTraitType)))
            {
                foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(policyId++, p, t, MaxPolicyValue));
            }

            foreach (EconomyTrait t in Enum.GetValues(typeof(EconomyTrait)))
            {
                foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(policyId++, p, t, MaxPolicyValue));
            }

            foreach (Density t in Enum.GetValues(typeof(Density)))
            {
                foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(policyId++, p, t, MaxPolicyValue));
            }

            foreach (AgeGroup t in Enum.GetValues(typeof(AgeGroup)))
            {
                foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(policyId++, p, t, MaxPolicyValue));
            }

            foreach (Language t in Enum.GetValues(typeof(Language)))
            {
                foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(policyId++, p, t, MaxPolicyValue));
            }

            foreach (Religion t in Enum.GetValues(typeof(Religion)))
            {
                foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(policyId++, p, t, MaxPolicyValue));
            }
        }

        #endregion

        #region Game Flow

        // Update is called once per frame
        void Update()
        {
            // Timer
            if(GameType != GameType.Singleplayer && (State == GameState.PreparationPhase || State == GameState.Election))
            {
                UI.SidePanelFooter.UpdateButton();

                if (!LocalPlayerParty.IsReady)
                {
                    RemainingTime -= Time.deltaTime;
                    if (RemainingTime <= 0)
                    {
                        RemainingTime = 0f;
                        EndTurn();
                    }
                }
            }

            switch(State)
            {
                case GameState.PreparationPhase:
                    // Mouse click
                    if (Input.GetMouseButtonDown(0))
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

        private void ResetTurnTimer()
        {
            TurnTime = BaseTimePerTurn + ElectionCycle * CumulativeTimePerTurn;
            RemainingTime = TurnTime;
        }

        /// <summary>
        /// Handles everything related to starting a new election cycle:
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
            foreach(Party p in Parties)
            {
                if(p.IsHuman) AddPolicyPoints(p, PlayerPolicyPointsPerCycle);
                else AddPolicyPoints(p, UnityEngine.Random.Range(MinAIPolicyPointsPerCycle, MaxAIPolicyPointsPerCycle + 1));
            }

            // Mark all human players as not ready for next turn
            foreach (Party p in Parties.Where(x => x.IsHuman)) p.IsReady = false;

            // Reset countdown
            ResetTurnTimer();
        }

        /// <summary>
        /// Clientside function that gets executed when the "End Turn" button is pressed.
        /// </summary>
        public void EndTurn()
        {
            Debug.Log("Ending Turn");

            UI.SidePanelFooter.SetBackgroundColor(ColorManager.Singleton.UiInteractableDisabled);

            if (GameType == GameType.Singleplayer) ConcludePreparationPhase(GetRandomSeed());
            else // Multiplayer
            {
                if (LocalPlayerParty.IsReady) return;
                LocalPlayerParty.IsReady = true;
                RemainingTime = 0f;
                NetworkPlayer.Server.EndTurnServerRpc(LocalPlayerParty.Id);
            }
        }

        /// <summary>
        /// That gets executed on all clients (including host) whenever a player hits the "End Turn" button. When all players are ready, the preparation phase concludes.
        /// </summary>
        public void OnPlayerReady(int playerId)
        {
            Party party = GetParty(playerId);
            Debug.Log("Player " + party.Name + " (" + party.Id + ") is ready.");
            party.IsReady = true;

            if(GameType == GameType.MultiplayerHost)
            {
                if (Parties.Where(x => x.IsHuman).All(x => x.IsReady)) ConcludePreparationPhaseServer();
            }
            
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
            return CreateDistrict(new DistrictData(r.Id, seed, name));
        }

        /// <summary>
        /// Creates an invisible district given the seed, name and region on the map. To make it visible call SetAllDistrictsVisible().
        /// </summary>
        public District CreateDistrict(DistrictData data)
        {
            Region region = Map.Regions.First(x => x.Id == data.RegionId);
            District newDistrict = new District(data.Seed, this, region, data.Name);
            UI_DistrictLabel label = Instantiate(UI.DistrictLabelPrefab);
            label.gameObject.SetActive(false);
            label.Init(newDistrict);
            newDistrict.MapLabel = label;
            newDistrict.OrderId = Districts.Count;
            newDistrict.SetVisible(false);

            Districts.Add(region, newDistrict);

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
            // TODO: Instead of creating new policies, just change the active flag

            foreach (District d in Districts.Values)
            {
                foreach (GeographyTraitType t in d.Geography.Select(x => x.Type))
                {
                    if (!ActiveGeographyTraits.Contains(t))
                    {
                        ActiveGeographyTraits.Add(t);
                        foreach (Party p in Parties) p.GetPolicy(t).Activate();
                    }
                }

                if (!ActiveEconomyTraits.Contains(d.Economy1))
                {
                    ActiveEconomyTraits.Add(d.Economy1);
                    foreach (Party p in Parties) p.GetPolicy(d.Economy1).Activate();
                }
                if (!ActiveEconomyTraits.Contains(d.Economy2))
                {
                    ActiveEconomyTraits.Add(d.Economy2);
                    foreach (Party p in Parties) p.GetPolicy(d.Economy2).Activate();
                }
                if (!ActiveEconomyTraits.Contains(d.Economy3))
                {
                    ActiveEconomyTraits.Add(d.Economy3);
                    foreach (Party p in Parties) p.GetPolicy(d.Economy3).Activate();
                }

                if (!ActiveDensityTraits.Contains(d.Density))
                {
                    ActiveDensityTraits.Add(d.Density);
                    foreach (Party p in Parties) p.GetPolicy(d.Density).Activate();
                }
                if (!ActiveAgeGroupTraits.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupTraits.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.GetPolicy(d.AgeGroup).Activate();
                }
                if (!ActiveLanguageTraits.Contains(d.Language))
                {
                    ActiveLanguageTraits.Add(d.Language);
                    foreach (Party p in Parties) p.GetPolicy(d.Language).Activate();
                }
                if (!ActiveReligionTraits.Contains(d.Religion) && d.Religion != Religion.None)
                {
                    ActiveReligionTraits.Add(d.Religion);
                    foreach (Party p in Parties) p.GetPolicy(d.Religion).Activate();
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
        // This chapter contains all functions that players can trigger in their turn through their actions.

        public void IncreasePolicy(Policy p)
        {
            if (GameType == GameType.Singleplayer) DoIncreasePolicy(LocalPlayerParty.Id, p.Id);
            else NetworkPlayer.Server.ChangePolicyServerRpc(LocalPlayerParty.Id, p.Id, 1);
        }

        public void DecreasePolicy(Policy p)
        {
            if (GameType == GameType.Singleplayer) DoDecreasePolicy(LocalPlayerParty.Id, p.Id);
            else NetworkPlayer.Server.ChangePolicyServerRpc(LocalPlayerParty.Id, p.Id, -1);
        }

        #endregion

        #region Game Value Changes

        public void AddPolicyPoints(Party p, int amount)
        {
            p.PolicyPoints += amount;
            UI.SidePanelHeader.UpdateValues(this);
        }

        public void AddModifier(District d, Modifier mod)
        {
            d.Modifiers.Add(mod);
            mod.SetDistrict(d);
        }

        public void DoIncreasePolicy(int playerId, int policyId)
        {
            Party party = GetParty(playerId);
            Policy policy = party.GetPolicy(policyId);

            if (party.PolicyPoints == 0 || policy.Value == policy.MaxValue) return;
            Debug.Log(party.Name + " increased " + policy.Name + " policy.");
            
            party.PolicyPoints--;
            policy.IncreaseValue();
            if(party == LocalPlayerParty) UI.SidePanelHeader.UpdateValues(this);
        }
        public void DoDecreasePolicy(int playerId, int policyId)
        {
            Party party = GetParty(playerId);
            Policy policy = party.GetPolicy(policyId);

            if (policy.Value == policy.LockedValue) return;
            Debug.Log(party.Name + " decreased " + policy.Name + " policy.");

            party.PolicyPoints++;
            policy.DecreaseValue();
            if (party == LocalPlayerParty) UI.SidePanelHeader.UpdateValues(this);
        }

        #endregion

        #region Election

        /// <summary>
        /// Only used in multiplayer, a seed is sended to all clients with a signal that the preparation phase ended.
        /// </summary>
        private void ConcludePreparationPhaseServer()
        {
            int seed = GetRandomSeed();
            NetworkPlayer.Server.ConcludePreparationPhaseServerRpc(seed);
        }

        /// <summary>
        /// This is a client-side function, a seed ensures that the outcome is the same for all clients.
        /// Concludes the preparation phase when all players are ready and handles everything that happens:
        /// - AI actions
        /// - Locking player actions
        /// - Election result
        /// - Updating party values (seats, total seats, etc.)
        /// - Handling district effects
        /// - Starting next election cycle
        /// </summary>
        /// <param name="seed">Seed is used to synchronize in multiplayer</param>
        public void ConcludePreparationPhase(int seed)
        {
            UnityEngine.Random.InitState(seed);

            // Change state
            if (State != GameState.PreparationPhase) return;
            State = GameState.Election;

            // AI policies
            foreach (Party p in Parties.Where(p => !p.IsHuman))
                p.AI.DistributePolicyPoints();

            // Lock policies
            foreach (Party party in Parties)
                foreach (Policy policy in party.Policies) policy.LockValue();

            // Reset seats
            foreach (Party p in Parties) p.Seats = 0;

            // Create and apply election result
            List<DistrictElectionResult> districtResults = new List<DistrictElectionResult>();
            foreach (District district in VisibleDistricts.Values)
            {
                DistrictElectionResult districtResult = district.RunElection(Parties);
                districtResults.Add(districtResult);
            }
            GeneralElectionResult electionResult = new GeneralElectionResult(ElectionCycle, Year, districtResults);
            electionResult.Apply(this);

            // District effects
            foreach (District d in VisibleDistricts.Values) d.OnElectionEnd();

            // Handle next election start
            StartNextElectionCycle();

            ElectionAnimationHandler.StartAnimation(electionResult);
        }

        public void AddGeneralElectionResult(GeneralElectionResult electionResult)
        {
            ElectionResults.Add(electionResult);
        }

        /// <summary>
        /// Gets triggered when the election animation is done
        /// </summary>
        public void OnElectionAnimationDone()
        {
            // Check win condition
            if (GameType != GameType.MultiplayerClient && HandleWinConditions()) return;

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
        public static int GetRandomSeed()
        {
            return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        #endregion

        #region Getters

        public GeographyTrait GetGeographyTrait(GeographyTraitType type, int category)
        {
            return GeographyTraits.First(x => x.Type == type && x.Category == category);
        }

        public Party GetParty(int partyId)
        {
            return Parties.First(x => x.Id == partyId);
        }

        public Dictionary<Region, District> VisibleDistricts { get { return Districts.Where(x => x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }
        public Dictionary<Region, District> InvisibleDistricts { get { return Districts.Where(x => !x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }
        public GeneralElectionResult GetLatestElectionResult() { return ElectionResults.Last(); }

        #endregion
    }
}
