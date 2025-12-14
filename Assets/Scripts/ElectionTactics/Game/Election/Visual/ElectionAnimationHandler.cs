using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Handles the client-side animation that plays during an election. Does only show and not change any game values.
    /// </summary>
    public class ElectionAnimationHandler
    {
        private ElectionTacticsGame Game;

        private enum AnimationState
        {
            Inactive,
            Wait,

            InitAnimation,

            InitNextDistrict,
            MoveToNextDistrict,

            InitShowModifiers,
            InitShowNextModifier,
            ShowModifier,

            InitGraphAnimation,
            GraphAnimation,

            InitApplyDistrictResult,
            ApplyDistrictResult,

            InitApplyElectionResult,
            ApplyElectionResult,

            InitEndAnimation,
            EndAnimation
        }
        private AnimationState State;
        private AnimationState LastState;

        private GeneralElectionResult ElectionResult;
        private Dictionary<Party, int> TempSeats = new Dictionary<Party, int>();
        private Dictionary<Party, int> TempStandingsScore = new Dictionary<Party, int>();

        private List<District> DistrictOrder;
        private int CurElectionDistrictIndex;
        private DistrictElectionResult CurrentDistrictResult;
        private int CurModifierIndex;
        private int NumModifiers;

        // Election animation wait times
        private float CurrentWaitTime;
        private float CurrentWaitDelay;
        private AnimationState PostWaitState;

        private float UiControlsSlideTime = 0.3f;
        private float DistrictPanTime = 2f;
        private float PostDistrictPanPauseTime = 1f; // Length of pause after moving to a district
        private float ModifierSlideTime = 1f; // Length of slide animation per modifier
        private float PostModifierSlideTime = 1.5f; // Length of pause after a modifier
        private float GraphAnimationTime = 6f;
        private float PostGraphPauseTime = 0.8f;
        private float ListAnimationTime = 1f;
        private float ScoreTokenFlyingTime = 2f; // How long it takes for the damage tokens (i.e. "-5") to fly from the district label seat text to the standings panel 
        private float PostPartyListAnimationPauseTime = 1f;

        private float AnimationSpeedModifier = 1f;
        private Dictionary<ElectionAnimationSpeed, float> AnimationSpeeds = new Dictionary<ElectionAnimationSpeed, float>()
        {
            { ElectionAnimationSpeed.Normal, 1.25f },
            { ElectionAnimationSpeed.Fast, 2.5f },
            { ElectionAnimationSpeed.VeryFast, 5f },
        };

        public ElectionAnimationHandler(ElectionTacticsGame game)
        {
            Game = game;
            State = AnimationState.Inactive;
        }

        public void StartAnimation(GeneralElectionResult result)
        {
            ElectionResult = result;
            State = AnimationState.InitAnimation;
        }

        public void Update()
        {
            if (State != LastState)
            {
                string toString = State == AnimationState.Wait ? $"{State} ({PostWaitState})" : State.ToString();
                Debug.Log("Changing animation state : " + LastState.ToString() + " --> " + toString);
                LastState = State;
            }
            

            switch (State)
            {
                case AnimationState.Wait:
                    if (CurrentWaitDelay >= CurrentWaitTime) State = PostWaitState;
                    else CurrentWaitDelay += Time.deltaTime * AnimationSpeedModifier;
                    break;

                case AnimationState.InitAnimation:
                    InitAnimation();
                    State = AnimationState.InitNextDistrict;
                    break;

                case AnimationState.InitNextDistrict:
                    InitMoveToNextDistrict();
                    break;

                case AnimationState.MoveToNextDistrict:
                    // Wait for callback from camera movement 
                    break;

                case AnimationState.InitShowModifiers:
                    InitShowModifiers();
                    State = AnimationState.InitShowNextModifier;
                    break;

                case AnimationState.InitShowNextModifier:
                    InitShowNextModifier();
                    State = AnimationState.ShowModifier;
                    break;

                case AnimationState.ShowModifier:
                    // Wait for callback from modifier animation
                    break;

                case AnimationState.InitGraphAnimation:
                    InitGraphAnimation();
                    State = AnimationState.GraphAnimation;
                    break;

                case AnimationState.GraphAnimation:
                    // Wait for callback from graph animation
                    break;

                case AnimationState.InitApplyDistrictResult:
                    InitDistrictResultApplication();
                    State = AnimationState.ApplyDistrictResult;
                    break;

                case AnimationState.ApplyDistrictResult:
                    // Wait for callback from party list update animation
                    break;

                case AnimationState.InitApplyElectionResult:
                    InitElectionResultApplication();
                    State = AnimationState.ApplyElectionResult;
                    break;

                case AnimationState.ApplyElectionResult:
                    // Wait for callback from score update
                    break;

                case AnimationState.InitEndAnimation:
                    InitEndAnimation();
                    State = AnimationState.EndAnimation;
                    break;

                case AnimationState.EndAnimation:
                    // Wait for callback from zoom out
                    break;
            }
        }

        private void InitAnimation()
        {
            // Change UI controls
            Game.UI.SidePanelHeader.Slide(new Vector2(0, 60), UiControlsSlideTime);
            Game.UI.SidePanelFooter.Slide(new Vector2(0, -60), UiControlsSlideTime);
            Game.UI.ElectionControls.DoSetSpeed(ElectionAnimationSpeed.Normal);

            // Hide map controls (colors of winning parties will be set during animation)
            Game.UI.MapControls.SetMapDisplayMode(MapDisplayMode.NoOverlay, DistrictLabelMode.InElection);
            Game.UI.MapControls.gameObject.SetActive(false);

            // Prepare election animation
            Game.UI.Parliament.ParliamentPartyList.ShowEliminatedParties = false;
            Game.UI.SelectTab(Tab.Parliament);
            Game.UI.Parliament.ParliamentTitle.text = $"Cycle {ElectionResult.ElectionCycle} Parliament";
            Game.UI.Parliament.StandingsContainer.SetActive(false);

            // Clear visual seats
            TempSeats.Clear();
            foreach (Party p in Game.Parties) TempSeats.Add(p, 0);
            Game.UI.Parliament.ParliamentPartyList.Init(TempSeats, dynamic: true);

            // Init visual standings
            TempStandingsScore.Clear();
            foreach (Party p in Game.Parties) TempStandingsScore.Add(p, p.PreviousScore);

            DistrictOrder = ElectionResult.DistrictResults.Select(x => x.District).OrderBy(x => x.Population).ToList();
            CurElectionDistrictIndex = -1;
        }


        // District movement
        private void InitMoveToNextDistrict()
        {
            CurElectionDistrictIndex++;

            // Clear graph and highlight from last district
            if (CurrentDistrictResult != null) CurrentDistrictResult.District.Region.SetAnimatedHighlight(false);
            Game.UI.Parliament.CurrentElectionGraph.ClearGraph();
            Game.UI.Parliament.ModifierSliderContainer.ClearContainer();

            if (CurElectionDistrictIndex < DistrictOrder.Count)
            {
                // Update current result
                CurrentDistrictResult = DistrictOrder[CurElectionDistrictIndex].GetLatestElectionResult();
                CurrentDistrictResult.District.Region.SetAnimatedHighlight(true);
                TempSeats[CurrentDistrictResult.WinnerParty] += CurrentDistrictResult.Seats;
                if (Game.IsBattleRoyale)
                    foreach (Party p in CurrentDistrictResult.NonWinners)
                        TempStandingsScore[p] -= CurrentDistrictResult.Seats;


                // Prepare graph for next district
                if (CurrentDistrictResult.District.ElectionResults.Count > 1)
                {
                    Game.UI.Parliament.CurrentElectionMarginText.gameObject.SetActive(true);
                    Game.UI.Parliament.LastElectionWinnerKnob.gameObject.SetActive(true);
                    Game.UI.Parliament.CurrentElectionMarginText.text = CurrentDistrictResult.District.GetLatestElectionResult(offset: 1).GetMargin(Game.LocalPlayerParty);
                    Game.UI.Parliament.LastElectionWinnerKnob.color = CurrentDistrictResult.District.GetLatestElectionResult(offset: 1).WinnerParty.Color;
                }
                else
                {
                    Game.UI.Parliament.CurrentElectionMarginText.gameObject.SetActive(false);
                    Game.UI.Parliament.LastElectionWinnerKnob.gameObject.SetActive(false);
                }


                List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
                foreach (KeyValuePair<Party, float> kvp in CurrentDistrictResult.VoteShare)
                    dataPoints.Add(new GraphDataPoint(kvp.Key.Acronym, kvp.Value, kvp.Key.Color));
                int yMax = (((int)CurrentDistrictResult.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;

                // Update current election graph
                Game.UI.Parliament.CurrentElectionContainer.SetActive(true);
                Game.UI.Parliament.CurrentElectionTitle.text = CurrentDistrictResult.District.Name;
                Game.UI.Parliament.CurrentElectionSeatsText.text = CurrentDistrictResult.Seats.ToString();
                Game.UI.Parliament.CurrentElectionSeatsIcon.gameObject.SetActive(true);
                Game.UI.Parliament.CurrentElectionGraph.InitAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, Game.UI.Font, GraphAnimationTime, startAnimation: false);

                Game.CameraHandler.MoveToFocusDistricts(new List<District>() { CurrentDistrictResult.District }, DistrictPanTime, OnCameraMoveToNextDistrictDone);
                State = AnimationState.MoveToNextDistrict;
            }
            else
            {
                State = AnimationState.InitApplyElectionResult;
            }
        }

        private void OnCameraMoveToNextDistrictDone()
        {
            AnimationState nextState = CurrentDistrictResult.Modifiers.Count == 0 ? AnimationState.InitGraphAnimation : AnimationState.InitShowModifiers;
            InitWaitTime(PostDistrictPanPauseTime, nextState);
        }

        // Modifiers
        private void InitShowModifiers()
        {
            CurModifierIndex = 0;
            NumModifiers = CurrentDistrictResult.Modifiers.Count;
        }
        private void InitShowNextModifier()
        {
            Game.UI.Parliament.ModifierSliderContainer.SlideInModifier(CurrentDistrictResult.Modifiers[CurModifierIndex], ModifierSlideTime, OnModfierSlideDone);
            CurModifierIndex++;
        }
        private void OnModfierSlideDone()
        {
            AnimationState nextState = CurModifierIndex == NumModifiers ? AnimationState.InitGraphAnimation : AnimationState.InitShowNextModifier;
            InitWaitTime(PostModifierSlideTime, nextState);
        }

        // Graph animation
        private void InitGraphAnimation()
        {
            Game.UI.Parliament.CurrentElectionGraph.StartAnimation(OnGraphAnimationDone);
        }
        private void OnGraphAnimationDone()
        {
            InitWaitTime(PostGraphPauseTime, AnimationState.InitApplyDistrictResult);
        }

        /// <summary>
        /// Gets called right when the district result is decided in the animation.
        /// </summary>
        private void InitDistrictResultApplication()
        {
            SetColorOfCurrentDistrictResult();

            Game.UI.Parliament.ParliamentPartyList.HighlightParty(CurrentDistrictResult.WinnerParty);
            Game.UI.Parliament.ParliamentPartyList.MovePositionsAnimated(TempSeats, ListAnimationTime, callback: OnPartyListUpdateAnimationDone);

            // Init flying damage tokens from district label towards standings panel
            if (Game.IsBattleRoyale)
            {
                foreach (Party p in CurrentDistrictResult.NonWinners)
                {
                    ScoreTokenAnimationHandler.Instance.AddTokenToNextAnimation(CurrentDistrictResult.District.MapLabel.SeatsText.transform.position, p, -CurrentDistrictResult.Seats);
                }
                ScoreTokenAnimationHandler.Instance.StartAnimation(ScoreTokenFlyingTime, callback: OnDistrictResultTokensArrived);
            }
        }
        /// <summary>
        /// Updates the background color and the district label with the result of the district election we're currently at.
        /// </summary>
        private void SetColorOfCurrentDistrictResult()
        {
            CurrentDistrictResult.District.Region.SetColor(CurrentDistrictResult.WinnerParty.Color);
            CurrentDistrictResult.District.MapLabel.SetBackgroundColor(CurrentDistrictResult.WinnerParty.Color);
            CurrentDistrictResult.District.MapLabel.SetMargin(CurrentDistrictResult.GetMargin(Game.LocalPlayerParty));
        }

        private void OnPartyListUpdateAnimationDone()
        {
            Game.UI.Parliament.ParliamentPartyList.UnhighlightParty(CurrentDistrictResult.WinnerParty);

            if (!Game.IsBattleRoyale) // In some game modes we wait for tokens to arrive and the standings panel to update first
            {
                InitWaitTime(PostPartyListAnimationPauseTime, AnimationState.InitNextDistrict);
            }
        }

        private void OnDistrictResultTokensArrived()
        {
            Game.UI.StandingsPanel.MovePositionsAnimated(TempStandingsScore, ListAnimationTime, callback: OnStandingsPanelDistrictUpdateAnimationDone);
        }

        private void OnStandingsPanelDistrictUpdateAnimationDone()
        {
            InitWaitTime(PostPartyListAnimationPauseTime, AnimationState.InitNextDistrict);
        }

        private void InitElectionResultApplication()
        {
            // Disable graph
            Game.UI.Parliament.CurrentElectionContainer.SetActive(false);

            // Apply election score points as flying tokens
            if (Game.IsClassicMode)
            {
                foreach (Party p in ElectionResult.WinnerParties)
                {
                    int scoreBonus = 1;
                    ScoreTokenAnimationHandler.Instance.AddTokenToNextAnimation(Game.UI.Parliament.ParliamentPartyList.GetElementCenter(p), p, scoreBonus);
                    TempStandingsScore[p] += scoreBonus;
                }
                ScoreTokenAnimationHandler.Instance.StartAnimation(ScoreTokenFlyingTime, callback: OnElectionResultTokensArrived);
            }
            else if (Game.IsBattleRoyale)
            {
                foreach (Party p in ElectionResult.WinnerParties)
                {
                    int scoreBonus = ElectionResult.GetWinnerLegitimacyBonus();
                    ScoreTokenAnimationHandler.Instance.AddTokenToNextAnimation(Game.UI.Parliament.ParliamentPartyList.GetElementCenter(p), p, scoreBonus);
                    TempStandingsScore[p] += scoreBonus;
                }
                ScoreTokenAnimationHandler.Instance.StartAnimation(ScoreTokenFlyingTime, callback: OnElectionResultTokensArrived);
            }

            // Instantly switch to end animation if no flying tokens needed
            else State = AnimationState.InitEndAnimation;
        }

        private void OnElectionResultTokensArrived()
        {
            Game.UI.StandingsPanel.MovePositionsAnimated(TempStandingsScore, ListAnimationTime, callback: OnStandingsPanelElectionUpdateAnimationDone);
        }

        private void OnStandingsPanelElectionUpdateAnimationDone()
        {
            InitWaitTime(PostPartyListAnimationPauseTime, AnimationState.InitEndAnimation);
        }

        private void InitEndAnimation()
        {
            // Clear graph and highlight from last district
            if (CurrentDistrictResult != null) CurrentDistrictResult.District.Region.SetAnimatedHighlight(false);
            Game.UI.Parliament.CurrentElectionGraph.ClearGraph();
            Game.UI.Parliament.ModifierSliderContainer.ClearContainer();
            Game.UI.Parliament.CurrentElectionContainer.SetActive(false);

            // Show new districts
            Game.SetAllDistrictsVisible();

            // Change UI controls
            Game.UI.SidePanelHeader.Slide(new Vector2(0, 0), UiControlsSlideTime);
            Game.UI.SidePanelFooter.Slide(new Vector2(0, 0), UiControlsSlideTime);
            Game.UI.SidePanelFooter.SetBackgroundColor(ColorManager.Instance.UiInteractable);

            Game.UI.MapControls.gameObject.SetActive(true);
            Game.UI.MapControls.SetMapDisplayMode(MapDisplayMode.LastElection, DistrictLabelMode.Default);

            Game.UI.Parliament.ParliamentPartyList.ShowEliminatedParties = true;
            Game.UI.SelectTab(Tab.Parliament);
            Game.UI.SidePanelHeader.UpdateValues(Game);

            // Update standings panel to real values
            Game.UI.StandingsPanel.MovePositionsAnimated(Game.GetCurrentStandings(), ListAnimationTime);

            // Zoom out to show all districts
            Game.CameraHandler.MoveToFocusDistricts(Game.VisibleDistricts.Values.ToList(), DistrictPanTime, callback: OnCameraZoomOutAfterElectionDone);
        }

        private void OnCameraZoomOutAfterElectionDone()
        {
            Game.OnElectionAnimationDone();
            State = AnimationState.Inactive;
        }

        private void InitWaitTime(float time, AnimationState postState)
        {
            CurrentWaitTime = time;
            CurrentWaitDelay = 0f;
            State = AnimationState.Wait;
            PostWaitState = postState;
        }

        #region Public methods

        public void SetAnimationSpeed(ElectionAnimationSpeed speed)
        {
            AnimationSpeedModifier = AnimationSpeeds[speed];
            Game.CameraHandler.SetMoveSpeedModifier(AnimationSpeedModifier);
            Game.UI.Parliament.ModifierSliderContainer.SetAnimationSpeedModifier(AnimationSpeedModifier);
            Game.UI.Parliament.CurrentElectionGraph.SetAnimationSpeedModifier(AnimationSpeedModifier);
            Game.UI.Parliament.ParliamentPartyList.SetAnimationSpeedModifier(AnimationSpeedModifier);
            Game.UI.StandingsPanel.SetAnimationSpeedModifier(AnimationSpeedModifier);
            ScoreTokenAnimationHandler.Instance.SetAnimationSpeedModifier(AnimationSpeedModifier);
        }

        public void ConcludeDistrict()
        {
            // Disallow in certain states
            List<AnimationState> forbiddenStates = new List<AnimationState>() { AnimationState.InitApplyElectionResult, AnimationState.ApplyElectionResult, AnimationState.InitEndAnimation, AnimationState.EndAnimation };
            if (forbiddenStates.Contains(State) || (State == AnimationState.Wait && forbiddenStates.Contains(PostWaitState))) return;

            Debug.Log("Concluding District early.");

            // Color district
            SetColorOfCurrentDistrictResult();

            // Interrupt ongoing animations & callbacks
            Game.CameraHandler.StopMovement();

            // Update party lists with result from current district
            Game.UI.Parliament.ParliamentPartyList.Init(TempSeats, dynamic: true);
            Game.UI.StandingsPanel.Init(TempStandingsScore, dynamic: true);

            State = AnimationState.InitNextDistrict;
        }

        public void ConcludeElection()
        {
            State = AnimationState.InitEndAnimation;
        }

        #endregion

    }
}
