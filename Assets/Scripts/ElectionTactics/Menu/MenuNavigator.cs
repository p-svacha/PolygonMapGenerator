using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ElectionTactics
{
    public class MenuNavigator : MonoBehaviour
    {
        public ElectionTacticsGame Game;
        public UI_MainMenu MainMenu;
        public UI_Lobby Lobby;

        void Start()
        {
            // Initialize menu screens
            MainMenu.Init(this);
            Lobby.Init(this);
            MainMenu.gameObject.SetActive(true);
            Lobby.gameObject.SetActive(false);
        }

        #region Menu Actions

        public void CreateSingleplayerGame()
        {
            string playerName = MainMenu.PlayerNameText.text;
            Lobby.InitSingleplayerGame(playerName);
            SwitchToLobbyScreen();
        }


        public void SwitchToLobbyScreen()
        {
            MainMenu.gameObject.SetActive(false);
            Lobby.gameObject.SetActive(true);
        }

        public void InitGame(GameSettings gameSettings)
        {
            MainMenu.gameObject.SetActive(false);
            Lobby.gameObject.SetActive(false);
            Game.InitNewGame(gameSettings, Lobby.Type);
        }

        public void StartAndJoinGame()
        {
            MainMenu.gameObject.SetActive(false);
            Lobby.gameObject.SetActive(false);
            Game.InitJoinGame();
        }

        #endregion

        
    }
}
