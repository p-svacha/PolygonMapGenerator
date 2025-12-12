using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class CameraHandler : BaseMapCameraHandler
    {
        ElectionTacticsGame Game;

        // Move
        public bool IsMoving { get; private set; }
        private Vector3 MoveStartPosition;
        private Vector3 MoveTargetPosition;
        private float MoveTime;
        private float MoveDelay;
        private float MoveSpeedModifier = 1f;
        private Action OnMoveDoneCallback;

        public void Init(ElectionTacticsGame game)
        {
            base.Init();
            Game = game;
            transform.rotation = Quaternion.Euler(90, 0, 0);
            CenterOffset = new Vector2(0.4f, 0f);
        }

        public override void Update()
        {
            if (Game == null) return;

            // Free Movement
            if (Game.State == GameState.PreparationPhase && !EventSystem.current.IsPointerOverGameObject()) base.Update();

            // Camera Lerp
            if (IsMoving)
            {
                if (MoveDelay >= MoveTime)
                {
                    transform.position = MoveTargetPosition;
                    IsMoving = false;
                    if(OnMoveDoneCallback != null) OnMoveDoneCallback();
                }
                else
                {
                    transform.position = Vector3.Lerp(MoveStartPosition, MoveTargetPosition, MoveDelay / MoveTime);
                    MoveDelay += Time.deltaTime * MoveSpeedModifier;
                }
            }
        }

        public void SetMoveSpeedModifier(float speed)
        {
            MoveSpeedModifier = speed;
        }

        public void FocusDistricts(List<District> districts)
        {
            SetBoundariesToRegions(districts.Select(x => x.Region).ToList(), focusDistricts: true);
        }

        public void MoveToFocusDistricts(List<District> districts, float time, Action callback = null)
        {
            SetBoundariesToRegions(districts.Select(x => x.Region).ToList(), focusDistricts: false);
            Vector3 targetPos = GetCenterPosition(districts.Select(x => x.Region).ToList());
            InitMovement(targetPos, time, callback);
        }
        
        private void InitMovement(Vector3 targetPosition, float time, Action callback)
        {
            IsMoving = true;
            MoveStartPosition = transform.position;
            MoveTargetPosition = targetPosition;
            MoveTime = time;
            MoveDelay = 0f;
            OnMoveDoneCallback = callback;
        }

    }
}
