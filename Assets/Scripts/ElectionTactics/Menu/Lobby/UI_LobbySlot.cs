using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_LobbySlot : MonoBehaviour
    {
        [Header("Active Form")]
        public GameObject ActivePanel;
        public Text PlayerText;
        public Button PlayerColorButton;
        public Image PlayerColor;
        public Button RemovePlayerButton;

        [Header("Inactive Form")]
        public GameObject AddPlayerPanel;
        public Button AddPlayerButton;

        public UI_Lobby Lobby;
        public LobbySlot Slot;


        void Start()
        {
            PlayerColorButton.onClick.AddListener(() => Lobby.ChangeSlotColor(this));
            RemovePlayerButton.onClick.AddListener(() => Lobby.RemovePlayer(this));
            AddPlayerButton.onClick.AddListener(() => Lobby.AddBot());
        }

        public void Init(UI_Lobby lobby, LobbySlot slot)
        {
            Lobby = lobby;
            Slot = slot;
            SetInactive();
        }

        public void SetActive(string playerName, Color c, LobbySlotType type, ulong clientId)
        {
            Slot.SetActive(playerName, c, type, clientId);

            AddPlayerPanel.SetActive(false);
            ActivePanel.SetActive(true);
            PlayerText.text = playerName;
            PlayerText.color = c;

            PlayerColor.color = c;
            bool canChangeColor = Slot.ClientId == NetworkPlayer.LocalClientId;
            PlayerColorButton.enabled = canChangeColor;

            bool canRemove = Slot.SlotType == LobbySlotType.Bot && (Lobby.Type == GameType.Singleplayer || NetworkManager.Singleton.IsHost);
            RemovePlayerButton.gameObject.SetActive(canRemove);
        }

        public void SetAddPlayer()
        {
            Slot.SetAddPlayer();

            AddPlayerPanel.SetActive(true);
            ActivePanel.SetActive(false);
        }

        public void SetInactive()
        {
            Slot.SetInactive();

            AddPlayerPanel.SetActive(false);
            ActivePanel.SetActive(false);
        }

        public void SetColor(Color c)
        {
            PlayerText.color = c;
            PlayerColor.color = c;
            Slot.SetColor(c);
        }
    }
}
