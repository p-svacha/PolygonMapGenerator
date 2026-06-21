using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace ElectionTactics
{
    public class TutorialManager : MonoBehaviour
    {
        private static float DEFAULT_UI_SLIDE_TIME = 0.3f;
        private static string HIGHLIGHT = "#F16F55";
        private static int NUM_STEPS = 11;

        public enum TutorialStep
        {
            Welcome,
            District,
            Policies,
            Popularity,
            Seats,
            Election,
            DuringElection,
            Newspaper,
            Parliament,
            NewDistrict,
            CulturalTraits,
            End

        }
        public static TutorialManager Instance { get; private set; }
        public bool IsTutorialActive { get; private set; }
        public TutorialStep CurrentStep { get; private set; }

        [Header("Tutorial Popup")]
        public GameObject TutorialPopup;
        public TextMeshProUGUI StepText;
        public TextMeshProUGUI TutorialText;
        public TextMeshProUGUI ContinueText;
        public Button ContinueButton;
        public TextMeshProUGUI ContinueButtonText;

        [Header("Highlight Arrows")]
        public GameObject ArrowContainer;
        public TutorialArrow DistrictArrow_Map;
        public TutorialArrow DistrictArrow_List;
        public TutorialArrow PolicyTabArrow;
        public TutorialArrow PolicyPointsArrow;
        public TutorialArrow PopularityArrow;
        public TutorialArrow SeatsArrow;
        public TutorialArrow EndTurnArrow;
        public TutorialArrow ElectionControlsArrow;
        public TutorialArrow ParliamentArrow;
        public TutorialArrow NewDistrictArrow;
        public TutorialArrow CulturalTraitsArrow;

        private void Awake()
        {
            Instance = this;
            ContinueButton.onClick.AddListener(ContinueButton_OnClick); 
        }

        private void ContinueButton_OnClick()
        {
            AudioManager.PlayStandardClickSound();

            if (CurrentStep == TutorialStep.Popularity) ShowSeatsStep();
            else if (CurrentStep == TutorialStep.Seats) ShowElectionStep();
            else if (CurrentStep == TutorialStep.Parliament) ShowNewDistrictStep();
            else if (CurrentStep == TutorialStep.CulturalTraits) ShowConclusionStep();
            else if (CurrentStep == TutorialStep.End) EndTutorial();
        }


        private void Update()
        {
            if (!IsTutorialActive) return;

            // World position arrows
            if (CurrentStep == TutorialStep.Welcome)
            {
                District districtToMark = ElectionTacticsGame.Instance.ActiveDistricts.OrderByDescending(x => x.Population).First();
                Vector3 screenPosTarget = districtToMark.MapLabel.transform.position + new Vector3(30f, 70f, 0f);
                DistrictArrow_Map.Position = screenPosTarget;
            }
            if (CurrentStep == TutorialStep.NewDistrict)
            {
                District districtToMark = ElectionTacticsGame.Instance.ActiveDistricts.Last();
                Vector3 screenPosTarget = districtToMark.MapLabel.transform.position + new Vector3(30f, 70f, 0f);
                NewDistrictArrow.Position = screenPosTarget;
            }

            // Check for step transitions
            if (CurrentStep == TutorialStep.Welcome && ElectionTacticsGame.Instance.UI.SelectedDistrict != null)
            {
                ShowDistrictInfoStep();
            }
            if (CurrentStep == TutorialStep.District && ElectionTacticsGame.Instance.UI.ActiveTab == Tab.Policies)
            {
                ShowPolicyStep();
            }
            if (CurrentStep == TutorialStep.Policies && ElectionTacticsGame.Instance.LocalPlayerParty.PolicyPoints == 0)
            {
                ShowPopularityStep();
            }
            if (CurrentStep == TutorialStep.Election && ElectionTacticsGame.Instance.State == GameState.Election)
            {
                ShowDuringElectionStep();
            }
            if (CurrentStep == TutorialStep.DuringElection && ElectionTacticsGame.Instance.State != GameState.Election && UI_ElectionTactics.Instance.Newspaper.ShowAnimationCoroutine == null)
            {
                ShowNewspaperStep();
            }
            if (CurrentStep == TutorialStep.Newspaper && !UI_ElectionTactics.Instance.IsShowingNewspaper)
            {
                ShowParliamentStep();
            }
            if (CurrentStep == TutorialStep.NewDistrict && ElectionTacticsGame.Instance.UI.SelectedDistrict == ElectionTacticsGame.Instance.ActiveDistricts.Last())
            {
                ShowCulturalTraitStep();
            }

            // Popularity step
            if (CurrentStep == TutorialStep.Popularity)
            {
                // We are in district info screen
                if (ElectionTacticsGame.Instance.UI.ActiveTab == Tab.DistrictInfo)
                {
                    TutorialText.text = $"Hover over the popularity value for a detailed breakdown.\n\nElection results are determined by <color={HIGHLIGHT}>Relative Popularity</color> between each party.\n\nYou can't see your opponents' popularity or policies, but you can infer their strength from election results.";
                    ContinueText.text = "";
                    PopularityArrow.gameObject.SetActive(true);
                    ContinueButton.gameObject.SetActive(true);
                }
                // Tell player to switch back to district info screen if they are not
                else
                {
                    TutorialText.text = "Now select any district to see how your policies affect your popularity there.";
                    ContinueText.text = "Select any district to continue.";
                    ContinueButton.gameObject.SetActive(false);
                    PopularityArrow.gameObject.SetActive(false);

                }
            }
        }

        public void StartTutorial()
        {
            // Set tutorial active
            IsTutorialActive = true;
            gameObject.SetActive(true);

            // Reset values from previous playthroughs
            ContinueButtonText.text = "Continue";
            foreach (Transform t in ArrowContainer.transform)
            {
                t.gameObject.SetActive(false);
            }

            // Start tutorial
            ShowWelcomeStep();
        }

        private void ShowWelcomeStep()
        {
            CurrentStep = TutorialStep.Welcome;

            StepText.text = $"1/{NUM_STEPS}";
            TutorialText.text = $"Welcome to Levers of Democracy!\n\nYou lead a political party competing in elections across a fictional country consisting of different <color={HIGHLIGHT}>Districts</color>.";
            ContinueText.text = "Select any district to continue.";
            ContinueButton.gameObject.SetActive(false);

            // Start with header and footer slid out
            ElectionTacticsGame.Instance.UI.ElectionControls.gameObject.SetActive(false);
            ElectionTacticsGame.Instance.UI.SlideOutMapControls(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutStandings(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutHeader(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutFooter(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.HideTabButton(Tab.Parliament);
            ElectionTacticsGame.Instance.UI.HideTabButton(Tab.Newspaper);

            // Arrows
            DistrictArrow_Map.gameObject.SetActive(true);
            DistrictArrow_List.gameObject.SetActive(true);
        }

        private void ShowDistrictInfoStep()
        {
            CurrentStep = TutorialStep.District;

            DistrictArrow_Map.gameObject.SetActive(false);
            DistrictArrow_List.gameObject.SetActive(false);

            StepText.text = $"2/{NUM_STEPS}";
            TutorialText.text = $"Each district has various attributes across different categories.\n\nFor each attribute, there is a matching <color={HIGHLIGHT}>Policy</color>.\n\nSupport the right policies to increase your party's <color={HIGHLIGHT}>Popularity</color> in districts.";
            ContinueText.text = "Switch to the policy screen to continue.";

            ElectionTacticsGame.Instance.UI.SlideInHeader(DEFAULT_UI_SLIDE_TIME);

            PolicyTabArrow.gameObject.SetActive(true);
        }

        private void ShowPolicyStep()
        {
            CurrentStep = TutorialStep.Policies;

            PolicyTabArrow.gameObject.SetActive(false);

            StepText.text = $"3/{NUM_STEPS}";
            TutorialText.text = $"Each election cycle you receive <color={HIGHLIGHT}>{ElectionTacticsGame.PP_PER_CYCLE} Policy Points</color> (PP) to spend freely.\n\nHover over any policy to see its popularity impact on the map.";
            ContinueText.text = "Spend all your PP to continue.";

            PolicyPointsArrow.gameObject.SetActive(true);
        }

        private void ShowPopularityStep()
        {
            CurrentStep = TutorialStep.Popularity;

            PolicyPointsArrow.gameObject.SetActive(false);

            StepText.text = $"4/{NUM_STEPS}";
            // Rest handled in Update
        }

        private void ShowSeatsStep()
        {
            CurrentStep = TutorialStep.Seats;

            PopularityArrow.gameObject.SetActive(false);

            StepText.text = $"5/{NUM_STEPS}";
            TutorialText.text = $"Districts are worth different amounts of <color={HIGHLIGHT}>Seats</color>, based on their population.\n\nUsually, the winning party wins all seats in a district.";
            ContinueText.text = "";

            ContinueButton.gameObject.SetActive(true);
            SeatsArrow.gameObject.SetActive(true);
        }

        private void ShowElectionStep()
        {
            CurrentStep = TutorialStep.Election;

            ContinueButton.gameObject.SetActive(false);
            SeatsArrow.gameObject.SetActive(false);

            StepText.text = $"6/{NUM_STEPS}";
            TutorialText.text = $"All policy points spent. Time to see how your party performs in the first <color={HIGHLIGHT}>General Election</color>!\n\nNote: This locks all policies. Policy points cannot be redistributed after elections.";
            ContinueText.text = "End your turn to continue.";

            ElectionTacticsGame.Instance.UI.ElectionControls.gameObject.SetActive(true);
            ElectionTacticsGame.Instance.UI.SlideInFooter(DEFAULT_UI_SLIDE_TIME);
            EndTurnArrow.gameObject.SetActive(true);
        }

        private void ShowDuringElectionStep()
        {
            CurrentStep = TutorialStep.DuringElection;

            TutorialPopup.gameObject.SetActive(false);
            EndTurnArrow.gameObject.SetActive(false);
        }

        private void ShowNewspaperStep()
        {
            CurrentStep = TutorialStep.Newspaper;

            TutorialPopup.gameObject.SetActive(true);

            StepText.text = $"7/{NUM_STEPS}";
            TutorialText.text = $"After each election, the <color={HIGHLIGHT}>Yearly Newspaper</color> will tell you everything important happening in the country.\n\nMany articles provide important information to shape your policy strategy.";
            ContinueText.text = "Close the newspaper to continue.";
        }

        private void ShowParliamentStep()
        {
            CurrentStep = TutorialStep.Parliament;

            StepText.text = $"8/{NUM_STEPS}";
            TutorialText.text = $"The <color={HIGHLIGHT}>Parliament</color> view shows the seat distribution from the last election and the current overall standings.\n\nYou can also inspect individual district results by selecting them.\n\nBe the first party to <color={HIGHLIGHT}>win {ElectionTacticsGame.Instance.Constitution.WinCondition.ConditionValue} General Elections</color> to win the game.";
            ContinueText.text = "";

            ElectionTacticsGame.Instance.UI.SlideInStandings(slideTime: 0.3f);
            ElectionTacticsGame.Instance.UI.ShowTabButton(Tab.Parliament);
            ElectionTacticsGame.Instance.UI.ShowTabButton(Tab.Newspaper);
            ParliamentArrow.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(true);
        }

        private void ShowNewDistrictStep()
        {
            CurrentStep = TutorialStep.NewDistrict;

            ParliamentArrow.gameObject.SetActive(false);
            ContinueButton.gameObject.SetActive(false);

            StepText.text = $"9/{NUM_STEPS}";
            TutorialText.text = "A new district has been added to the map.\n\nAfter each election, the country grows by one district.";
            ContinueText.text = "Select the new district to continue.";

            NewDistrictArrow.gameObject.SetActive(true);
        }

        private void ShowCulturalTraitStep()
        {
            CurrentStep = TutorialStep.CulturalTraits;

            NewDistrictArrow.gameObject.SetActive(false);

            StepText.text = $"10/{NUM_STEPS}";
            TutorialText.text = $"The new district has <color={HIGHLIGHT}>Cultural Traits</color>.\n\nCultural Traits can change the behaviour of districts in all kinds of ways.\n\nHover over them to see their effects.";
            ContinueText.text = "";

            ContinueButton.gameObject.SetActive(true);
            CulturalTraitsArrow.gameObject.SetActive(true);
        }

        private void ShowConclusionStep()
        {
            CurrentStep = TutorialStep.End;

            CulturalTraitsArrow.gameObject.SetActive(false);

            StepText.text = $"11/{NUM_STEPS}";
            TutorialText.text = $"You now know the basics: match your policies to district traits, build more popularity than your opponents, and win seats.\n\nRemember: You can hover over many elements for a helpful tooltip.\n\n<color={HIGHLIGHT}>Good luck!</color>";
            ContinueText.text = "";
            ContinueButtonText.text = "Finish Tutorial";

            ElectionTacticsGame.Instance.UI.SlideInMapControls(slideTime: 0.3f);
            ContinueButton.gameObject.SetActive(true);
        }

        public void EndTutorial()
        {
            // Set all elements active that may be hidden during the tutorial
            ElectionTacticsGame.Instance.UI.SlideInStandings(0f);
            ElectionTacticsGame.Instance.UI.ElectionControls.gameObject.SetActive(true);
            ElectionTacticsGame.Instance.UI.SlideInFooter(0f);
            ElectionTacticsGame.Instance.UI.SlideInHeader(0f);
            ElectionTacticsGame.Instance.UI.SlideInMapControls(0f);
            ElectionTacticsGame.Instance.UI.ShowTabButton(Tab.Parliament);
            ElectionTacticsGame.Instance.UI.ShowTabButton(Tab.Newspaper);

            // End tutorial
            IsTutorialActive = false;
            gameObject.SetActive(false);
        }
    }

    
}