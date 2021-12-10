using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    [System.Serializable]
    public class LobbySlot
    {
        public LobbySlotType SlotType;
        public string Name;

        public LobbySlot(GameType lobbyType, LobbySlotType slotType)
        {
            SlotType = slotType;
            Name = "";
        }

        public void SetActive(string name, LobbySlotType type)
        {
            Name = name;
            SlotType = type;
        }

        public void SetAddPlayer()
        {
            Name = "";
            SlotType = LobbySlotType.Free;
        }

        public void SetInactive()
        {
            Name = "";
            SlotType = LobbySlotType.Inactive;
        }
    }
}
