using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class CameraHandler : MonoBehaviour
    {
        private Vector2 Offset = new Vector2(0.4f, 0f);

        // Move
        public bool IsMoving { get; private set; }
        private Vector3 MoveStartPosition;
        private Vector3 MoveTargetPosition;
        private float MoveTime;
        private float MoveDelay;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        private void Update()
        {
            // Camera Lerp
            if (IsMoving)
            {
                if (MoveDelay >= MoveTime)
                {
                    transform.position = MoveTargetPosition;
                    IsMoving = false;
                }
                else
                {
                    transform.position = Vector3.Lerp(MoveStartPosition, MoveTargetPosition, MoveDelay / MoveTime);
                    MoveDelay += Time.deltaTime;
                }
            }
        }

        public void FocusDistricts(List<District> districts)
        {
            Debug.Log("Focussing " + districts.Count + " districts.");
            Vector3 targetPos = GetTargetPosition(districts);
            transform.position = targetPos;
        }

        public void MoveToFocusDistricts(List<District> districts, float time)
        {
            Vector3 targetPos = GetTargetPosition(districts);
            InitMovement(targetPos, time);
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
            return new Vector3(minX + (width / 2) + Offset.x * altitude, altitude, minY + (height / 2) + Offset.y * altitude);
        }

        private void InitMovement(Vector3 targetPosition, float time)
        {
            if (IsMoving) return;
            IsMoving = true;
            MoveStartPosition = transform.position;
            MoveTargetPosition = targetPosition;
            MoveTime = time;
            MoveDelay = 0f;
        }

    }
}
