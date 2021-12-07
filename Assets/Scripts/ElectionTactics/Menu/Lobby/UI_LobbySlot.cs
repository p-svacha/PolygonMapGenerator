using System.Collections;
using System.Collections.Generic;
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

        public LobbySlotType Type;

        // Start is called before the first frame update
        void Start()
        {
            RemovePlayerButton.onClick.AddListener(SetInactive);
            AddPlayerButton.onClick.AddListener(() => SetActive(PartyNameGenerator.GetRandomPartyName(), LobbySlotType.Bot, canRemove: true));
        }

        public void SetActive(string playerName, LobbySlotType type, bool canRemove)
        {
            InactivePanel.SetActive(false);
            ActivePanel.SetActive(true);
            PlayerText.text = playerName;
            RemovePlayerButton.gameObject.SetActive(canRemove);
            Type = type;
        }

        public void SetInactive()
        {
            InactivePanel.SetActive(true);
            ActivePanel.SetActive(false);
            Type = LobbySlotType.Free;
        }
    }
}
