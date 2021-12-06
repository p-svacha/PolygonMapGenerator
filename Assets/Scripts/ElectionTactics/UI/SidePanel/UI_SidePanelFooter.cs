using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics {
    public class UI_SidePanelFooter : UIElement
    {
        private ElectionTacticsGame Game;

        [Header("Elements")]
        public Button NextActionButton;

        void Start()
        {
            NextActionButton.onClick.AddListener(DoNextAction);
        }

        public void Init(ElectionTacticsGame game)
        {
            Game = game;
        }
    
        private void DoNextAction()
        {
            Game.ConcludePreparationPhase();
        }
    }
}
