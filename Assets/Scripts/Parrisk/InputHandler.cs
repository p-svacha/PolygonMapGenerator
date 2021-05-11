using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParriskGame
{
    public class InputHandler : BaseMapCameraHandler
    {
        public ParriskGame Game;

        private InputState State;

        // Planning phase
        public UI_TroopMovementDialog ActiveTroopDialog;
        public Vector2 DragArrowStart;
        public Vector2 DragArrowEnd;
        public Territory DragSourceTerritory;
        public Territory DragTargetTerritory;
        public GameObject DragArrow;

        public ArmyArrow EditedMovement;

        // Combat phase


        public void Init(ParriskGame game)
        {
            Game = game;
            Init(game.Map);
            State = InputState.Idle;
        }

        #region Update

        public override void Update()
        {
            base.Update();

            switch(Game.State)
            {
                case GameState.PlanningPhase:
                    UpdatePlanningPhase();
                    break;

                case GameState.CombatPhase:
                    UpdateCombatPhase();
                    break;
            }
        }

        private void UpdatePlanningPhase()
        {
            switch (State)
            {
                
                case InputState.Idle:
                    // Click on existing movement
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        ArmyArrow hoveredMovement = GetHoveredTroopMovement();
                        if (hoveredMovement != null)
                        {
                            EditedMovement = hoveredMovement;
                            ActiveTroopDialog = Instantiate(Game.UI.TroopMovementDialog);
                            ActiveTroopDialog.Init(hoveredMovement.TroopMovement.SourceTerritory.UnplannedTroops + hoveredMovement.TroopMovement.NumTroops, hoveredMovement.TroopMovement.NumTroops, OnEditTroopMovementDialogReturn);
                            ActiveTroopDialog.transform.SetParent(Game.UI.OverlayContainer.transform);
                            Vector3 targetPos = Camera.WorldToScreenPoint(new Vector3(hoveredMovement.MidPoint.x, 0f, hoveredMovement.MidPoint.y));
                            ActiveTroopDialog.transform.position = targetPos;
                            State = InputState.AmountDialog;
                        }
                    }

                    // Space ends planning phase
                    else if(Input.GetKeyDown(KeyCode.Space))
                    {
                        Game.EndPlanningPhase();
                    }
                    break;

                // Dragging current arrow
                case InputState.Dragging:
                    Destroy(DragArrow);
                    bool validDrag = CanCreateNewTroopMovement();
                    DragArrowEnd = GetMousePositionOnMap();
                    DragArrow = MeshGenerator.DrawArrow(DragArrowStart, DragArrowEnd, validDrag ? Color.red : Color.black, width: 0.02f, arrowHeadWidth: 0.06f, arrowHeadLength: 0.15f, yPos: PolygonMapGenerator.LAYER_OVERLAY1);
                    break;
            }
        }

        private void UpdateCombatPhase()
        {

        }

        #endregion

        #region Mouse Triggers

        protected override void OnLeftMouseDragStart()
        {
            base.OnLeftMouseDragStart();
            if(State == InputState.Idle && Game.State == GameState.PlanningPhase)
            {
                Region hoveredRegion = GetHoveredRegion();
                if (hoveredRegion != null && !hoveredRegion.IsWater && Game.Territories[hoveredRegion].Player == Game.LocalPlayer && Game.Territories[hoveredRegion].UnplannedTroops > 0)
                {
                    State = InputState.Dragging;
                    DragSourceTerritory = Game.Territories[hoveredRegion];
                    DragArrowStart = GetMousePositionOnMap();
                    DragArrow = new GameObject();
                }
            }
        }

        protected override void OnLeftMouseDragEnd()
        {
            base.OnLeftMouseDragEnd();
            if(State == InputState.Dragging)
            {
                Region hoveredRegion = GetHoveredRegion();
                if (CanCreateNewTroopMovement())
                {
                    DragTargetTerritory = Game.Territories[hoveredRegion];
                    ActiveTroopDialog = Instantiate(Game.UI.TroopMovementDialog);
                    ActiveTroopDialog.Init(DragSourceTerritory.UnplannedTroops, DragSourceTerritory.UnplannedTroops, OnNewTroopMovementDialogReturn);
                    ActiveTroopDialog.transform.SetParent(Game.UI.OverlayContainer.transform);
                    Vector3 targetPos = Vector3.Lerp(Camera.WorldToScreenPoint(new Vector3(DragArrowStart.x, 0f, DragArrowStart.y)), Camera.WorldToScreenPoint(new Vector3(DragArrowEnd.x, 0f, DragArrowEnd.y)), 0.5f);
                    ActiveTroopDialog.transform.position = targetPos;
                    State = InputState.AmountDialog;
                }
                else
                {
                    Destroy(DragArrow);
                    State = InputState.Idle;
                }
            }
        }

        #endregion

        public void OnNewTroopMovementDialogReturn(int numTroops)
        {
            Destroy(ActiveTroopDialog.gameObject);
            ActiveTroopDialog = null;

            if (numTroops == 0) Destroy(DragArrow);
            else
            {
                Vector2 midPos = Vector2.Lerp(DragArrowStart, DragArrowEnd, 0.5f);

                GameObject rectangle = MeshGenerator.DrawRectangle(midPos, PolygonMapGenerator.LAYER_OVERLAY2, 0.13f, 0.13f, Color.gray);
                rectangle.AddComponent<MeshCollider>();
                rectangle.transform.SetParent(DragArrow.transform);
                
                TextMesh movementLabel = MeshGenerator.DrawTextMesh(midPos, PolygonMapGenerator.LAYER_OVERLAY3, numTroops.ToString(), 60);
                movementLabel.transform.SetParent(DragArrow.transform);

                Army newMovement = Game.AddArmy(DragSourceTerritory, DragTargetTerritory, numTroops);

                ArmyArrow movementArrow = DragArrow.AddComponent<ArmyArrow>();
                movementArrow.Init(newMovement, movementLabel, midPos);
                Game.ArmyArrows.Add(movementArrow);
            }

            State = InputState.Idle;
        }

        public void OnEditTroopMovementDialogReturn(int numTroopsNew)
        {
            if(numTroopsNew == 0)
            {
                Game.RemoveArmy(EditedMovement.TroopMovement);
                Destroy(EditedMovement.gameObject);
            }
            if(numTroopsNew != 0)
            {
                Game.EditArmy(EditedMovement.TroopMovement, numTroopsNew);
                EditedMovement.Label.text = numTroopsNew.ToString();
            }

            Destroy(ActiveTroopDialog.gameObject);
            EditedMovement = null;
            ActiveTroopDialog = null;
            State = InputState.Idle;
        }

        private bool CanCreateNewTroopMovement()
        {
            Region hoveredRegion = GetHoveredRegion();
            if (hoveredRegion == null) return false;
            if (hoveredRegion.IsWater) return false;
            if (!DragSourceTerritory.Region.Neighbours.Contains(hoveredRegion)) return false;
            if (Game.TroopMovements[Game.Turn].Where(x => x.SourceTerritory == DragSourceTerritory && x.TargetTerritory == Game.Territories[hoveredRegion]).Count() > 0) return false;
            return true;
        }

        private ArmyArrow GetHoveredTroopMovement()
        {
            if (!IsHoveringUi())
            {
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.GetComponent<ArmyArrow>() != null) 
                        return hit.transform.gameObject.GetComponent<ArmyArrow>();

                    if (hit.transform.gameObject.GetComponentInParent<ArmyArrow>() != null) 
                        return hit.transform.gameObject.GetComponentInParent<ArmyArrow>();
                }
            }

            return null;
        }
    }

    public enum InputState
    {
        Idle,
        Dragging,
        AmountDialog
    }

    public enum CombatPhaseState
    {

    }
}
