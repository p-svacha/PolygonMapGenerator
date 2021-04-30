using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class ParriskGame : MonoBehaviour
    {
        public PolygonMapGenerator PMG;
        public InputManager InputManager;

        // Rules
        public int NumPlayers;
        public int NumStartingTerritories;

        // Game elements
        public Map Map;

        public Player LocalPlayer;
        public List<Player> Players = new List<Player>();
        public Dictionary<Region, Territory> Territories = new Dictionary<Region, Territory>();

        public List<List<TroopMovement>> TroopMovements = new List<List<TroopMovement>>(); // A list of all troop movement for every turn (index of list represents turn)

        public int Turn;

        // State
        public GameState State;

        // UI
        public UI_ParriskGame UI;

        #region Initialization

        public void InitGame(int numPlayers, int numStartingTerritories)
        {
            State = GameState.Initializing;
            NumPlayers = numPlayers;
            NumStartingTerritories = numStartingTerritories;
            Turn = -1;

            MapGenerationSettings settings = new MapGenerationSettings(15, 10, 0.4f, 2f, 3, 12, MapType.BigOceans);
            PMG.GenerateMap(settings, OnMapGenerationDone);
        }

        public void OnMapGenerationDone(Map map)
        {
            // Init map
            Map = map;
            Map.InitializeMap(true, true, true, true, MapDrawMode.ParriskBoard);
            Map.FocusMapCentered();

            foreach(Region r in Map.LandRegions)
            {
                Territory regionTerritory = new Territory(r, "Giorgio");
                Territories.Add(r, regionTerritory);
            }

            // Init players
            LocalPlayer = new Player("Local", ColorManager.GetColorByName("Green"));
            List<Color> playerColors = new List<Color>() { LocalPlayer.Color };
            Players.Add(LocalPlayer);
            for(int i = 0; i < NumPlayers - 1; i++)
            {
                Color playerColor = ColorManager.GetRandomDistinctColor(playerColors, noGreyScale: true);
                string name = ColorManager.GetColorName(playerColor);
                Player newPlayer = new Player(name, playerColor);
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
                    AddTroops(startingTerritory, 5);
                }
            }

            // Init controls
            InputManager.Init(this);

            State = GameState.Ready;
            StartGame();
        }

        public void StartGame()
        {
            StartTurn();
        }

        private void StartTurn()
        {
            Turn++;
            TroopMovements.Add(new List<TroopMovement>());
            foreach (Territory t in Territories.Values) t.ResetPlannedTroops();
            State = GameState.PlanningPhase;
        }

        // Start is called before the first frame update
        void Start()
        {
            InitGame(8, 2);
        }

        #endregion

        #region Game Flow

        // Update is called once per frame
        void Update()
        {
            
        }

        #endregion

        #region Game Commands

        public void CaptureTerritory(Player p, Territory t)
        {
            if (t.Player != null) t.Player.Territories.Remove(t);
            t.Player = p;
            p.Territories.Add(t);
            t.Region.SetColor(p.Color);
        }

        public void AddTroops(Territory t, int troops)
        {
            t.AddTroops(troops);
        }

        public void AddTroopMovement(TroopMovement tm)
        {
            TroopMovements[Turn].Add(tm);
            tm.SourceTerritory.PlanTroops(tm.NumTroops);
        }

        public void EditTroopMovement(TroopMovement tm, int numTroopsNew)
        {
            int numTroopsBefore = tm.NumTroops;
            tm.NumTroops = numTroopsNew;
            tm.SourceTerritory.PlanTroops(numTroopsNew - numTroopsBefore);
        }

        public void RemoveTroopMovement(TroopMovement tm)
        {
            TroopMovements[Turn].Remove(tm);
            tm.SourceTerritory.PlanTroops(-tm.NumTroops);
        }

        #endregion
    }
}
