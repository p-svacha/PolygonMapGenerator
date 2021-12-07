using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Lobby : MonoBehaviour
    {
        private MenuNavigator MenuNavigator;

        public List<UI_LobbySlot> Slots;
        public Button StartGameButton;
        public Text StartGameButtonText;

        public LobbyType Type;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
            StartGameButton.onClick.AddListener(StartGame);
        }

        public void InitSingleplayerGame(string playerName)
        {
            foreach (UI_LobbySlot player in Slots) player.SetInactive();
            Slots[0].SetActive(playerName, LobbySlotType.LocalPlayer, canRemove: false);
            Type = LobbyType.Singleplayer;
        }

        public void InitHostMultiplayerGame(string playerName)
        {
            foreach (UI_LobbySlot player in Slots) player.SetInactive();
            Slots[0].SetActive(playerName, LobbySlotType.LocalPlayer, canRemove: false);
            Type = LobbyType.MultiplayerHost;
        }

        public void AddHumanPlayer(string name)
        {
            foreach(UI_LobbySlot player in Slots)
            {
                if (player.Type == LobbySlotType.Free)
                {
                    player.SetActive(name, LobbySlotType.Human, canRemove: false);
                    break;
                }
                
            }
        }

        public void InitJoinMultiplayerGame()
        {
            Type = LobbyType.MultiplayerClient;
            StartGameButtonText.text = "Waiting";
            StartGameButton.enabled = false;
        }

        private void StartGame()
        {
            GameSettings gameSettings = new GameSettings(Slots);
            MenuNavigator.StartGame(gameSettings);
        }
    }
}
