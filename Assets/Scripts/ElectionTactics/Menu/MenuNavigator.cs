using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace ElectionTactics
{
    public class MenuNavigator : MonoBehaviour
    {
        public ElectionTacticsGame Game;
        public UI_MainMenu MainMenuScreen;
        public UI_Lobby LobbyScreen;

        void Start()
        {
            // Initialize network hooks
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            // Initialize menu screens
            MainMenuScreen.Init(this);
            LobbyScreen.Init(this);
            MainMenuScreen.gameObject.SetActive(true);
            LobbyScreen.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Remove network hooks
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        #region Menu Actions

        public void CreateSingleplayerGame()
        {
            string playerName = MainMenuScreen.PlayerNameText.text;
            LobbyScreen.InitSingleplayerGame(playerName);
            SwitchToLobbyScreen();
        }

        public void HostGame()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += HandleApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }

        public void JoinGame()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(MainMenuScreen.PlayerNameText.text);
            NetworkManager.Singleton.StartClient();
        }

        private void SwitchToLobbyScreen()
        {
            MainMenuScreen.gameObject.SetActive(false);
            LobbyScreen.gameObject.SetActive(true);
        }

        public void StartGame(GameSettings gameSettings)
        {
            MainMenuScreen.gameObject.SetActive(false);
            LobbyScreen.gameObject.SetActive(false);

            Game.StarNewGame(gameSettings);
        }

        #endregion

        #region Network Hooks

        /// <summary>
        /// Gets executed when server is started and the host creates the game.
        /// </summary>
        private void HandleServerStarted()
        {
            Debug.Log("NETWORK: Server Started");
        }

        /// <summary>
        /// Gets executed on the server when a client wants to connect with the given connection data.
        /// </summary>
        private void HandleApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            string connectedPlayerName = Encoding.ASCII.GetString(connectionData);

            bool approveConnection = true;

            callback(false, null, approveConnection, null, null);
            Debug.Log("NETWORK: Incoming connection, Approval: " + approveConnection);

            if (approveConnection && clientId != NetworkManager.Singleton.LocalClientId)
            {
                LobbyScreen.AddHumanPlayer(connectedPlayerName);
            }
        }

        /// <summary>
        /// Gets executed on the server every time has connected. And also on the client side when they themselves join.
        /// </summary>
        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) // We ourselves just joined
            {
                string playerName = MainMenuScreen.PlayerNameText.text;
                Debug.Log("NETWORK: We (" + playerName + ") connected to the server");

                if (NetworkManager.Singleton.IsHost) // Host
                {
                    LobbyScreen.InitHostMultiplayerGame(playerName);
                    SwitchToLobbyScreen();
                }
                else // Client
                {
                    LobbyScreen.InitJoinMultiplayerGame();
                    SwitchToLobbyScreen();
                }
            }

            else // We are the server and someone joined
            {
                Debug.Log("NETWORK: Client Connected");
            }
        }

        /// <summary>
        /// Gets executed on the server every time a client disconnects. And also on the client side when they themselves disconnect.
        /// </summary>
        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) // We ourselves just disconnected
            {
                Debug.Log("NETWORK: We disconnected from the server");
            }
            else
            {
                Debug.Log("NETWORK: Client Disconnected");
            }
        }

        #endregion
    }
}
