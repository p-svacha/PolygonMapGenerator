using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics {
    public class UI_SidePanelFooter : UIElement
    {
        private ElectionTacticsGame Game;

        [Header("Elements")]
        public Image Background;
        public Button NextActionButton;
        public Text EndTurnButtonText;
        public Text CountdownText;
        public GameObject CountdownOverlay;

        void Start()
        {
            NextActionButton.onClick.AddListener(DoNextAction);
        }

        public void Init(ElectionTacticsGame game)
        {
            Game = game;
            if(Game.GameType == GameType.Singleplayer)
            {
                CountdownOverlay.gameObject.SetActive(false);
                CountdownText.gameObject.SetActive(false);
                EndTurnButtonText.text = "End Turn";
            }
            else
            {
                CountdownOverlay.gameObject.SetActive(true);
                CountdownText.gameObject.SetActive(true);
            }
        }
    
        private void DoNextAction()
        {
            Game.EndTurn();
        }

        public void SetBackgroundColor(Color c)
        {
            Background.color = c;
        }

        /// <summary>
        /// Updates the end turn button text according to how many players are not ready. Only for multiplayer.
        /// </summary>
        public void UpdateButton()
        {
            if(Game.LocalPlayerParty.IsReady) EndTurnButtonText.text = "Waiting for " + Game.Parties.Where(x => x.IsHuman && !x.IsReady).Count() + " players";
            else EndTurnButtonText.text = "End Turn (" + Game.Parties.Where(x => x.IsHuman && !x.IsReady).Count() + " remaining)";

            float timeRatio = Game.RemainingTime / (float)Game.TurnTime;
            float buttonWidth = Background.GetComponent<RectTransform>().rect.width;
            float offsetValue = buttonWidth - (buttonWidth * timeRatio);
            CountdownOverlay.GetComponent<RectTransform>().offsetMax = new Vector2(-offsetValue, 0);
            CountdownText.text = Game.RemainingTime == 0 ? "" : ((int)Game.RemainingTime).ToString();
        }
    }
}
