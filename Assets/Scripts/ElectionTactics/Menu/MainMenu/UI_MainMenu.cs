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

        // Logo
        public Button LogoLeverButton;
        public Image LogoElectionBar1;
        public Image LogoElectionBar2;
        public Image LogoElectionBar3;

        [Header("Logo Animation")]
        public RectTransform LogoLeverTransform; // The lever's RectTransform (pivot set in sprite editor)

        private bool leverToggled = false;
        private bool isAnimating = false;
        private float animationTime = 0.3f;
        private float animationDelay;

        // Lever states
        private Quaternion leverRotationOff;
        private Quaternion leverRotationOn;
        private Quaternion leverSourceRot;
        private Quaternion leverTargetRot;
        private float leverPosOff, leverPosOn;
        private float leverSourcePos, leverTargetPos;

        // Bar states
        private float bar1Source, bar1Target;
        private float bar2Source, bar2Target;
        private float bar3Source, bar3Target;

        public void Init(MenuNavigator nav)
        {
            MenuNavigator = nav;

            // Logo
            LogoLeverButton.onClick.AddListener(ToggleLever);
            LogoLeverButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            // Capture the lever's resting rotation (the "off" state as authored in the scene)
            leverRotationOff = LogoLeverTransform.localRotation;
            leverRotationOn = leverRotationOff * Quaternion.Euler(0, 0, 35); // First toggle is CCW (+35°)
            leverPosOff = LogoLeverTransform.anchoredPosition.x;
            leverPosOn = leverPosOff - 50f; // Move left when toggled on

            // Menu buttons
            PlayerNameRandomizeButton.onClick.AddListener(RandomizePlayerPartyName);
            PlayerNameRandomizeButton.onClick.AddListener(() => AudioManager.PlaySound(AudioManager.Instance.Chimes));

            PlayerColorButton.onClick.AddListener(RandomizePlayerPartyColor);
            PlayerColorButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());
            PlayerColorButton.GetComponent<RightClickHandler>().OnRightClick = CyclePlayerColorBackward;

            TutorialButton.onClick.AddListener(ToggleTutorial);
            TutorialButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            QuickPlayButton.onClick.AddListener(QuickPlay);
            QuickPlayButton.onClick.AddListener(() => AudioManager.PlayStartGameSound());

            SingleplayerButton.onClick.AddListener(CreateSingleplayerGame);
            SingleplayerButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            HostGameButton.onClick.AddListener(HostGame);
            HostGameButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            JoinGameButton.onClick.AddListener(JoinGame);
            JoinGameButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

            ExitButton.onClick.AddListener(() => Application.Quit());
            ExitButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound());

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
            PlayerNameInput.text = PartyNameGenerator.GetRandomPartyName();
        }

        private void RandomizePlayerPartyColor()
        {
            Color newColor = PartyNameGenerator.GetRandomColor(new List<Color>(), PlayerNameText.color);
            SetPlayerColor(newColor);
        }
        private void SetPlayerColor(Color color)
        {
            PlayerNameText.color = color;
            PlayerColorButton.GetComponent<Image>().color = color;
        }
        private void CyclePlayerColorBackward()
        {
            Color newColor = PartyNameGenerator.GetRandomColorBackward(new List<Color>(), PlayerNameText.color);
            SetPlayerColor(newColor);
            AudioManager.PlayStandardClickSound();
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

        #region Logo

        private void ToggleLever()
        {
            if (isAnimating) return; // Ignore clicks mid-animation so states don't break

            AudioManager.PlaySound(AudioManager.Instance.Lever, volume: 0.45f);
            leverToggled = !leverToggled;

            // Calculate lever target up front
            leverSourceRot = LogoLeverTransform.localRotation;
            leverTargetRot = leverToggled ? leverRotationOn : leverRotationOff;
            leverSourcePos = LogoLeverTransform.anchoredPosition.x;
            leverTargetPos = leverToggled ? leverPosOn : leverPosOff;

            // Calculate bar targets
            bar1Source = LogoElectionBar1.rectTransform.anchoredPosition.y;
            bar2Source = LogoElectionBar2.rectTransform.anchoredPosition.y;
            bar3Source = LogoElectionBar3.rectTransform.anchoredPosition.y;

            bar1Target = Random.Range(345f, 420f);
            bar2Target = Random.Range(315f, 390f);
            bar3Target = Random.Range(300f, 375f);

            animationDelay = 0f;
            isAnimating = true;
        }

        private void Update()
        {
            if (!isAnimating) return;

            animationDelay += Time.deltaTime;
            float r = Mathf.Clamp01(animationDelay / animationTime);

            // Lever rotation
            LogoLeverTransform.localRotation = Quaternion.Lerp(leverSourceRot, leverTargetRot, r);
            LogoLeverTransform.anchoredPosition = new Vector2(Mathf.Lerp(leverSourcePos, leverTargetPos, r), LogoLeverTransform.anchoredPosition.y);

            // Bars
            SetBarY(LogoElectionBar1, Mathf.Lerp(bar1Source, bar1Target, r));
            SetBarY(LogoElectionBar2, Mathf.Lerp(bar2Source, bar2Target, r));
            SetBarY(LogoElectionBar3, Mathf.Lerp(bar3Source, bar3Target, r));

            if (r >= 1f)
            {
                // Snap to exact final state
                LogoLeverTransform.localRotation = leverTargetRot;
                SetBarY(LogoElectionBar1, bar1Target);
                SetBarY(LogoElectionBar2, bar2Target);
                SetBarY(LogoElectionBar3, bar3Target);
                isAnimating = false;
            }
        }

        private void SetBarY(Image bar, float y)
        {
            Vector2 pos = bar.rectTransform.anchoredPosition;
            pos.y = y;
            bar.rectTransform.anchoredPosition = pos;
        }

        #endregion

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

            // Tutorial
            bool isTutorialEnabled = TutorialCheckmark.gameObject.activeSelf;

            // Other default settings
            GameModeDef mode = GameModeDefOf.Classic;
            TurnLengthDef turnLength = TurnLengthDefOf.Medium;
            BotDifficultyDef botDifficulty = BotDifficultyDefOf.Standard;
            GameLengthDef gameLength = isTutorialEnabled ? GameLengthDefOf.Short : GameLengthDefOf.Standard;
            StartingDistrictsDef startingDistricts = isTutorialEnabled ? StartingDistrictsDefOf.Two : StartingDistrictsDefOf.Three;

            return new GameSettings(slots, mode, turnLength, botDifficulty, gameLength, startingDistricts, isTutorialEnabled);
        }

        #endregion
    }


}
    