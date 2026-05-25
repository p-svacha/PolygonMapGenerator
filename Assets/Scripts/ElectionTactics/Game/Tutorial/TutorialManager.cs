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

        enum TutorialStep
        {
            Welcome,
            District,
            Policies,
            Popularity,
            Election,
            DuringElection,
            Parliament,
            NewDistrict,
            End

        }
        public static TutorialManager Instance { get; private set; }
        public bool IsTutorialActive { get; private set; }
        private TutorialStep CurrentStep;

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
        public TutorialArrow EndTurnArrow;
        public TutorialArrow ElectionControlsArrow;
        public TutorialArrow ParliamentArrow;
        public TutorialArrow NewDistrictArrow;
        public TutorialArrow LensArrow;

        private void Awake()
        {
            Instance = this;
            ContinueButton.onClick.AddListener(ContinueButton_OnClick); 
        }

        private void ContinueButton_OnClick()
        {
            AudioManager.PlayStandardClickSound();

            if (CurrentStep == TutorialStep.Popularity) ShowElectionStep();
            else if (CurrentStep == TutorialStep.Parliament) ShowNewDistrictStep();
            else if (CurrentStep == TutorialStep.NewDistrict) ShowConclusionStep();
            else if (CurrentStep == TutorialStep.End) Hide();
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
            if (CurrentStep == TutorialStep.DuringElection && ElectionTacticsGame.Instance.State != GameState.Election)
            {
                ShowParliamentStep();
            }

            // Popularity step
            if (CurrentStep == TutorialStep.Popularity)
            {
                // We are in district info screen
                if (ElectionTacticsGame.Instance.UI.ActiveTab == Tab.DistrictInfo)
                {
                    TutorialText.text = "Hover over the popularity value for a detailed breakdown.\n\nElection results are determined by RELATIVE popularity between each party. You can't see your opponents' popularity or policies, but you can infer their strength from election results.";
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
            IsTutorialActive = true;

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

            StepText.text = "1/9";
            TutorialText.text = "Welcome to Levers of Democracy!\n\nYou lead a political party competing in elections across a fictional country. Win seats by being the most popular party in a district.";
            ContinueText.text = "Select any district to continue.";
            ContinueButton.gameObject.SetActive(false);

            // Start with header and footer slid out
            ElectionTacticsGame.Instance.UI.ElectionControls.gameObject.SetActive(false);
            ElectionTacticsGame.Instance.UI.SlideOutMapControls(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutStandings(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutHeader(slideTime: 0f);
            ElectionTacticsGame.Instance.UI.SlideOutFooter(slideTime: 0f);

            // Arrows
            DistrictArrow_Map.gameObject.SetActive(true);
            DistrictArrow_List.gameObject.SetActive(true);
        }

        private void ShowDistrictInfoStep()
        {
            CurrentStep = TutorialStep.District;

            DistrictArrow_Map.gameObject.SetActive(false);
            DistrictArrow_List.gameObject.SetActive(false);

            StepText.text = "2/9";
            TutorialText.text = "On the right you see the district info screen. Each district has various TRAITS across different categories.\n\nFor each trait, there is a matching POLICY. Supporting a policy increases your party's POPULARITY in districts that have the matching trait.";
            ContinueText.text = "Switch to the policy screen to continue.";

            ElectionTacticsGame.Instance.UI.SlideInHeader(DEFAULT_UI_SLIDE_TIME);

            PolicyTabArrow.gameObject.SetActive(true);
        }

        private void ShowPolicyStep()
        {
            CurrentStep = TutorialStep.Policies;

            PolicyTabArrow.gameObject.SetActive(false);

            StepText.text = "3/9";
            TutorialText.text = $"Here you can set your POLICIES. Each election cycle you receive {ElectionTacticsGame.PP_PER_CYCLE} Policy Points (PP) to spend freely.\n\nHover over any policy to see its impact on the map.";
            ContinueText.text = "Spend all your PP to continue.";

            PolicyPointsArrow.gameObject.SetActive(true);
        }

        private void ShowPopularityStep()
        {
            CurrentStep = TutorialStep.Popularity;

            PolicyPointsArrow.gameObject.SetActive(false);

            StepText.text = "4/9";
            // Rest handled in Update
        }

        private void ShowElectionStep()
        {
            CurrentStep = TutorialStep.Election;

            PopularityArrow.gameObject.SetActive(false);
            ContinueButton.gameObject.SetActive(false);

            StepText.text = "5/9";
            TutorialText.text = "All policy points spent. Time to see how your party performs in the first general election!\n\nNote: This locks all policies. PP can cannot be redistributed.";
            ContinueText.text = "End your turn to continue.";

            ElectionTacticsGame.Instance.UI.ElectionControls.gameObject.SetActive(true);
            ElectionTacticsGame.Instance.UI.SlideInFooter(DEFAULT_UI_SLIDE_TIME);
            EndTurnArrow.gameObject.SetActive(true);
        }

        private void ShowDuringElectionStep()
        {
            CurrentStep = TutorialStep.DuringElection;

            EndTurnArrow.gameObject.SetActive(false);

            StepText.text = "6/9";
            TutorialText.text = "All you can do during the election is watch the results come in. You can speed up the process if you want.";
            ContinueText.text = "Wait for the election to finish.";

            ElectionControlsArrow.gameObject.SetActive(true);
        }

        private void ShowParliamentStep()
        {
            CurrentStep = TutorialStep.Parliament;

            ElectionControlsArrow.gameObject.SetActive(false);

            StepText.text = "7/9";
            TutorialText.text = "The first election is over.\n\nThe parliament view shows the seat distribution from the last election and the current overall standings.\n\nYou can also inspect individual district results by selecting them.";
            ContinueText.text = "";

            ElectionTacticsGame.Instance.UI.SlideInStandings(slideTime: 0.3f);
            ParliamentArrow.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(true);
        }

        private void ShowNewDistrictStep()
        {
            CurrentStep = TutorialStep.NewDistrict;

            ParliamentArrow.gameObject.SetActive(false);

            StepText.text = "8/9";
            TutorialText.text = "A new district has been added to the map. After each election, the country grows by one district.\n\nTip: Use the map overlay to color districts by traits like religion, language, or your popularity.";

            ElectionTacticsGame.Instance.UI.SlideInMapControls(slideTime: 0.3f);

            LensArrow.gameObject.SetActive(true);
            NewDistrictArrow.gameObject.SetActive(true);
        }

        private void ShowConclusionStep()
        {
            CurrentStep = TutorialStep.End;

            LensArrow.gameObject.SetActive(false);
            NewDistrictArrow.gameObject.SetActive(false);

            StepText.text = "9/9";
            TutorialText.text = "You now know the basics: match your policies to district traits, build popularity, and win seats.\n\nRemember: You can hover over many elements for a helpful tooltip.\n\nGood luck!";
            ContinueText.text = "";
            ContinueButtonText.text = "Finish Tutorial";
        }

        public void Hide()
        {
            IsTutorialActive = false;
            gameObject.SetActive(false);
        }
    }

    
}