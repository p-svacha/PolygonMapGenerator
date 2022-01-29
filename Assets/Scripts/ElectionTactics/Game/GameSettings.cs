using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Defines the settings of the game, including players and static rules. Is set during a game setup lobby.
    /// </summary>
    public class GameSettings
    {
        public List<LobbySlot> Slots;
        public TurnLengthOptions TurnLength;

        // Rules Options
        public enum TurnLengthOptions
        {
            [Description("Fast (60+10)")] Fast,
            [Description("Medium (90+15)")] Medium,
            [Description("Long (120+20)")] Slow,
        }

        public GameSettings(List<LobbySlot> lobbySlots, List<int> Rules)
        {
            Slots = lobbySlots;
            TurnLength = (TurnLengthOptions)Rules[0];
        }
    }
}
