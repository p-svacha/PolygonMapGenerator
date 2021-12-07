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
        public Button HostGameButton;
        public Button JoinGameButton;
        public Button ExitButton;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
            HostGameButton.onClick.AddListener(HostGame);
            JoinGameButton.onClick.AddListener(JoinGame);
            ExitButton.onClick.AddListener(() => Application.Quit());
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
    