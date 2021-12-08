using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Lobby : MonoBehaviour
    {
        private MenuNavigator MenuNavigator;

        public List<LobbySlot> Slots = new List<LobbySlot>();
        public List<UI_LobbySlot> UiSlots;
        public Button StartGameButton;
        public Text StartGameButtonText;

        public GameType Type;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
            StartGameButton.onClick.AddListener(InitGame);
            foreach (UI_LobbySlot uiSlot in UiSlots)
            {
                LobbySlot slot = new LobbySlot(Type, LobbySlotType.Free);
                Slots.Add(slot);
                uiSlot.Init(slot);
            }
        }

        public void InitSingleplayerGame(string playerName)
        {
            Type = GameType.Singleplayer;
            InitSlots();
            FillNextFreeSlot(playerName, LobbySlotType.LocalPlayer);
        }

        public void InitHostMultiplayerGame(string playerName)
        {
            Type = GameType.MultiplayerHost;
            InitSlots();
            FillNextFreeSlot(playerName, LobbySlotType.LocalPlayer);
        }

        
        public void FillNextFreeSlot(string name, LobbySlotType slotType)
        {
            foreach(UI_LobbySlot uiSlot in UiSlots)
            {
                if (uiSlot.Slot.SlotType == LobbySlotType.Free)
                {
                    uiSlot.SetActive(name, slotType);
                    break;
                }
                
            }
        }

        public void InitJoinMultiplayerGame()
        {
            Type = GameType.MultiplayerClient;
            InitSlots();
            StartGameButtonText.text = "Waiting";
            StartGameButton.enabled = false;
        }

        private void InitSlots()
        {
            foreach (LobbySlot slot in Slots) slot.LobbyType = Type;
        }

        private void InitGame()
        {
            GameSettings gameSettings = new GameSettings(Slots);
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.InitGameServerRpc();
            MenuNavigator.InitGame(gameSettings);
        }

        public void SetSlotsFromServer(byte[] data)
        {
            foreach (UI_LobbySlot uiSlot in UiSlots) uiSlot.SetInactive();

            List<LobbySlot> slots = (List<LobbySlot>) ET_NetworkManager.Deserialize(data);

            foreach(LobbySlot slot in slots)
            {
                if (slot.SlotType != LobbySlotType.Free) FillNextFreeSlot(slot.Name, slot.SlotType);
            }
        }
    }
}
