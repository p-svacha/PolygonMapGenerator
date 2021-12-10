using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Defines the settings of the game, including players and static rules. Is set during a game setup lobby.
    /// </summary>
    public class GameSettings
    {
        public List<LobbySlot> Slots;

        public GameSettings(List<LobbySlot> lobbySlots)
        {
            Slots = lobbySlots;
        }
    }
}