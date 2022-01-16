using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace ElectionTactics
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public ulong ClientId;
        public string Name;

        public void Init(NetworkConnectionData connectionData)
        {
            ClientId = connectionData.ClientId;
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

        public static ulong LocalClientId
        {
            get
            {
                return NetworkManager.Singleton.LocalClientId;
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
        public void StartGameServerRpc(int seed)
        {
            StartGameClientRpc(seed);
        }
        [ClientRpc]
        private void StartGameClientRpc(int seed)
        {
            if (IsHost) return;
            ET_NetworkManager.Singleton.Game.StartGameAsClient(seed);
        }
        #endregion

        #region Update Districts
        [ServerRpc]
        public void InitCycleServerRpc()
        {
            KeyValuePair<Region, District> newDistrict = ET_NetworkManager.Singleton.Game.InvisibleDistricts.First();
            InitCycleClientRpc(ET_NetworkManager.Serialize(newDistrict.Value.Seed), newDistrict.Value.Name, newDistrict.Key.Id);
        }
        [ClientRpc]
        private void InitCycleClientRpc(byte[] newDistrictSeed, string newDistrictName, int newDistrictRegionId)
        {
            if (IsHost) return;
            Random.State districtSeed = (Random.State)ET_NetworkManager.Deserialize(newDistrictSeed);
            ET_NetworkManager.Singleton.Game.StartNextElectionCycleClient(districtSeed, newDistrictName, newDistrictRegionId);
        }
        #endregion
        /// <summary>
        /// Call to the server from any client (including host) that a player has ended their turn.
        /// </summary>
        [ServerRpc]
        public void EndTurnServerRpc(int playerId)
        {
            EndTurnClientRpc(playerId);
        }
        [ClientRpc]
        public void EndTurnClientRpc(int playerId)
        {
            ET_NetworkManager.Singleton.Game.OnPlayerReady(playerId);
        }
        #region End Turn

        #endregion

    }
}
