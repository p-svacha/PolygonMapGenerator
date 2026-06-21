using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class ElectionTacticsGame : MonoBehaviour
    {
        public static ElectionTacticsGame Instance;

        public GameSettings GameSettings { get; private set; }
        public GameType GameType;           // Singleplayer, Host, Client

        public PolygonMapGenerator PMG;
        public Map Map;
        public CameraHandler CameraHandler;
        public UI_ElectionTactics UI;
        public VfxManager VfxManager;
        public MenuNavigator MenuNavigator;

        private int StartGameSeed;
        public GameState State;

        // Constitution
        public Constitution Constitution;

        // Districts
        private Dictionary<Region, District> Districts = new Dictionary<Region, District>();   // Contains districts that will be added after the election animation
        public District Capital { get; set; }

        // Parties
        public List<Party> Parties = new List<Party>();
        public Party LocalPlayerParty { get; set; }
        public Party WinnerParty;

        // Events (newspaper articles)
        public List<RandomEvent> RandomEvents = new List<RandomEvent>();
        public List<NewsEvent> NewsEvents = new List<NewsEvent>();
        public Newspaper CurrentNewspaper;
        public List<Newspaper> Newspapers = new List<Newspaper>(); // index corresponds to cycle, 1 per cycle

        // Traits
        public List<GeographyTrait> GeographyTraits = new List<GeographyTrait>();
        public List<GeographyTraitDef> ActiveGeographyTraits = new List<GeographyTraitDef>();

        public List<EconomicSectorDef> ActiveEconomicSectors = new List<EconomicSectorDef>();
        public List<DensityDef> ActiveDensityTraits = new List<DensityDef>();
        public List<AgeGroupDef> ActiveAgeGroupTraits = new List<AgeGroupDef>();
        public List<LanguageDef> ActiveLanguageTraits = new List<LanguageDef>();
        public List<ReligionDef> ActiveReligionTraits = new List<ReligionDef>();
        public List<District> ActiveDistrictTraits = new List<District>();
        // Constant Rules
        public const int PP_PER_CYCLE = 4;
        public const int MAX_POLICY_VALUE = 8;

        public const int STANDARD_MODIFIER_VALUE = 30;

        public const int BR_START_LEGITIMACY = 100;
        public const int BR_DMG_PER_UNWON_SEAT = 1; // How much legitimacy a party loses for not winning a seat
        public const int BR_HEAL_PER_WON_SEAT = 1; // How much legitimacy a party gains for winning a seat
        public const int BR_BASE_HEAL_PER_ELECTION_WON = 0; // Base healing amount from winning elections
        public const int BR_HEAL_PER_ELECTION_WON_PER_TURN = 2; // The healing from won elections increases by this every election.

        public const int MAX_CULTURAL_TRAITS = 4;

        public const float MIN_BASE_GROWTH_RATE = -0.5f; // in %
        public const float MAX_BASE_GROWTH_RATE = +1f; // in %

        // Global game values
        public float TurnTime;      // How much time players have this turn for their actions
        public float RemainingTime; // How much time is remaining this turn

        // General Election
        public int ElectionCycle;
        public int Year;
        private List<GeneralElectionResult> ElectionResults = new List<GeneralElectionResult>();
        public ElectionAnimationHandler ElectionAnimationHandler;

        #region Initialization

        private void Awake()
        {
            // Vote experiment
            /*
            ExecuteVotingExperiment(3000, 10);
            ExecuteVotingExperiment(222, 10);
            */

            Instance = this;

            ResourceManager.ClearCache();
            MarkovChainWordGenerator.Init();

            // Initialize Defs
            DefDatabaseRegistry.ClearAllDatabases();
            DefDatabase<TurnLengthDef>.AddDefs(TurnLengthDefs.Defs);
            DefDatabase<GameModeDef>.AddDefs(GameModeDefs.Defs);
            DefDatabase<BotDifficultyDef>.AddDefs(BotDifficultyDefs.Defs);
            DefDatabase<GameLengthDef>.AddDefs(GameLengthDefs.Defs);
            DefDatabase<StartingDistrictsDef>.AddDefs(StartingDistrictsDefs.Defs);
            DefDatabase<GeographyTraitDef>.AddDefs(GeographyTraitDefs.Defs);
            DefDatabase<CulturalTraitCategoryDef>.AddDefs(CulturalTraitCategoryDefs.Defs);
            DefDatabase<CulturalTraitDef>.AddDefs(CulturalTraitDefs.Defs);
            DefDatabase<AgeGroupDef>.AddDefs(AgeGroupDefs.Defs);
            DefDatabase<LanguageDef>.AddDefs(LanguageDefs.Defs);
            DefDatabase<ReligionDef>.AddDefs(ReligionDefs.Defs);
            DefDatabase<DensityDef>.AddDefs(DensityDefs.Defs);
            DefDatabase<EconomicSectorDef>.AddDefs(EconomicSectorDefs.Defs);
            DefDatabase<RandomEventDef>.AddDefs(RandomEventDefs.Defs);
            DefDatabase<SeatAllocationMethodDef>.AddDefs(SeatAllocationMethodDefs.Defs);
            DefDatabase<SeatDistributionGameSettingDef>.AddDefs(SeatDistributionGameSettingDefs.Defs);
            DefDatabase<RandomEventFrequencyDef>.AddDefs(RandomEventFrequencyDefs.Defs);
            DefDatabaseRegistry.ResolveAllReferences();
            DefDatabaseRegistry.OnLoadingDone();

            UI.Init(this);
        }

        private void ExecuteVotingExperiment(int numVotes, int numRuns)
        {
            Dictionary<string, int> experimentDic = new Dictionary<string, int>()
            {
                { "One", 20 },
                { "Two", 20 },
                { "Three", 20 },
                { "Four", 20 },
                { "Five", 20 },
            };

            List<float> minValues = new List<float>();
            List<float> maxValues = new List<float>();

            for (int i = 0; i < numRuns; i++)
            {
                Dictionary<string, int> picked = new Dictionary<string, int>();
                foreach (string s in experimentDic.Keys) picked.Add(s, 0);
                for (int v = 0; v < numVotes; v++)
                {
                    string selected = experimentDic.GetWeightedRandomElement();
                    picked[selected]++;
                }
                List<float> ratios = new List<float>();
                foreach (var x in picked)
                {
                    float ratio = (float)x.Value / (float)numVotes;
                    ratios.Add(ratio);
                }
                minValues.Add(ratios.Min());
                maxValues.Add(ratios.Max());
            }
            Debug.Log($"Maximum spread for {numVotes} votes across {numRuns} runs with 5 parties is {minValues.Min()} - {maxValues.Max()}. Average Spread is {minValues.Average()} - {maxValues.Average()}.");
        }

        void Start()
        {
            AudioManager.StartMusic();
            State = GameState.Inactive;
        }

        /// <summary>
        /// Creating a new singleplayer game or multiplayer game as host
        /// </summary>
        public void InitNewGame(GameSettings gameSettings, GameType type)
        {
            if (State != GameState.Inactive) ExitAndDestroyCurrentGame();
            InitGame();

            GameSettings = gameSettings;
            Debug.Log("Initializing new game with settings: " + gameSettings);

            GameType = type;
            int mapSeed = GetRandomSeed();
            StartGameSeed = GetRandomSeed();
            MapGenerationSettings mapGenSettings = new MapGenerationSettings(mapSeed, 10, 10, 0.08f, 1.5f, 5, 30, MapType.Island);
            if (GameType == GameType.MultiplayerHost) NetworkPlayer.Server.GenerateMapServerRpc(mapSeed, StartGameSeed);

            // mapGenSettings.Seed = ####;
            PMG.GenerateMap(mapGenSettings, callback: OnMapGenerationDone);
        }

        /// <summary>
        /// Joining a new game as a client
        /// </summary>
        public void InitJoinGame()
        {
            if (State != GameState.Inactive) ExitAndDestroyCurrentGame();
            InitGame();

            GameSettings = new GameSettings();

            GameType = GameType.MultiplayerClient;
        }

        /// <summary>
        /// Initialization independent of single/multiplayer
        /// </summary>
        private void InitGame()
        {
            // Load
            State = GameState.Loading;
            CameraHandler.Init(this);
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
            Debug.Log($"Starting a {GameType} game with seed {seed}.");

            UnityEngine.Random.InitState(seed);

            UI.LoadingScreen.gameObject.SetActive(false);
            Year = 1999;
            ElectionCycle = 0;

            Map.InitializeMap(showRegionBorders: true, showShorelineBorders: true, showContinentBorders: false, showWaterConnections: false, MapColorMode.Basic, MapTextureMode.MinorNoise);
            UI.MapControls.Init(this, MapDisplayMode.NoOverlay, DistrictLabelMode.Default);
            VfxManager.Init(this);

            // Init elements
            InitGeograhyTraits();
            InitParties();
            InitDistricts();
            InitPolicies();
            UI.SidePanelFooter.Init(this);

            // Add starting districts (-1 because 1 is added in StartNextElectionCycle)
            int numStartingDistricts = GetNumStartingDistricts();
            for (int i = 0; i < numStartingDistricts - 1; i++)
            {
                Districts.Values.ToList()[i].Activate();
            }

            // Assign a random capital district
            Capital = Districts.Values.Take(10).ToList().RandomElement();
            if (IsTutorialEnabled) Capital = Districts.Values.ToArray()[7]; // To make sure the earliest districts are not capital
            Capital.AddCulturalTrait(CulturalTraitDefOf.Capital);

            // Battle royale
            foreach (Party party in Parties) party.Legitimacy = (int)(BR_START_LEGITIMACY * GameSettings.GameLength.ModifierFactor);

            Constitution = new Constitution(this);
            UI.Constitution.Init(Constitution);
            StartNextElectionCycle(); // Start first cycle
            SetAllActiveDistrictsVisible();
            CameraHandler.FocusDistricts(VisibleDistricts.Values.ToList());
            State = GameState.PreparationPhase;

            UI.SelectTab(Tab.DistrictList);
            UI.StandingsPanel.Init(GetCurrentStandings(), dynamic: true);
            ElectionAnimationHandler = new ElectionAnimationHandler(this);

            // Tutorial
            if (GameSettings.IsTutorialEnabled) TutorialManager.Instance.StartTutorial();
            else TutorialManager.Instance.EndTutorial();
        }

        private int GetNumStartingDistricts()
        {
            int numStartingDistricts = GameSettings.StartingDistricts.Value;
            if (numStartingDistricts == -1) numStartingDistricts = Map.LandRegions.Count;
            return numStartingDistricts;
        }

        private void InitGeograhyTraits()
        {
            GeographyTraits.Clear();
            foreach (GeographyTraitDef def in DefDatabase<GeographyTraitDef>.AllDefs)
            {
                GeographyTraits.Add(new GeographyTrait(def, 1));
                GeographyTraits.Add(new GeographyTrait(def, 2));
                GeographyTraits.Add(new GeographyTrait(def, 3));
            }
        }

        private void InitParties()
        {
            Parties.Clear();

            int id = 0;
            foreach (LobbySlot slot in GameSettings.Slots)
            {
                if (slot.SlotType == LobbySlotType.Free || slot.SlotType == LobbySlotType.Inactive) continue;

                Color partyColor = slot.GetColor();
                Color partyTextColor = PartyNameGenerator.GetPartyTextColor(slot.GetColor());

                Party party = new Party(this, id++, slot.Name, partyColor, partyTextColor, isAi: slot.SlotType == LobbySlotType.Bot);

                if (slot.SlotType == LobbySlotType.Human && (GameType == GameType.Singleplayer || slot.ClientId == NetworkPlayer.LocalClientId))
                {
                    LocalPlayerParty = party;
                }


                LocalPlayerParty.IsLocalPlayer = true;
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
            int localId = 0;

            foreach (District district in Districts.Values)
            {
                foreach (Party p in Parties) p.AddPolicy(new DistrictPolicy(policyId++, localId, p, district, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (GeographyTraitDef def in DefDatabase<GeographyTraitDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (EconomicSectorDef def in DefDatabase<EconomicSectorDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (DensityDef def in DefDatabase<DensityDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (AgeGroupDef def in DefDatabase<AgeGroupDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (LanguageDef def in DefDatabase<LanguageDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            foreach (ReligionDef def in DefDatabase<ReligionDef>.AllDefs)
            {
                foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(policyId++, localId, p, def, MAX_POLICY_VALUE));
                localId++;
            }

            // Init bot policy weights
            foreach (Party p in Parties.Where(p => !p.IsHuman)) p.AI.InitRandomPolicyWeights();
        }

        /// <summary>
        /// Initializes all districts that can initially appear in the game. All except for the first one will be hidden in the beginning.
        /// </summary>
        private void InitDistricts()
        {
            Districts.Clear();

            for (int i = 0; i < Map.LandRegions.Count; i++)
            {
                AddRandomDistrict();
            }
        }

        #endregion

        #region Game Flow

        // Update is called once per frame
        void Update()
        {
            // Timer
            if (GameType != GameType.Singleplayer && (State == GameState.PreparationPhase || State == GameState.Election))
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

            switch (State)
            {
                case GameState.Loading:
                    string loadingStepText = PMG.GetCurrentStateString() + "...";
                    if (PMG.GenerationState == MapGenerationState.GenerationDone) loadingStepText = "Creating Districts...";
                    if (UI_LoadingScreen.Instance.LoadingScreenStepText.text != loadingStepText)
                    {
                        UI_LoadingScreen.Instance.LoadingScreenStepText.text = loadingStepText;
                    }
                    break;

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
            TurnTime = GameSettings.TurnLength.BaseTurnTime + ElectionCycle * GameSettings.TurnLength.TurnLengthIncreasePerTurn;
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
            Debug.Log($"Starting cycle {ElectionCycle} in Year {Year}.");

            // Add new district
            int numStartingDistricts = GetNumStartingDistricts();
            int indexToActivate = ElectionCycle + numStartingDistricts - 2; // -2 because cycle starts at 1 and we already have starting districts
            if (indexToActivate < Districts.Count)
            {
                District districtToActivate = Districts.Values.ToList()[indexToActivate];
                districtToActivate.Activate();
                RegisterNewsEvent(new NewsEvent_DistrictAdded(districtToActivate));
            }

            // Update stuff according to district state changes
            UpdateDistrictAges();
            UpdateActivePolicies();

            // Add policy points to all players
            foreach (Party p in Parties)
            {
                if (p.IsHuman) AddPolicyPoints(p, PP_PER_CYCLE);
                else AddPolicyPoints(p, UnityEngine.Random.Range(GameSettings.BotDifficulty.MinPolicyPointsPerCycle, GameSettings.BotDifficulty.MaxPolicyPointsPerCycle + 1));
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

            // If tutorial is still active and not in first election stage, end it
            if (TutorialManager.Instance.IsTutorialActive && TutorialManager.Instance.CurrentStep != TutorialManager.TutorialStep.Election)
            {
                TutorialManager.Instance.EndTutorial();
            }

            // Audio
            AudioManager.PlaySound(AudioManager.Instance.Gong, volume: 0.45f);
            AudioManager.SwitchToTrack(AudioManager.Instance.ElectionTrack, fadeDuration: 1f);

            // Disable footer
            UI.SidePanelFooter.SetBackgroundColor(ColorManager.Instance.UiInteractableDisabled);

            // Start election
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

            if (GameType == GameType.MultiplayerHost)
            {
                if (Parties.Where(x => x.IsHuman).All(x => x.IsReady)) ConcludePreparationPhaseServer();
            }

        }

        /// <summary>
        /// Checks if any party reached a win condition and returns true if one has.
        /// </summary>
        private bool HandleWinConditions()
        {
            if (IsClassicMode)
            {
                Party winner = Constitution.WinCondition.GetWinner();
                if (winner != null)
                {
                    WinnerParty = winner;
                    EndGame();
                    return true;
                }
            }
            else if (IsBattleRoyale)
            {
                // Eliminate parties with non-positive legitimacy
                List<Party> remainingParties = Parties.Where(p => !p.IsEliminated).ToList();
                List<Party> newlyEliminatedParties = remainingParties.Where(p => p.Legitimacy <= 0).OrderBy(p => p.Legitimacy).ToList();

                int nextEliminationRank = remainingParties.Count;
                while (newlyEliminatedParties.Count > 0)
                {
                    newlyEliminatedParties.First().Eliminate(nextEliminationRank);
                    nextEliminationRank--;
                    newlyEliminatedParties.RemoveAt(0);
                }

                // Update UI
                UI.StandingsPanel.Init(GetCurrentStandings(), dynamic: true);
                UI.SelectTab(Tab.Parliament); // Refresh parliament UI

                // If only one left, assign it as the winner (and eliminate it)
                remainingParties = Parties.Where(p => !p.IsEliminated).ToList();
                if (remainingParties.Count == 1) remainingParties[0].Eliminate(rank: 1);

                // If all parties are eliminated, end the game
                remainingParties = Parties.Where(p => !p.IsEliminated).ToList();
                if (remainingParties.Count == 0)
                {
                    List<Party> finalStandings = Parties.OrderBy(p => p.FinalRank).ToList();
                    WinnerParty = finalStandings.First();
                    EndGame();
                    return true;
                }

                // End the game is the player is eliminated
                if (LocalPlayerParty.IsEliminated)
                {
                    WinnerParty = null;
                    EndGame();
                    return true;
                }
            }

            return false;
        }

        private void EndGame()
        {
            State = GameState.Done;
            UI.PostGameScreen.Init(this);
        }

        /// <summary>
        /// Completely exits and destroys everything of the current game. Should not be called while in a game, only when going back to the main menu or starting a new game.
        /// </summary>
        public void ExitAndDestroyCurrentGame()
        {
            // Reset some values
            Parties.Clear();
            Districts.Clear();
            VisibleDistricts.Clear();

            ActiveDistricts.Clear();
            ActiveEconomicSectors.Clear();
            ActiveGeographyTraits.Clear();
            ActiveDistrictTraits.Clear();
            ActiveAgeGroupTraits.Clear();
            ActiveReligionTraits.Clear();
            ActiveLanguageTraits.Clear();
            ActiveDensityTraits.Clear();

            ElectionResults.Clear();
            GeographyTraits.Clear();
            ElectionResults.Clear();
            WinnerParty = null;

            Map.DestroyAllGameObjects();
            UI.DestroyCurrentGame();
            State = GameState.Inactive;
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
            int index = Districts.Count;
            Region region = Map.Regions.First(x => x.Id == data.RegionId);
            District newDistrict = new District(data.Seed, this, region, data.Name, index);
            UI_DistrictLabel label = Instantiate(UI.DistrictLabelPrefab, UI.OverlayContainer.transform);
            label.gameObject.SetActive(false);
            label.Init(newDistrict);
            newDistrict.MapLabel = label;

            Districts.Add(region, newDistrict);

            return newDistrict;
        }

        public void SetAllActiveDistrictsVisible()
        {
            foreach (District d in ActiveDistricts) d.SetVisible(true);
            UI.MapControls.RefreshMapDisplay();
        }

        /// <summary>
        /// Adds a new inactive district to the game.
        /// </summary>
        private void AddRandomDistrict()
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
                foreach (Region r in Map.LandRegions.Where(x => !Districts.Keys.Contains(x)))
                {
                    int activeNeighbours = r.Neighbours.Where(x => Districts.Keys.Contains(x)).Count();
                    int totalNeighbours = r.Neighbours.Count;
                    float ratio = 1f * activeNeighbours / totalNeighbours;
                    if (ratio == highestRatio) candidates.Add(r);
                    else if (ratio > highestRatio)
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

        public CulturalTraitDef GetRandomAdoptableCulturalTraitDef(District district)
        {

            Dictionary<CulturalTraitDef, int> candidates = new Dictionary<CulturalTraitDef, int>();
            foreach (CulturalTraitDef def in DefDatabase<CulturalTraitDef>.AllDefs)
            {
                bool canAdopt = true;

                // Exclusion criteria
                if (def.Commonness <= 0) canAdopt = false;
                if (Districts.Count < 2 && def.RequiresMultipleDistricts) canAdopt = false;
                if (district.CulturalTraits.Any(t => t.Def == def)) canAdopt = false;
                if (district.CulturalTraits.Any(t => def.ForbiddenCulturalTraits.Contains(t.Def.DefName))) canAdopt = false;
                if (def.RequiresReligion && district.Religion == ReligionDefOf.None) canAdopt = false;
                if (district.CulturalTraits.Any(t => t.Def.IsSeatDistributionTrait && def.IsSeatDistributionTrait)) canAdopt = false;

                if (canAdopt) candidates.Add(def, def.Commonness);
            }

            return candidates.GetWeightedRandomElement();
        }

        private void UpdateActivePolicies()
        {
            foreach (District d in ActiveDistricts)
            {
                if (!ActiveDistrictTraits.Contains(d))
                {
                    ActiveDistrictTraits.Add(d);
                    foreach (Party p in Parties) p.GetPolicy(d).Activate();
                }

                foreach (GeographyTraitDef t in d.Geography.Select(x => x.Def))
                {
                    if (!ActiveGeographyTraits.Contains(t))
                    {
                        ActiveGeographyTraits.Add(t);
                        foreach (Party p in Parties) p.GetPolicy(t).Activate();
                    }
                }

                if (!ActiveEconomicSectors.Contains(d.Economy1))
                {
                    ActiveEconomicSectors.Add(d.Economy1);
                    foreach (Party p in Parties) p.GetPolicy(d.Economy1).Activate();
                }
                if (!ActiveEconomicSectors.Contains(d.Economy2))
                {
                    ActiveEconomicSectors.Add(d.Economy2);
                    foreach (Party p in Parties) p.GetPolicy(d.Economy2).Activate();
                }
                if (!ActiveEconomicSectors.Contains(d.Economy3))
                {
                    ActiveEconomicSectors.Add(d.Economy3);
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
                if (!ActiveReligionTraits.Contains(d.Religion) && d.Religion != ReligionDefOf.None)
                {
                    ActiveReligionTraits.Add(d.Religion);
                    foreach (Party p in Parties) p.GetPolicy(d.Religion).Activate();
                }
            }
        }

        private void UpdateDistrictAges()
        {
            foreach (District d in ActiveDistricts)
            {
                GeographyTrait coreTrait = d.Geography.FirstOrDefault(x => x.Def == GeographyTraitDefOf.Core);
                if (coreTrait != null) d.Geography.Remove(coreTrait);
                GeographyTrait newTrait = d.Geography.FirstOrDefault(x => x.Def == GeographyTraitDefOf.New);
                if (newTrait != null) d.Geography.Remove(newTrait);

                int numStartingDistricts = GetNumStartingDistricts();
                if (numStartingDistricts > 3) numStartingDistricts = 3; // Make sure not more than 3 districts get the core 3 trait

                if (d.Index < numStartingDistricts) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.Core, 3)); // Starting districts get the Core III trait
                else if (d.Index == numStartingDistricts) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.Core, 2)); // Following district the Core II
                else if (d.Index == numStartingDistricts + 1) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.Core, 1)); // Following district the Core I

                int numDistricts = ActiveDistricts.Count;
                if (d.Index < numStartingDistricts) { } // Starting districts (Core III) districts never get the new trait
                else if (numDistricts - d.Index - 1 < 2) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.New, 3)); // 2 newest districts get New III
                else if (numDistricts - d.Index - 1 < 4) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.New, 2)); // 3rd and 4th newest districts get New II
                else if (numDistricts - d.Index - 1 < 6) d.Geography.Add(GetGeographyTrait(GeographyTraitDefOf.New, 1)); // 5th and 6th newest districts get New I
            }
        }

        #endregion

        #region Game Commands
        // This chapter contains all functions that local players can trigger in their turn through their actions.

        public bool CanIncreaseLocalPlayerPolicy(Policy p)
        {
            if (LocalPlayerParty.PolicyPoints == 0) return false;
            if (p.Value == p.MaxValue) return false;

            return true;
        }

        public void IncreaseLocalPlayerPolicy(Policy p)
        {
            if (!CanIncreaseLocalPlayerPolicy(p)) return;

            foreach (District d in ActiveDistricts) VfxManager.ShowDistrictPopularityImpactParticles(d, p.GetSinglePointPopularityDelta(d));

            if (GameType == GameType.Singleplayer) DoIncreasePolicy(LocalPlayerParty.Id, p.Id);
            else NetworkPlayer.Server.ChangePolicyServerRpc(LocalPlayerParty.Id, p.Id, 1);
        }


        public bool CanDecreaseLocalPlayerPolicy(Policy p)
        {
            if (p.Value == p.LockedValue) return false;

            return true;
        }

        public void DecreaseLocalPlayerPolicy(Policy p)
        {
            if (!CanDecreaseLocalPlayerPolicy(p)) return;

            foreach (District d in ActiveDistricts) VfxManager.ShowDistrictPopularityImpactParticles(d, p.GetSinglePointPopularityDelta(d, isIncrease: false));

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
            mod.SetDistrict(d);

            // Check if modifier can be combined with existing
            Modifier existingIdenticalModifier = d.Modifiers.FirstOrDefault(m => m.RemainingLength == mod.RemainingLength && m.Description == mod.Description && m.Source == mod.Source && m.PartyId == mod.PartyId && m.RegionId == mod.RegionId);

            if (existingIdenticalModifier != null)
            {
                existingIdenticalModifier.ModifyValue(mod.Value);
            }

            // Else just add it
            else d.Modifiers.Add(mod);
        }

        public void DoIncreasePolicy(int playerId, int policyId)
        {
            Party party = GetParty(playerId);
            Policy policy = party.GetPolicy(policyId);

            if (party.PolicyPoints == 0 || policy.Value == policy.MaxValue) return;
            // Debug.Log($"{party.Name} increased {policy.Name} policy. It is now at {policy.Value}/{policy.MaxValue}.");

            party.PolicyPoints--;
            policy.IncreaseValue();
            if (party == LocalPlayerParty)
            {
                UI.SidePanelHeader.UpdateValues(this);
                UI.MapControls.RefreshMapDisplay();
            }
        }
        public void DoDecreasePolicy(int playerId, int policyId)
        {
            Party party = GetParty(playerId);
            Policy policy = party.GetPolicy(policyId);

            if (policy.Value == policy.LockedValue) return;
            Debug.Log(party.Name + " decreased " + policy.Name + " policy.");

            party.PolicyPoints++;
            policy.DecreaseValue();
            if (party == LocalPlayerParty)
            {
                UI.SidePanelHeader.UpdateValues(this);
                UI.MapControls.RefreshMapDisplay();
            }
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

            // Generate and apply election result
            List<DistrictElectionResult> districtResults = new List<DistrictElectionResult>();
            foreach (District district in VisibleDistricts.Values)
            {
                DistrictElectionResult districtResult = district.RunElection(NonEliminatedParties);
                districtResults.Add(districtResult);
            }
            GeneralElectionResult electionResult = new GeneralElectionResult(ElectionCycle, Year, districtResults);
            electionResult.Apply(this);

            // District effects
            foreach (District d in VisibleDistricts.Values) d.OnElectionEnd();

            // Random events
            ExecuteRandomEvent();

            // Handle next election start
            StartNextElectionCycle();

            // Generate newspaper (after everything else happened so we have all information)
            CurrentNewspaper = NewspaperGenerator.GenerateYearNewspaper();
            Newspapers.Add(CurrentNewspaper);

            // Start election animation
            ElectionAnimationHandler.StartAnimation(electionResult);
        }

        private void ExecuteRandomEvent()
        {
            if (UnityEngine.Random.value > GameSettings.RandomEventFrequency.RandomEventChance) return;

            Dictionary<RandomEvent, int> eventCandidates = new Dictionary<RandomEvent, int>();
            foreach (RandomEventDef def in DefDatabase<RandomEventDef>.AllDefs)
            {
                RandomEvent eventCandidate = (RandomEvent)System.Activator.CreateInstance(def.RandomEventClass);
                eventCandidate.Init(def);
                if (!eventCandidate.CanExecute()) continue;

                eventCandidates.Add(eventCandidate, def.Commonness);
            }

            RandomEvent chosenEvent = eventCandidates.GetWeightedRandomElement();
            chosenEvent.Execute();
            RandomEvents.Add(chosenEvent);
        }

        public void RegisterNewsEvent(NewsEvent newsEvent)
        {
            NewsEvents.Add(newsEvent);
        }

        public void AddGeneralElectionResult(GeneralElectionResult electionResult)
        {
            ElectionResults.Add(electionResult);
        }

        /// <summary>
        /// Gets triggered when the election animation is done.
        /// </summary>
        public void OnElectionAnimationDone()
        {
            // Check win condition
            if (HandleWinConditions()) return;

            State = GameState.PreparationPhase;

            // Update all map labels
            foreach (District d in ActiveDistricts) d.MapLabel.Refresh();

            // Show the newspaper
            UI.Newspaper.ShowNewspaper(CurrentNewspaper, withAnimation: true);
            CurrentNewspaper = null;
        }

        #endregion

        #region Random Values

        public static DensityDef GetRandomDensity()
        {
            return DefDatabase<DensityDef>.AllDefs.RandomElement();
        }
        public static EconomicSectorDef GetRandomEconomicSector()
        {
            return DefDatabase<EconomicSectorDef>.AllDefs.RandomElement();
        }
        public static LanguageDef GetRandomLanguage()
        {
            return DefDatabase<LanguageDef>.AllDefs.RandomElement();
        }
        public static ReligionDef GetRandomReligion()
        {
            return DefDatabase<ReligionDef>.AllDefs.RandomElement();
        }
        public string GetRandomDistrictName()
        {
            string name = "";
            do
            {
                name = MarkovChainWordGenerator.GenerateWord("Province", 4, 10);
                name = name.CapitalizeEachWord();
            } while (Districts.Values.Any(d => d.Name == name));

            return name;
        }
        public static int GetRandomSeed()
        {
            return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        #endregion

        #region Getters

        public List<Party> NonEliminatedParties => Parties.Where(p => !p.IsEliminated).ToList();

        public bool HasDistrict(Region r) => Districts.ContainsKey(r);
        public District GetDistrict(Region r) => Districts.ContainsKey(r) ? Districts[r] : null;
        public List<District> AllDistricts => Districts.Values.ToList();
        public List<District> ActiveDistricts => Districts.Values.Where(d => d.IsActive).ToList();

        public bool IsClassicMode => GameSettings.GameMode == GameModeDefOf.Classic;
        public bool IsBattleRoyale => GameSettings.GameMode == GameModeDefOf.BattleRoyale;

        public GeographyTrait GetGeographyTrait(GeographyTraitDef def, int category)
        {
            return GeographyTraits.First(x => x.Def == def && x.Category == category);
        }

        public Party GetParty(int partyId)
        {
            return Parties.First(x => x.Id == partyId);
        }

        public Dictionary<Region, District> VisibleDistricts { get { return Districts.Where(x => x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }
        public Dictionary<Region, District> InvisibleDistricts { get { return Districts.Where(x => !x.Value.IsVisible).ToDictionary(x => x.Key, x => x.Value); } }
        public GeneralElectionResult GetLatestElectionResult() { return ElectionResults.Last(); }

        public List<GeneralElectionResult> GetAllElectionResults() => ElectionResults.ToList();

        public int GetNextElectionNumSeats() => ActiveDistricts.Sum(d => d.GetSeats());

        /// <summary>
        /// Returns the current standings as an ordered dictionary with the value representing the party score (varies by gamemode).
        /// </summary>
        public Dictionary<Party, int> GetCurrentStandings()
        {
            return Parties.OrderBy(p => p.FinalRank).ThenByDescending(p => p.GetGameScore()).ToDictionary(p => p, p => p.GetGameScore());
        }

        /// <summary>
        /// Returns the most common languages in the currently active districts.
        /// </summary>
        public List<LanguageDef> GetMostCommonLanguages()
        {
            if (ActiveDistricts.Count == 0) return new List<LanguageDef>();

            Dictionary<LanguageDef, int> counts = new Dictionary<LanguageDef, int>();
            foreach (District d in ActiveDistricts)
                counts.Increment(d.Language);

            int maxCount = counts.Values.Max();
            return counts.Where(x => x.Value == maxCount).Select(x => x.Key).ToList();
        }
        public bool IsMostCommonLanguage(LanguageDef language) => GetMostCommonLanguages().Contains(language);

        /// <summary>
        /// Returns the most common religions in the currently active districts.
        /// </summary>
        public List<ReligionDef> GetMostCommonReligions()
        {
            if (ActiveDistricts.Count == 0) return new List<ReligionDef>();

            Dictionary<ReligionDef, int> counts = new Dictionary<ReligionDef, int>();
            foreach (District d in ActiveDistricts)
                if (d.Religion != ReligionDefOf.None)
                    counts.Increment(d.Religion);

            if (counts.Count == 0) return new List<ReligionDef>();

            int maxCount = counts.Values.Max();
            return counts.Where(x => x.Value == maxCount).Select(x => x.Key).ToList();
        }
        public bool IsMostCommonReligion(ReligionDef religion) => GetMostCommonReligions().Contains(religion);

        public bool IsTutorialEnabled => GameSettings.IsTutorialEnabled;

        #endregion
    }
}
