using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace ElectionTactics
{
    public class MenuNavigator : MonoBehaviour
    {
        public UI_MainMenu MainMenuScreen;
        public UI_GameSetup GameSetupScreen;

        void Start()
        {
            // Initialize network hooks
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            // Initialize menu screens
            MainMenuScreen.Init(this);
            GameSetupScreen.Init(this);
            MainMenuScreen.gameObject.SetActive(true);
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

        public void HostGame()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += HandleApprovalCheck;
            NetworkManager.Singleton.StartHost();
            string playerName = MainMenuScreen.PlayerNameText.text;
            GameSetupScreen.CreateNewMultiplayerGame(playerName);

            MainMenuScreen.gameObject.SetActive(false);
            GameSetupScreen.gameObject.SetActive(true);
        }

        public void JoinGame()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(MainMenuScreen.PlayerNameText.text);
            NetworkManager.Singleton.StartClient();
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

            if (approveConnection)
            {
                GameSetupScreen.AddHumanPlayer(connectedPlayerName);
            }
        }

        /// <summary>
        /// Gets executed on the server every time has connected. And also on the client side when they themselves join.
        /// </summary>
        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) // We ourselves just joined
            {
                Debug.Log("NETWORK: We connected to the server");
            }
            else
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
