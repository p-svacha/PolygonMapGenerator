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
                if (NetworkManager.Singleton.IsHost) return NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<NetworkPlayer>();
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

        #region Generate Map
        [ServerRpc]
        public void GenerateMapServerRpc(int mapSeed, int startGameSeed)
        {
            GenerateMapClientRpc(mapSeed, startGameSeed);
        }
        [ClientRpc]
        private void GenerateMapClientRpc(int mapSeed, int startGameSeed)
        {
            if (IsHost) return;
            ET_NetworkManager.Singleton.Game.StartGameAsClient(mapSeed, startGameSeed);
        }
        #endregion

        #region End Turn
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
        #endregion

        #region End Preparation Phase
        // Election Result
        [ServerRpc]
        public void ConcludePreparationPhaseServerRpc(int seed)
        {
            ConcludePreparationPhaseClientRpc(seed);
        }
        [ClientRpc]
        public void ConcludePreparationPhaseClientRpc(int seed)
        {
            ET_NetworkManager.Singleton.Game.ConcludePreparationPhase(seed);
        }
        #endregion

        #region Change Policy
        [ServerRpc]
        public void ChangePolicyServerRpc(int playerId, int policyId, int value)
        {
            ChangePolicyClientRpc(playerId, policyId, value);
        }
        [ClientRpc]
        public void ChangePolicyClientRpc(int playerId, int policyId, int value)
        {
            if (value == 1) ET_NetworkManager.Singleton.Game.DoIncreasePolicy(playerId, policyId);
            else if(value == -1) ET_NetworkManager.Singleton.Game.DoDecreasePolicy(playerId, policyId);
        }
        #endregion
    }
}
