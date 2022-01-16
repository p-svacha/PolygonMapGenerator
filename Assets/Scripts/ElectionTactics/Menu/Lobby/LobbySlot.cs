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
        public ulong ClientId;
        private float ColorR;
        private float ColorG;
        private float ColorB;

        public LobbySlot(LobbySlotType slotType)
        {
            SlotType = slotType;
            Name = "";
        }

        public void SetActive(string name, Color c, LobbySlotType type, ulong clientId)
        {
            Name = name;
            SlotType = type;
            ColorR = c.r;
            ColorG = c.g;
            ColorB = c.b;
            ClientId = clientId;
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

        public Color GetColor()
        {
            return new Color(ColorR, ColorG, ColorB);
        }
    }
}
