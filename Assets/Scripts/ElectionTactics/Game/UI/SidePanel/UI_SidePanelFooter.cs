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
        public GameObject PPWarning;

        private bool awaitingConfirmation = false;
        private float confirmationTimer = 0f;
        private const float CONFIRMATION_DURATION = 3f;

        void Start()
        {
            NextActionButton.onClick.AddListener(DoNextAction);
            PPWarning.gameObject.SetActive(false);
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

        protected override void Update()
        {
            base.Update();
            if (awaitingConfirmation)
            {
                confirmationTimer -= Time.deltaTime;
                if (confirmationTimer <= 0f) CancelConfirmation();
            }
        }

        private void DoNextAction()
        {
            if (Game.State != GameState.PreparationPhase) return;

            bool hasRemainingPP = Game.LocalPlayerParty.PolicyPoints > 0;

            if (hasRemainingPP && !awaitingConfirmation)
            {
                // First click with remaining PP — show warning and wait for confirmation
                awaitingConfirmation = true;
                confirmationTimer = CONFIRMATION_DURATION;
                AudioManager.PlaySound(AudioManager.Instance.AddBot);
                PPWarning.SetActive(true);
                return;
            }

            // Either no remaining PP, or this is the confirmation click
            CancelConfirmation();
            Game.EndTurn();
        }

        private void CancelConfirmation()
        {
            awaitingConfirmation = false;
            confirmationTimer = 0f;
            PPWarning.SetActive(false);
        }

        public void SetBackgroundColor(Color c)
        {
            Background.color = c;
            CancelConfirmation();
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
