using System;
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

        [Header("Elements")]
        public List<LobbySlot> Slots = new List<LobbySlot>();
        public List<UI_LobbySlot> UiSlots;

        public Button StartGameButton;
        public Text StartGameButtonText;
        public Text StartGameWarningText;
        private float StartGameWarningTextDelay;
        private const float StartGameWarningTextDisplayTime = 3f;

        public Button BackButton;
        public Text BackButtonText;

        public List<Dropdown> Rules;

        public GameType Type;
        List<Color> UsedColors = new List<Color>();

        private void Update()
        {
            if(StartGameWarningText.text != "")
            {
                StartGameWarningTextDelay += Time.deltaTime;
                if (StartGameWarningTextDelay >= StartGameWarningTextDisplayTime) StartGameWarningText.text = "";
            }
        }

        #region Init

        /// <summary>
        /// Gets executed once when game program is started
        /// </summary>
        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;

            // Buttons
            StartGameButton.onClick.AddListener(StartGameButton_OnClick);
            BackButton.onClick.AddListener(BackButton_OnClick);

            // Slots
            for(int i = 0; i < UiSlots.Count; i++)
            {
                LobbySlot slot = new LobbySlot(i, LobbySlotType.Free);
                Slots.Add(slot);
                UiSlots[i].Init(this, slot);
            }

            // Rules

            foreach (GameSettings.TurnLengthOptions option in Enum.GetValues(typeof(GameSettings.TurnLengthOptions))) Rules[0].options.Add(new Dropdown.OptionData(option.ToString()));

            foreach(Dropdown rule in Rules)
            {
                rule.value = 1; rule.value = 0;
            }

            for (int i = 0; i < Rules.Count; i++)
            {
                int j = i; // needed to to make sure call is made by value and not reference
                Rules[j].onValueChanged.AddListener((x) => OnRuleChanged(j, x));
            }
        }

        public void InitSingleplayerGame(string playerName)
        {
            Type = GameType.Singleplayer;
            foreach (UI_LobbySlot slot in UiSlots) slot.SetInactive();

            UsedColors.Clear();
            Color partyColor = PartyNameGenerator.GetPartyColor(playerName, UsedColors);
            UsedColors.Add(partyColor);
            FillNextFreeSlot(playerName, partyColor, LobbySlotType.Human);

            OrganizeSlots();
        }   

        public void InitHostMultiplayerGame(string playerName)
        {
            Type = GameType.MultiplayerHost;
            foreach (UI_LobbySlot slot in UiSlots) slot.SetInactive();

            UsedColors.Clear();
            Color partyColor = PartyNameGenerator.GetPartyColor(playerName, UsedColors);
            UsedColors.Add(partyColor);
            FillNextFreeSlot(playerName, partyColor, LobbySlotType.Human, NetworkPlayer.LocalClientId);

            OrganizeSlots();

            StartGameButtonText.text = "Start";
            StartGameButton.enabled = true;
            foreach (Dropdown rule in Rules) rule.enabled = true;
        }

        public void InitJoinMultiplayerGame()
        {
            Type = GameType.MultiplayerClient;
            StartGameButtonText.text = "Waiting";
            StartGameButton.enabled = false;
            foreach (Dropdown rule in Rules) rule.enabled = false;
        }

        #endregion

        #region Slots

        /// <summary>
        /// Gets called on the server when a player joins
        /// </summary>
        public void PlayerJoined(NetworkConnectionData connectionData)
        {
            Color playerColor = PartyNameGenerator.GetPartyColor(connectionData.Name, UsedColors);
            UsedColors.Add(playerColor);
            FillNextFreeSlot(connectionData.Name, playerColor, LobbySlotType.Human, connectionData.ClientId);

            OrganizeSlots();

            // Send lobby data (slots and rules) to newly connected client
            if (Type == GameType.MultiplayerHost)
            {
                NetworkPlayer.Server.UpdateLobbySlotsServerRpc();
                for (int i = 0; i < Rules.Count; i++)
                    if (Rules[i].value != 0) NetworkPlayer.Server.LobbyRuleChangedServerRpc(i, Rules[i].value);
            }
        }

        /// <summary>
        /// Gets called on the server whenever a connected player leaves.
        /// </summary>
        public void PlayerLeft(ulong clientId)
        {
            UI_LobbySlot slot = UiSlots.First(x => x.Slot.ClientId == clientId);
            RemovePlayer(slot);
        }

        public void AddBot()
        {
            string botName = PartyNameGenerator.GetRandomPartyName();
            Color botColor = PartyNameGenerator.GetPartyColor(botName, UsedColors);
            UsedColors.Add(botColor);
            FillNextFreeSlot(botName, botColor, LobbySlotType.Bot);

            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.UpdateLobbySlotsServerRpc();
            else OrganizeSlots();
        }
        public void RemovePlayer(UI_LobbySlot slot)
        {
            slot.SetInactive();
            UsedColors.Remove(slot.Slot.GetColor());
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.UpdateLobbySlotsServerRpc();
            else OrganizeSlots();
        }

        public void ChangeSlotColor(UI_LobbySlot slot)
        {
            if (Type == GameType.Singleplayer) UpdatePlayerColor(slot.Slot.Id, GetRandomNewColorFor(slot.Slot.Id));
            else
            {
                NetworkPlayer.Server.RequestColorChangeServerRpc(slot.Slot.Id);
            }
        }

        /// <summary>
        /// Server method for color change
        /// </summary>
        public Color GetRandomNewColorFor(int slotId)
        {
            Color oldColor = UiSlots[slotId].Slot.GetColor();
            Color newColor = PartyNameGenerator.GetRandomColor(UsedColors);
            UsedColors.Remove(oldColor);
            UsedColors.Add(newColor);
            return newColor;
        }

        public void UpdatePlayerColor(int slotId, Color c)
        {
            UiSlots[slotId].SetColor(c);
        }

        public void FillNextFreeSlot(string name, Color c, LobbySlotType slotType, ulong clientId = 0)
        {
            foreach(UI_LobbySlot uiSlot in UiSlots)
            {
                if (uiSlot.Slot.SlotType == LobbySlotType.Free || uiSlot.Slot.SlotType == LobbySlotType.Inactive)
                {
                    uiSlot.SetActive(name, c, slotType, clientId);
                    break;
                }
            }
        }

        private void OrganizeSlots()
        {
            List<UI_LobbySlot> filledSlots = UiSlots.Where(x => x.Slot.SlotType != LobbySlotType.Free && x.Slot.SlotType != LobbySlotType.Inactive).ToList();

            for(int i = 0; i < UiSlots.Count; i++)
            {
                if (i < filledSlots.Count) UiSlots[i].SetActive(filledSlots[i].Slot.Name, filledSlots[i].Slot.GetColor(), filledSlots[i].Slot.SlotType, filledSlots[i].Slot.ClientId);
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
                if (slot.SlotType != LobbySlotType.Free && slot.SlotType != LobbySlotType.Inactive) FillNextFreeSlot(slot.Name, slot.GetColor(), slot.SlotType, slot.ClientId);
            }
            OrganizeSlots();
        }

        #endregion

        #region Rules

        private void OnRuleChanged(int ruleId, int value)
        {
            if (Type == GameType.Singleplayer) return;
            else if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.LobbyRuleChangedServerRpc(ruleId, value);
        }

        public void GetRuleChangeFromServer(int ruleId, int value)
        {
            Debug.Log("Client: Rule " + ruleId + " changed to " + value);
            Rules[ruleId].value = value;
        }

        #endregion

        #region Buttons

        private void StartGameButton_OnClick()
        {
            // 2 player minimum error
            if (Slots.Where(x => x.SlotType == LobbySlotType.Bot || x.SlotType == LobbySlotType.Human).Count() < 2)
            {
                DisplayStartGameError("Can't start game with less than two players.");
                return;
            }

            // Start game
            GameSettings gameSettings = new GameSettings(Slots, Rules.Select(x => x.value).ToList());
            if (Type == GameType.MultiplayerHost) NetworkPlayer.Server.InitGameServerRpc();
            MenuNavigator.InitGame(gameSettings);
        }

        private void DisplayStartGameError(string msg)
        {
            StartGameWarningText.text = msg;
            StartGameWarningTextDelay = 0f;
        }

        private void BackButton_OnClick()
        {
            if (Type != GameType.Singleplayer) ET_NetworkManager.Singleton.LeaveGame();
            MenuNavigator.SwitchToMainMenuScreen();
        }

        #endregion
    }
}
