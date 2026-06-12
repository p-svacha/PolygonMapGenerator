using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Defines the settings of the game, including players and static rules. Is set during a game setup lobby.
    /// </summary>
    public class GameSettings
    {
        public List<LobbySlot> Slots;
        public GameModeDef GameMode;
        public TurnLengthDef TurnLength;
        public BotDifficultyDef BotDifficulty;
        public GameLengthDef GameLength;
        public StartingDistrictsDef StartingDistricts;
        public bool IsTutorialEnabled { get; private set; }

        /// <summary>
        /// Creates a new game setting object based on the current state of the lobby.
        /// </summary>
        public GameSettings()
        {
            Slots = new List<LobbySlot>(UI_Lobby.Instance.Slots);
            TurnLength = DefDatabase<TurnLengthDef>.AllDefs[UI_Lobby.Instance.TurnLengthDropdown.value];
            GameMode = DefDatabase<GameModeDef>.AllDefs[UI_Lobby.Instance.GameModeDropdown.value];
            BotDifficulty = DefDatabase<BotDifficultyDef>.AllDefs[UI_Lobby.Instance.BotDifficultyDropdown.value];
            GameLength = DefDatabase<GameLengthDef>.AllDefs[UI_Lobby.Instance.GameLengthDropdown.value];
            StartingDistricts = DefDatabase<StartingDistrictsDef>.AllDefs[UI_Lobby.Instance.StartingDistrictsDropdown.value];
            IsTutorialEnabled = false;
        }

        public GameSettings(List<LobbySlot> slots, GameModeDef gameMode, TurnLengthDef turnLength, BotDifficultyDef botDifficulty, GameLengthDef gameLength, StartingDistrictsDef numStartingDistricts, bool isTutorialEnabled)
        {
            Slots = slots;
            GameMode = gameMode;
            TurnLength = turnLength;
            BotDifficulty = botDifficulty;
            GameLength = gameLength;
            StartingDistricts = numStartingDistricts;
            IsTutorialEnabled = isTutorialEnabled;
        }


        public override string ToString()
        {
            return $"Game mode: {GameMode.Label}, Turn length: {TurnLength.Label}, Bot difficuly: {BotDifficulty.Label}";
        }
    }
}
