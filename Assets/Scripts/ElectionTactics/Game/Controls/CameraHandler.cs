using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            if (Game.State == GameState.PreparationPhase) base.Update();

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
            Vector3 targetPos = GetTargetPosition(districts);
            InitMovement(targetPos, time, callback);
        }

        private Vector3 GetTargetPosition(List<District> districts)
        {
            float minX = districts.Min(x => x.Region.MinWorldX);
            float minY = districts.Min(x => x.Region.MinWorldY);
            float maxX = districts.Max(x => x.Region.MaxWorldX);
            float maxY = districts.Max(x => x.Region.MaxWorldY);
            float width = maxX - minX;
            float height = maxY - minY;
            float altitude = height > width ? height : width;
            altitude *= 1.2f;
            return new Vector3(minX + (width / 2) + CenterOffset.x * altitude, altitude, minY + (height / 2) + CenterOffset.y * altitude);
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
