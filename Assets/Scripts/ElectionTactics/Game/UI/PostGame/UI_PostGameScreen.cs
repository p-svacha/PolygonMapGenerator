using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PostGameScreen : MonoBehaviour
    {
        [Header("Elements")]
        public GameObject Container;
        public Text TitleText;
        public Button QuitToDesktopButton;

        void Start()
        {
            //QuitToDesktopButton.onClick.AddListener(() => Application.Quit());
            QuitToDesktopButton.onClick.AddListener(BackToMenuButton_OnClick);
        }

        private void BackToMenuButton_OnClick()
        {
            AudioManager.PlayStandardClickSound();
            Container.gameObject.SetActive(false);
            ElectionTacticsGame.Instance.MenuNavigator.SwitchToMainMenuScreen();
        }

        public void Init(ElectionTacticsGame game)
        {
            Container.SetActive(true);

            if (game.WinnerParty == game.LocalPlayerParty)
            {
                TitleText.text = "You have won the game!\n\nCongratulations!";
                AudioManager.PlaySound(AudioManager.Instance.WinGame);
            }
            else
            {
                TitleText.text = $"You have lost the game.\n\nBetter luck next time.";
                AudioManager.PlaySound(AudioManager.Instance.LoseGame);
            }
        }
    }
}
