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
            QuitToDesktopButton.onClick.AddListener(() => Application.Quit());
        }

        public void Init(ElectionTacticsGame game)
        {
            Container.SetActive(true);
            TitleText.text = $"{game.WinnerParty.Name} has won the game!";
        }
    }
}
