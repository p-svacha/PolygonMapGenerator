using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_LobbySlot : MonoBehaviour
    {
        [Header("Active Form")]
        public GameObject ActivePanel;
        public TMP_InputField PartyNameInput;
        public TextMeshProUGUI PartyNameText;
        public Button PlayerColorButton;
        public Button RandomizeNameButton;
        public Image PlayerColor;
        public Button RemovePlayerButton;

        [Header("Inactive Form")]
        public GameObject AddPlayerPanel;
        public Button AddPlayerButton;

        public UI_Lobby Lobby;
        public LobbySlot Slot;


        void Start()
        {
            PartyNameInput.onValueChanged.AddListener(OnNameChanged);

            RandomizeNameButton.onClick.AddListener(RandomizeName);
            RandomizeNameButton.onClick.AddListener(() => AudioManager.PlaySound(AudioManager.Instance.Chimes));

            PlayerColorButton.onClick.AddListener(() => Lobby.ChangeSlotColor(this, backwards: false));
            PlayerColorButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());
            PlayerColorButton.GetComponent<RightClickHandler>().OnRightClick = () => Lobby.ChangeSlotColor(this, backwards: true);

            RemovePlayerButton.onClick.AddListener(() => Lobby.RemovePlayer(this));
            RemovePlayerButton.onClick.AddListener(() => AudioManager.PlaySound(AudioManager.Instance.RemoveBot));

            AddPlayerButton.onClick.AddListener(() => Lobby.AddBot());
            AddPlayerButton.onClick.AddListener(() => AudioManager.PlaySound(AudioManager.Instance.AddBot));
        }

        public void Init(UI_Lobby lobby, LobbySlot slot)
        {
            Lobby = lobby;
            Slot = slot;
            SetInactive();
        }

        private void OnNameChanged(string newName)
        {
            Slot.Name = newName;
        }

        private void RandomizeName()
        {
            PartyNameInput.text = PartyNameGenerator.GetRandomPartyName();
        }

        public void SetActive(string playerName, Color c, LobbySlotType type, ulong clientId)
        {
            Slot.SetActive(playerName, c, type, clientId);

            AddPlayerPanel.SetActive(false);
            ActivePanel.SetActive(true);
            PartyNameInput.text = playerName;
            PartyNameText.color = PartyNameGenerator.GetPartyTextColor(c);

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
            PartyNameText.color = PartyNameGenerator.GetPartyTextColor(c);
            PlayerColor.color = c;
            Slot.SetColor(c);
        }
    }
}
