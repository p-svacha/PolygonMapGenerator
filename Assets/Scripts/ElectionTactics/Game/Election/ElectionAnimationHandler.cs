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

            InitUpdatePartyList,
            UpdatePartyList,

            InitEndAnimation,
            EndAnimation
        }
        private AnimationState State;
        private AnimationState LastState;

        private GeneralElectionResult ElectionResult;
        private Dictionary<Party, int> TempSeats = new Dictionary<Party, int>();

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
            if(State != LastState)
            {
                Debug.Log("--> " + State.ToString());
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

                case AnimationState.InitUpdatePartyList:
                    InitUpdatePartyList();
                    State = AnimationState.UpdatePartyList;
                    break;

                case AnimationState.UpdatePartyList:
                    // Wait for callback from party list update animation
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

            // Change map display to none (colors of winning parties will be set during animation)
            Game.UI.MapControls.SetMapDisplayMode(MapDisplayMode.NoOverlay);

            // Prepare election animation
            Game.UI.SelectTab(Tab.Parliament);
            Game.UI.Parliament.StandingsContainer.SetActive(false);

            // Clear visual seats
            TempSeats.Clear();
            foreach (Party p in Game.Parties) TempSeats.Add(p, 0);
            Game.UI.Parliament.ParliamentPartyList.Init(TempSeats, dynamic: true);

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
                TempSeats[CurrentDistrictResult.Winner] += CurrentDistrictResult.Seats;

                // Prepare graph for next district
                if (CurrentDistrictResult.District.ElectionResults.Count > 1)
                {
                    Game.UI.Parliament.CurrentElectionMarginText.gameObject.SetActive(true);
                    Game.UI.Parliament.LastElectionWinnerKnob.gameObject.SetActive(true);
                    float margin = CurrentDistrictResult.District.GetLatestElectionResult(offset: 1).GetMargin(Game.LocalPlayerParty);
                    Game.UI.Parliament.CurrentElectionMarginText.text = (margin > 0 ? "+" : "") + margin.ToString("0.0") + " %";
                    Game.UI.Parliament.LastElectionWinnerKnob.color = CurrentDistrictResult.Winner.Color;
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
                State = AnimationState.InitEndAnimation;
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
            InitWaitTime(PostGraphPauseTime, AnimationState.InitUpdatePartyList);
        }

        // Party list update
        private void InitUpdatePartyList()
        {
            // Set color of district to winner party
            CurrentDistrictResult.District.Region.SetColor(CurrentDistrictResult.Winner.Color);

            Game.UI.Parliament.ParliamentPartyList.HighlightParty(CurrentDistrictResult.Winner);
            Game.UI.Parliament.ParliamentPartyList.MovePositionsAnimated(TempSeats, ListAnimationTime, OnPartyListUpdateAnimationDone);
        }
        private void OnPartyListUpdateAnimationDone()
        {
            Game.UI.Parliament.ParliamentPartyList.UnhighlightParty(CurrentDistrictResult.Winner);
            InitWaitTime(PostPartyListAnimationPauseTime, AnimationState.InitNextDistrict);
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
            Game.UI.MapControls.SetMapDisplayMode(MapDisplayMode.Political);
            Game.UI.SelectTab(Tab.Parliament);
            Game.UI.SidePanelHeader.UpdateValues(Game);

            Game.CameraHandler.MoveToFocusDistricts(Game.VisibleDistricts.Values.ToList(), DistrictPanTime, OnCameraZoomOutAfterElectionDone);
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
        }

        public void ConcludeDistrict()
        {
            // Update region color of current district
            CurrentDistrictResult.District.Region.SetColor(CurrentDistrictResult.Winner.Color);

            // Update party list with result from current district
            Game.UI.Parliament.ParliamentPartyList.Init(TempSeats, dynamic: true);

            State = AnimationState.InitNextDistrict;
        }

        public void ConcludeElection()
        {
            State = AnimationState.InitEndAnimation;
        }

        #endregion

    }
}
