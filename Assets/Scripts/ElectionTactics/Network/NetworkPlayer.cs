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
        public void GenerateMapServerRpc(int seed)
        {
            GenerateMapClientRpc(seed);
        }
        [ClientRpc]
        private void GenerateMapClientRpc(int seed)
        {
            if (IsHost) return;
            ET_NetworkManager.Singleton.Game.StartGameAsClient(seed);
        }
        #endregion

        #region Start Game
        [ServerRpc]
        public void StartGameServerRpc()
        {
            KeyValuePair<Region, District> firstDistrict = ET_NetworkManager.Singleton.Game.InvisibleDistricts.First();
            StartGameClientRpc(ET_NetworkManager.Serialize(firstDistrict.Value.Seed), firstDistrict.Value.Name, firstDistrict.Key.Id);
        }
        [ClientRpc]
        private void StartGameClientRpc(byte[] newDistrictSeed, string newDistrictName, int newDistrictRegionId)
        {
            if (IsHost) return;
            Random.State districtSeed = (Random.State)ET_NetworkManager.Deserialize(newDistrictSeed);
            ET_NetworkManager.Singleton.Game.StartGameClient(districtSeed, newDistrictName, newDistrictRegionId);
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
        [ServerRpc]
        public void ConcludePreparationPhaseServerRpc()
        {
            GeneralElectionResult electionResult = ET_NetworkManager.Singleton.Game.GetLatestElectionResult();
            byte[] electionResultData = ET_NetworkManager.Serialize(electionResult);
            ConcludePreparationPhaseClientRpc(electionResultData);
        }
        [ClientRpc]
        public void ConcludePreparationPhaseClientRpc(byte[] electionResultData)
        {
            if (IsHost) return;
            GeneralElectionResult electionResult = (GeneralElectionResult)ET_NetworkManager.Deserialize(electionResultData);
            foreach (DistrictElectionResult districtResult in electionResult.DistrictResults) districtResult.InitReferences(ET_NetworkManager.Singleton.Game);
            ET_NetworkManager.Singleton.Game.ConcludePreparationPhaseClient(electionResult);
        }
        #endregion

    }
}
