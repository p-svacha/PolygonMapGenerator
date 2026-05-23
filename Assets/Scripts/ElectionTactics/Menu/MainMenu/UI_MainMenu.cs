using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_MainMenu : MonoBehaviour
    {
        private MenuNavigator MenuNavigator;

        public TMP_InputField PlayerNameInput;
        public TextMeshProUGUI PlayerNameText;
        public Button PlayerNameRandomizeButton;
        public Button PlayerColorButton;

        public Button TutorialButton;
        public Image TutorialCheckmark;

        public Button QuickPlayButton;
        public Button SingleplayerButton;
        public Button HostGameButton;
        public Button JoinGameButton;
        public Button ExitButton;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;
            PlayerNameRandomizeButton.onClick.AddListener(RandomizePlayerPartyName);
            PlayerColorButton.onClick.AddListener(RandomizePlayerPartyColor);
            TutorialButton.onClick.AddListener(ToggleTutorial);
            QuickPlayButton.onClick.AddListener(QuickPlay);
            SingleplayerButton.onClick.AddListener(CreateSingleplayerGame);
            HostGameButton.onClick.AddListener(HostGame);
            JoinGameButton.onClick.AddListener(JoinGame);
            ExitButton.onClick.AddListener(() => Application.Quit());

            // Initial values
            PlayerNameInput.text = "Your Party Name";
            SetPlayerColor(PartyNameGenerator.GetDefaultPlayerColor());
            SetTutorialEnabled(true);

            // Focus name input
            StartCoroutine(FocusInput());
        }

        private IEnumerator FocusInput()
        {
            yield return null;

            PlayerNameInput.ActivateInputField();

            // Move caret to end without selecting
            yield return null; // Wait one more frame for activation to complete
            PlayerNameInput.caretPosition = PlayerNameInput.text.Length;
            PlayerNameInput.selectionAnchorPosition = PlayerNameInput.text.Length;
            PlayerNameInput.selectionFocusPosition = PlayerNameInput.text.Length;
        }

        private void RandomizePlayerPartyName()
        {
            PlayerNameInput.text = PartyNameGenerator.GetRandomPartyName(maxLength: 32);
        }

        private void RandomizePlayerPartyColor()
        {
            Color newColor = PartyNameGenerator.GetRandomColor(new List<Color>() { PlayerNameText.color });
            SetPlayerColor(newColor);
        }
        private void SetPlayerColor(Color color)
        {
            PlayerNameText.color = color;
            PlayerColorButton.GetComponent<Image>().color = color;
        }

        private void ToggleTutorial()
        {
            bool isEnabled = TutorialCheckmark.gameObject.activeSelf;
            SetTutorialEnabled(!isEnabled);
        }

        private void SetTutorialEnabled(bool value)
        {
            TutorialCheckmark.gameObject.SetActive(value);
        }

        private void CreateSingleplayerGame()
        {
            MenuNavigator.CreateSingleplayerGame();
        }

        private void HostGame()
        {
            ET_NetworkManager.Singleton.HostGame();
        }

        private void JoinGame()
        {
            ET_NetworkManager.Singleton.JoinGame();
        }

        #region Quick Play

        private void QuickPlay()
        {
            GameSettings quickPlaySettings = GetQuickPlaySettings();
            MenuNavigator.InitGame(quickPlaySettings);
        }

        private GameSettings GetQuickPlaySettings()
        {
            // Create empty slots
            List<LobbySlot> slots = new List<LobbySlot>();
            for(int i = 0; i < 8; i++)
            {
                slots.Add(new LobbySlot(i, LobbySlotType.Inactive));
            }

            // Add player
            slots[0].SetActive(PlayerNameInput.text, PlayerNameText.color, LobbySlotType.Human, 0);
            List<Color> usedColors = new List<Color>() { PlayerNameText.color };

            // Add 3 bots
            for (int i = 1; i <= 3; i++)
            {
                slots[i].SetActive(PartyNameGenerator.GetRandomPartyName(), PartyNameGenerator.GetRandomColor(usedColors), LobbySlotType.Bot, 0);
                usedColors.Add(slots[i].GetColor());
            }

            // Other default settings
            GameModeDef mode = GameModeDefOf.Classic;
            TurnLengthDef turnLength = TurnLengthDefOf.Medium;
            BotDifficultyDef botDifficulty = BotDifficultyDefOf.Standard;

            // Tutorial
            bool isTutorialEnabled = TutorialCheckmark.gameObject.activeSelf;

            return new GameSettings(slots, mode, turnLength, botDifficulty, isTutorialEnabled);
        }

        #endregion
    }


}
    