using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    [System.Serializable]
    public class LobbySlot
    {
        public GameType LobbyType;
        public LobbySlotType SlotType;
        public string Name;

        public LobbySlot(GameType lobbyType, LobbySlotType slotType)
        {
            LobbyType = lobbyType;
            SlotType = slotType;
            Name = "";
        }

        public void SetActive(string name, LobbySlotType type)
        {
            Name = name;
            SlotType = type;
        }

        public void SetInactive()
        {
            Name = "";
            SlotType = LobbySlotType.Free;
        }
    }
}
