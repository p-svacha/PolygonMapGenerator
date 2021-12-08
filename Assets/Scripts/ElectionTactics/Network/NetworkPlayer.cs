using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ElectionTactics
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public string Name;

        public void Init(NetworkConnectionData connectionData)
        {
            Name = connectionData.Name;
        }

        public static NetworkPlayer Server
        {
            get
            {
                if(NetworkManager.Singleton.IsHost) return NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<NetworkPlayer>();
                else return NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
            }
        }

        #region Update Lobby
        [ServerRpc]
        public void UpdateLobbyServerRpc()
        {
            UpdateLobbyClientRpc(ET_NetworkManager.Serialize(ET_NetworkManager.Singleton.MenuNavigator.Lobby.Slots));
        }
        [ClientRpc]
        private void UpdateLobbyClientRpc(byte[] data)
        {
            ET_NetworkManager.Singleton.MenuNavigator.Lobby.SetSlotsFromServer(data);
        }
        #endregion

        #region Initialize Game
        [ServerRpc]
        public void InitGameServerRpc()
        {
            InitGameClientRpc();
        }
        [ClientRpc]
        private void InitGameClientRpc()
        {
            if (IsHost) return;
            ET_NetworkManager.Singleton.MenuNavigator.StartAndJoinGame();
        }
        #endregion

        #region Start Game
        [ServerRpc]
        public void StartGameServerRpc()
        {
            byte[] mapData = ET_NetworkManager.Serialize(ET_NetworkManager.Singleton.Game.Map);
            StartGameClientRpc(mapData);
        }
        [ClientRpc]
        private void StartGameClientRpc(byte[] mapData)
        {
            if (IsHost) return;

            Map map = (Map) ET_NetworkManager.Deserialize(mapData);
            if (!IsHost) ET_NetworkManager.Singleton.Game.StartGameAsClient(map);
        }
        #endregion


    }
}
