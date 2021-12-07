using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_MainMenu : MonoBehaviour
    {
        private MenuNavigator MenuNavigator;

        public Text PlayerNameText;
        public Button SingleplayerButton;
        public Button HostGameButton;
        public Button JoinGameButton;
        public Button ExitButton;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
            SingleplayerButton.onClick.AddListener(CreateSingleplayerGame);
            HostGameButton.onClick.AddListener(HostGame);
            JoinGameButton.onClick.AddListener(JoinGame);
            ExitButton.onClick.AddListener(() => Application.Quit());
        }

        private void CreateSingleplayerGame()
        {
            MenuNavigator.CreateSingleplayerGame();
        }

        private void HostGame()
        {
            MenuNavigator.HostGame();
        }

        private void JoinGame()
        {
            MenuNavigator.JoinGame();
        }
    }


}
    