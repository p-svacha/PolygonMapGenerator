using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace ElectionTactics
{
    public class ET_NetworkManager : MonoBehaviour
    {
        public MenuNavigator MenuNavigator;
        public ElectionTacticsGame Game;

        private Dictionary<ulong, NetworkConnectionData> ConnectionData = new Dictionary<ulong, NetworkConnectionData>();

        void Start()
        {
            // Initialize network hooks
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDestroy()
        {
            Debug.Log("NETWORK: Network Manager gets destroyed");
            // Remove network hooks
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        public void HostGame()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += HandleApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }

        public void JoinGame()
        {
            NetworkConnectionData connectionData = new NetworkConnectionData(MenuNavigator.MainMenu.PlayerNameText.text);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Serialize(connectionData);
            NetworkManager.Singleton.StartClient();
        }

        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost) // TODO: If the host leaves, they can't rehost without getting an error, so that doesn't work yet
            {
                Debug.Log("NETWORK: Server shut down.");
                ConnectionData.Clear();
            }
            NetworkManager.Singleton.Shutdown();
        }

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
            // Get connection data of connected client (not if we as the host just joined ourselves)
            NetworkConnectionData data = null;
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                data = (NetworkConnectionData) Deserialize(connectionData);
                data.ClientId = clientId;
            }

            bool approveConnection = true;

            callback(true, null, approveConnection, null, null);

            if (approveConnection && clientId != NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("NETWORK: Incoming client connection, Approval: " + approveConnection + "\n" + data.ToString());
                ConnectionData.Add(clientId, data);
            }
        }

        /// <summary>
        /// Gets executed on the server every time a client has connected. And also on the client side when they themselves join.
        /// </summary>
        private void HandleClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) // We ourselves just joined
            {
                string playerName = MenuNavigator.MainMenu.PlayerNameText.text;
                Debug.Log("NETWORK: We (" + playerName + " / " + clientId + ") connected to the server");

                if (NetworkManager.Singleton.IsHost) // Host
                {
                    // Set data of host network player object
                    NetworkClient connectedClient = NetworkManager.Singleton.ConnectedClients[clientId];
                    connectedClient.PlayerObject.GetComponent<NetworkPlayer>().Init(new NetworkConnectionData(MenuNavigator.MainMenu.PlayerNameText.text, clientId));

                    // Host new game
                    MenuNavigator.Lobby.InitHostMultiplayerGame(playerName);
                    MenuNavigator.SwitchToLobbyScreen();
                }
                else // Client
                {
                    // Join existing game
                    MenuNavigator.Lobby.InitJoinMultiplayerGame();
                    MenuNavigator.SwitchToLobbyScreen();
                }
            }

            else // We are the server and someone joined
            {
                // Write connection data to the network player object
                NetworkClient connectedClient = NetworkManager.Singleton.ConnectedClients[clientId];
                NetworkConnectionData connectionData = ConnectionData[clientId];
                connectedClient.PlayerObject.GetComponent<NetworkPlayer>().Init(connectionData);

                MenuNavigator.Lobby.PlayerJoined(connectionData);

                Debug.Log("NETWORK: Client Connected\n" + connectionData.ToString());
            }
        }

        /// <summary>
        /// Gets executed on the server every time a client disconnects. And also on the client side when they themselves disconnect.
        /// </summary>
        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) // We ourselves just disconnected - afaik this never gets executed
            {
                Debug.Log("NETWORK: We disconnected from the server");
            }
            else // We are the server and someone disconnected
            {
                Debug.Log("NETWORK: Client Disconnected: " + ConnectionData[clientId].ToString());
                MenuNavigator.Lobby.PlayerLeft(clientId);
                ConnectionData.Remove(clientId);
            }
        }

        #endregion

        #region Serialize / Deserialize

        /// <summary>
        /// Serializes any serializable object with the default built-in serialization. Do only use this on very small objects!
        /// </summary>
        public static byte[] Serialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }

        }

        public static object Deserialize(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        #endregion

        public static ET_NetworkManager Singleton
        {
            get
            {
                return GameObject.Find("NetworkManager").GetComponent<ET_NetworkManager>();
            }
        }
    }
}
