using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_GameSetupPlayer : MonoBehaviour
    {
        [Header("Active Form")]
        public GameObject ActivePanel;
        public Text PlayerText;
        public Button RemovePlayerButton;

        [Header("Inactive Form")]
        public GameObject InactivePanel;
        public Button AddPlayerButton;

        public bool IsActive;

        // Start is called before the first frame update
        void Start()
        {
            RemovePlayerButton.onClick.AddListener(SetInactive);
            AddPlayerButton.onClick.AddListener(() => SetActive(PartyNameGenerator.GetRandomPartyName(), canRemove: true));
        }

        public void SetActive(string playerName, bool canRemove)
        {
            InactivePanel.SetActive(false);
            ActivePanel.SetActive(true);
            PlayerText.text = playerName;
            RemovePlayerButton.gameObject.SetActive(canRemove);
            IsActive = true;
        }

        public void SetInactive()
        {
            InactivePanel.SetActive(true);
            ActivePanel.SetActive(false);
            IsActive = false;
        }
    }
}
