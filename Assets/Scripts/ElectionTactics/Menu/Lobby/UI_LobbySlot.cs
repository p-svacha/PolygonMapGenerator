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
        public Button RemovePlayerButton;

        [Header("Inactive Form")]
        public GameObject AddPlayerPanel;
        public Button AddPlayerButton;

        public UI_Lobby Lobby;
        public LobbySlot Slot;


        void Start()
        {
            RemovePlayerButton.onClick.AddListener(() => Lobby.RemovePlayer(this));
            AddPlayerButton.onClick.AddListener(() => Lobby.AddBot());
        }

        public void Init(UI_Lobby lobby, LobbySlot slot)
        {
            Lobby = lobby;
            Slot = slot;
            SetInactive();
        }

        public void SetActive(string playerName, LobbySlotType type)
        {
            Slot.SetActive(playerName, type);

            AddPlayerPanel.SetActive(false);
            ActivePanel.SetActive(true);
            PlayerText.text = playerName;
            bool canRemove = Slot.SlotType != LobbySlotType.LocalPlayer && (Lobby.Type == GameType.Singleplayer || NetworkManager.Singleton.IsHost);
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
    }
}
