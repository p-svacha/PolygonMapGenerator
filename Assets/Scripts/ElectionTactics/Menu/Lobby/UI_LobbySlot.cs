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
        public GameObject InactivePanel;
        public Button AddPlayerButton;

        public LobbySlot Slot;

        void Start()
        {
            RemovePlayerButton.onClick.AddListener(SetInactive);
            AddPlayerButton.onClick.AddListener(() => SetActive(PartyNameGenerator.GetRandomPartyName(), LobbySlotType.Bot));
        }

        public void Init(LobbySlot slot)
        {
            Slot = slot;
            SetInactive();
        }

        public void SetActive(string playerName, LobbySlotType type)
        {
            Slot.SetActive(playerName, type);

            InactivePanel.SetActive(false);
            ActivePanel.SetActive(true);
            PlayerText.text = playerName;
            bool canRemove = Slot.SlotType != LobbySlotType.LocalPlayer && (Slot.LobbyType == GameType.Singleplayer || NetworkManager.Singleton.IsHost);
            RemovePlayerButton.gameObject.SetActive(canRemove);
        }

        public void SetInactive()
        {
            Slot.SetInactive();

            InactivePanel.SetActive(true);
            ActivePanel.SetActive(false);
        }
    }
}
