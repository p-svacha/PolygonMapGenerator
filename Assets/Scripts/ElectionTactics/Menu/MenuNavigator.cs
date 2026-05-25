using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ElectionTactics
{
    public class MenuNavigator : MonoBehaviour
    {
        public static MenuNavigator Instance;

        public ElectionTacticsGame Game;
        public UI_MainMenu MainMenu;
        public UI_Lobby Lobby;

        private void Awake()
        {
            Instance = this;
            MainMenu.gameObject.SetActive(true);
        }

        void Start()
        {
            // Initialize menu screens
            MainMenu.Init(this);
            Lobby.Init(this);
            Lobby.gameObject.SetActive(false);
        }

        #region Menu Actions

        public void CreateSingleplayerGame()
        {
            string playerName = MainMenu.PlayerNameInput.text;
            Lobby.InitSingleplayerGame(playerName);
            SwitchToLobbyScreen(GameType.Singleplayer);
        }


        public void SwitchToLobbyScreen(GameType gameType)
        {
            MainMenu.gameObject.SetActive(false);
            Lobby.Show(gameType);
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

        public void SwitchToMainMenuScreen()
        {
            MainMenu.gameObject.SetActive(true);
            Lobby.gameObject.SetActive(false);
        }

    }
}
