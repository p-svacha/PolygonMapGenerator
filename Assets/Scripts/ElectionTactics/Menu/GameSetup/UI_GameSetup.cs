using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_GameSetup : MonoBehaviour
    {
        private MenuNavigator MenuNavigator;

        public List<UI_GameSetupPlayer> Players;
        public Button StartGameButton;

        public UI_GameSetupPlayer Host;
        public UI_GameSetupPlayer LocalPlayer;
        private List<UI_GameSetupPlayer> HumanPlayers = new List<UI_GameSetupPlayer>();

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
        }

        public void CreateNewMultiplayerGame(string playerName)
        {
            HumanPlayers.Clear();
            foreach (UI_GameSetupPlayer player in Players) player.SetInactive();
            Players[0].SetActive(playerName, canRemove: false);
            Host = Players[0];
            LocalPlayer = Players[0];
            HumanPlayers.Add(Players[0]);
        }

        public void AddHumanPlayer(string name)
        {
            foreach(UI_GameSetupPlayer player in Players)
            {
                if (!player.IsActive)
                {
                    player.SetActive(name, canRemove: false);
                    HumanPlayers.Add(player);
                    break;
                }
                
            }
        }
    }
}
