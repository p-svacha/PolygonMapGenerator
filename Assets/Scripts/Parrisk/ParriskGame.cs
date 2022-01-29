using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class ParriskGame : MonoBehaviour
    {
        //Prefabs
        public VisualArmy ArmyPrefab;

        public PolygonMapGenerator PMG;
        public InputHandler InputManager;

        // Rule contants
        public const float BaseOffenseStrength = 100;
        public const float BaseDefenseStrength = 120;
        public const int BaseTroopsToDistribute = 5;

        // Rules
        public int NumPlayers;
        public int NumStartingTerritories;
        public int NumStartingArmies;

        // Game elements
        public Map Map;

        public Player LocalPlayer;
        public List<Player> Players = new List<Player>();
        public Dictionary<Region, Territory> Territories = new Dictionary<Region, Territory>();

        public List<List<Army>> TroopMovements = new List<List<Army>>(); // A list of all troop movement for every turn (index of list represents turn)
        public List<List<Battle>> Battles = new List<List<Battle>>();

        public int Turn;

        // State
        public GameState State;

        // Temporary Objects
        public List<ArmyArrow> ArmyArrows = new List<ArmyArrow>();

        // Animation Times
        public const float ArmyApproachTime = 10f;

        // UI
        public UI_ParriskGame UI;

        #region Initialization

        // Start is called before the first frame update
        void Start()
        {
            InitGame(8, 1, 10);
        }

        public void InitGame(int numPlayers, int numStartingTerritories, int numStartingArmies)
        {
            State = GameState.Initializing;
            NumPlayers = numPlayers;
            NumStartingTerritories = numStartingTerritories;
            NumStartingArmies = numStartingArmies;
            Turn = 0;

            MapGenerationSettings settings = new MapGenerationSettings(MapGenerationSettings.RandomSeed(), 15, 10, 0.4f, 2f, 3, 12, MapType.BigOceans);
            PMG.GenerateMap(settings, OnMapGenerationDone);
        }

        public void OnMapGenerationDone(Map map)
        {
            // Init map
            Map = map;
            Map.InitializeMap(true, true, true, true, MapColorMode.ParriskBoard, MapTextureMode.None);
            Map.FocusMapCentered();

            foreach(Region r in Map.LandRegions)
            {
                Territory regionTerritory = new Territory(r, "Giorgio");
                Territories.Add(r, regionTerritory);
            }

            // Init players
            LocalPlayer = new Player(this, "Local", ColorManager.GetColorByName("Green"), false);
            List<Color> playerColors = new List<Color>() { LocalPlayer.Color };
            Players.Add(LocalPlayer);
            for(int i = 0; i < NumPlayers - 1; i++)
            {
                Color playerColor = ColorManager.GetRandomDistinctColor(playerColors, noGreyScale: true);
                string name = ColorManager.GetColorName(playerColor);
                Player newPlayer = new Player(this, name, playerColor, true);
                Players.Add(newPlayer);
                playerColors.Add(playerColor);
            }

            // Init starting regions
            foreach(Player p in Players)
            {
                for (int i = 0; i < NumStartingTerritories; i++)
                {
                    List<Region> startingCandidates = Map.LandRegions.Where(x => Territories[x].Player == null).ToList();
                    Region startingRegion = startingCandidates[Random.Range(0, startingCandidates.Count)];
                    Territory startingTerritory = Territories[startingRegion];
                    CaptureTerritory(p, startingTerritory);
                    AddTroops(startingTerritory, NumStartingArmies);
                }
            }

            // Init controls
            InputManager.Init(this);

            State = GameState.Ready;
            StartGame();
        }

        #endregion

        #region Game Flow

        public void StartGame()
        {
            StartPlannningPhase();
        }

        private void StartPlannningPhase()
        {
            Debug.Log("Planning Phase started");

            TroopMovements.Add(new List<Army>());
            Battles.Add(new List<Battle>());
            foreach (Territory t in Territories.Values) t.ResetPlannedTroops();
            
            State = GameState.PlanningPhase;
        }

        public void EndPlanningPhase()
        {
            foreach (ArmyArrow armyArrow in ArmyArrows) Destroy(armyArrow.gameObject);
            ArmyArrows.Clear();
            foreach (Player p in Players.Where(x => x.IsNpc)) p.DoNpcTroopsMovements();
            foreach (Territory t in Territories.Values) t.EndPlanningPhase();
            StartCombatPhase();
        }

        private void StartCombatPhase()
        {
            Debug.Log("Combat Phase started");
            // Border battles
            List<Army> borderFightArmies = new List<Army>();
            foreach(Army army in TroopMovements[Turn])
            {
                Army mirrorMovement = TroopMovements[Turn].Where(x => x.SourceTerritory == army.TargetTerritory && x.TargetTerritory == army.SourceTerritory && !borderFightArmies.Contains(x)).FirstOrDefault();
                if (mirrorMovement != null)
                {
                    borderFightArmies.Add(army);
                    borderFightArmies.Add(mirrorMovement);
                    BorderBattle borderBattle = new BorderBattle(ArmyPrefab, Turn, new List<Army>() { army, mirrorMovement });
                    Battles[Turn].Add(borderBattle);
                }
            }

            // Territory 
            Dictionary<Territory, List<Army>> territoryBattles = new Dictionary<Territory, List<Army>>();
            foreach(Army army in TroopMovements[Turn].Where(x => !borderFightArmies.Contains(x)))
            {
                if (!territoryBattles.ContainsKey(army.TargetTerritory)) territoryBattles.Add(army.TargetTerritory, new List<Army>());
                territoryBattles[army.TargetTerritory].Add(army);
            }
            foreach(KeyValuePair<Territory, List<Army>> kvp in territoryBattles)
            {
                Battle territoryBattle = new TerritoryBattle(ArmyPrefab, Turn, kvp.Value);
                Battles[Turn].Add(territoryBattle);
            }

            Invoke(nameof(ResolveBorderBattles), ArmyApproachTime);
            Invoke(nameof(ResolveTerritoryBattles), ArmyApproachTime * 2);
            Invoke(nameof(EndCombatPhase), ArmyApproachTime * 2);

            State = GameState.CombatPhase;
        }

        private void ResolveBorderBattles()
        {
            List<Battle> newBattles = new List<Battle>();
            foreach(BorderBattle battle in Battles[Turn].Where(x => x.Type == BattleType.BorderBattle))
            {
                foreach (VisualArmy va in battle.Armies.Select(x => x.VisualArmy)) Destroy(va.gameObject);

                if (battle.SupportOnly)
                {
                    // Continue the armies to their target when it's just support
                    foreach (Army army in battle.Armies)
                    {
                        TerritoryBattle targetBattle = (TerritoryBattle)Battles[Turn].Where(x => x.Type == BattleType.TerritoryBattle && x.Territories.Contains(army.TargetTerritory)).FirstOrDefault();
                        if (targetBattle != null) targetBattle.AddArmyFromBorderBattle(ArmyPrefab, army);
                        else
                        {
                            TerritoryBattle newTerritoryBattle = new TerritoryBattle(ArmyPrefab, Turn, new List<Army>());
                            newTerritoryBattle.AddArmyFromBorderBattle(ArmyPrefab, army);
                            newBattles.Add(newTerritoryBattle);
                        }
                    }
                }

                else
                {
                    // Fight
                    battle.ResolveBattle();

                    // Create new army for winner
                    Territory sourceTerritory = battle.Territories.First(x => x.Player == battle.Winner);
                    Territory targetTerritory = battle.Territories.First(x => x.Player == battle.Winner);
                    Player winner = battle.Winner;
                    Player loser = battle.Players.First(x => x != battle.Winner);
                    Army armyFromBorderBattle = new Army(Turn, battle.WinnerSourceTerritory, battle.WinnerTargetTerritory, battle.Winner, battle.Losers[0], battle.WinnerTroopsRemaining, battle.IsWaterBattle);

                    // Add winner army to existing territory battle (if exists) or create new territory battle
                    TerritoryBattle targetBattle = (TerritoryBattle)Battles[Turn].Where(x => x.Type == BattleType.TerritoryBattle && x.Territories.Contains(battle.WinnerTargetTerritory)).FirstOrDefault();
                    if (targetBattle != null) targetBattle.AddArmyFromBorderBattle(ArmyPrefab, armyFromBorderBattle);
                    else
                    {
                        TerritoryBattle newTerritoryBattle = new TerritoryBattle(ArmyPrefab, Turn, new List<Army>());
                        newTerritoryBattle.AddArmyFromBorderBattle(ArmyPrefab, armyFromBorderBattle);
                        newBattles.Add(newTerritoryBattle);
                    }
                }
            }
            Battles[Turn].AddRange(newBattles);
        }

        private void ResolveTerritoryBattles()
        {
            foreach (Battle battle in Battles[Turn].Where(x => x.Type == BattleType.TerritoryBattle))
            {
                foreach (VisualArmy va in battle.Armies.Select(x => x.VisualArmy)) Destroy(va.gameObject);

                if (battle.SupportOnly)
                {
                    foreach (Army army in battle.Armies) army.TargetTerritory.AddTroops(army.NumTroops);
                }
                else
                {
                    battle.ResolveBattle();
                    if(battle.Winner == battle.Territories[0].Player)
                    {
                        battle.Territories[0].SetTroops(battle.WinnerTroopsRemaining);
                    }
                    else
                    {
                        CaptureTerritory(battle.Winner, battle.Territories[0]);
                        battle.Territories[0].SetTroops(battle.WinnerTroopsRemaining);
                    }
                }
            }

            foreach (Territory t in Territories.Values) t.ResetPlannedTroops();
        }

        private void EndCombatPhase()
        {
            // Remove dead players
            List<Player> deadPlayers = new List<Player>();
            foreach(Player p in Players)
            {
                if (p.Territories.Count == 0) deadPlayers.Add(p);
            }
            foreach(Player p in deadPlayers)
            {
                Players.Remove(p);
                Debug.Log(p.Name + " died :(");
                if(Players.Count == 1)
                {
                    Debug.Log(Players.First().Name + " won the game :)");
                    State = GameState.Ended;
                }
            }
            State = GameState.CombatPhaseEnded;
            StartDistributionPhase();
        }

        public void StartDistributionPhase()
        {
            Debug.Log("Distribution Phase started");
            Turn++;
            foreach (Player p in Players) p.CalculateTroopAmountToDistribute();
            State = GameState.DistributionPhase;
        }

        public void EndDistributionPhase()
        {
            foreach (Player p in Players.Where(x => x.IsNpc)) p.DoNpcTroopDistribution();
            foreach (Territory t in Territories.Values) t.LockInDistributedTroops();
            StartPlannningPhase();
        }

        #endregion

        #region Game Commands

        /// <summary>
        /// Change to owner of a territory.
        /// </summary>
        public void CaptureTerritory(Player p, Territory t)
        {
            Player oldOwner = t.Player;
            if (oldOwner != null) t.Player.Territories.Remove(t);
            t.Player = p;
            p.Territories.Add(t);
            t.Region.SetColor(p.Color);
        }

        /// <summary>
        /// Add a certain amount of troops to a territory
        /// </summary>
        public void AddTroops(Territory t, int troops)
        {
            t.AddTroops(troops);
        }

        public void DistributeTroop(Territory t)
        {
            if (t.Player.TroopsToDistribute <= 0) throw new System.Exception("Player of this territory has no troops left to distribute.");
            t.DistributeTroop();
            t.Player.TroopsToDistribute--;
        }

        public void UndistributeTroop(Territory t)
        {
            if (t.DistributedTroops <= 0) throw new System.Exception("Territory has no distributed troops to undistribute.");
            t.UndistributeTroop();
            t.Player.TroopsToDistribute++;
        }

        /// <summary>
        /// An army represents a movement of n troops from one territory to another. Use this method to add a new army to the current planning phase.
        /// </summary>
        public Army AddArmy(Territory sourceTerritory, Territory targetTerritory, int numTroops)
        {
            bool isWaterMovement = sourceTerritory.Region.WaterNeighbours.Contains(targetTerritory.Region);
            Army newMovement = new Army(Turn, sourceTerritory, targetTerritory, sourceTerritory.Player, targetTerritory.Player, numTroops, isWaterMovement);
            TroopMovements[Turn].Add(newMovement);
            newMovement.SourceTerritory.PlanTroops(newMovement.NumTroops);
            return newMovement;
        }

        /// <summary>
        /// An army represents a movement of n troops from one territory to another. Use this method to edit the amount of troops in an existing army of the current planning phase.
        /// </summary>
        public void EditArmy(Army army, int numTroopsNew)
        {
            int numTroopsBefore = army.NumTroops;
            army.NumTroops = numTroopsNew;
            army.SourceTerritory.PlanTroops(numTroopsNew - numTroopsBefore);
        }

        /// <summary>
        /// An army represents a movement of n troops from one territory to another. Use this method to remove an existing army of the current planning phase.
        /// </summary>
        public void RemoveArmy(Army army)
        {
            TroopMovements[Turn].Remove(army);
            army.SourceTerritory.PlanTroops(-army.NumTroops);
        }

        #endregion

        #region Helper

        #endregion
    }
}
