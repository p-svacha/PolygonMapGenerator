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
                uiSlot.Init(this, slot);
            }
        }

        public void InitSingleplayerGame(string playerName)
        {
            Type = GameType.Singleplayer;
            foreach (UI_LobbySlot slot in UiSlots) slot.SetInactive();
            FillNextFreeSlot(playerName, LobbySlotType.LocalPlayer);
            OrganizeSlots();
        }

        public void InitHostMultiplayerGame(string playerName)
        {
            Type = GameType.MultiplayerHost;
            foreach (UI_LobbySlot slot in UiSlots) slot.SetInactive();
            FillNextFreeSlot(playerName, LobbySlotType.LocalPlayer);
            OrganizeSlots();
        }

        public void InitJoinMultiplayerGame()
        {
            Type = GameType.MultiplayerClient;
            StartGameButtonText.text = "Waiting";
            StartGameButton.enabled = false;
        }

        /// <summary>
        /// Is run on server when a player joins
        /// </summary>
        public void PlayerJoined(NetworkConnectionData connectionData)
        {
            FillNextFreeSlot(connectionData.Name, LobbySlotType.Human);
            OrganizeSlots();
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.UpdateLobbyServerRpc();
        }

        public void AddBot()
        {
            FillNextFreeSlot(PartyNameGenerator.GetRandomPartyName(), LobbySlotType.Bot);
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.UpdateLobbyServerRpc();
            else OrganizeSlots();
        }
        public void RemovePlayer(UI_LobbySlot slot)
        {
            slot.SetInactive();
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.UpdateLobbyServerRpc();
            else OrganizeSlots();
        }

        public void FillNextFreeSlot(string name, LobbySlotType slotType)
        {
            foreach(UI_LobbySlot uiSlot in UiSlots)
            {
                if (uiSlot.Slot.SlotType == LobbySlotType.Free || uiSlot.Slot.SlotType == LobbySlotType.Inactive)
                {
                    uiSlot.SetActive(name, slotType);
                    break;
                }
            }
        }

        private void OrganizeSlots()
        {
            List<UI_LobbySlot> filledSlots = UiSlots.Where(x => x.Slot.SlotType != LobbySlotType.Free && x.Slot.SlotType != LobbySlotType.Inactive).ToList();

            for(int i = 0; i < UiSlots.Count; i++)
            {
                if (i < filledSlots.Count) UiSlots[i].SetActive(filledSlots[i].Slot.Name, filledSlots[i].Slot.SlotType);
                else if (i == filledSlots.Count && Type != GameType.MultiplayerClient) UiSlots[i].SetAddPlayer();
                else UiSlots[i].SetInactive();
            }
        }

        public void SetSlotsFromServer(byte[] data)
        {
            foreach (UI_LobbySlot uiSlot in UiSlots) uiSlot.SetInactive();

            List<LobbySlot> slots = (List<LobbySlot>) ET_NetworkManager.Deserialize(data);

            foreach(LobbySlot slot in slots)
            {
                if (slot.SlotType != LobbySlotType.Free && slot.SlotType != LobbySlotType.Inactive) FillNextFreeSlot(slot.Name, slot.SlotType);
            }
            OrganizeSlots();
        }

        private void InitGame()
        {
            GameSettings gameSettings = new GameSettings(Slots);
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.InitGameServerRpc();
            MenuNavigator.InitGame(gameSettings);
        }
    }
}
