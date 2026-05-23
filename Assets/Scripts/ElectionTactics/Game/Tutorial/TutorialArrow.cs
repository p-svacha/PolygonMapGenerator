using UnityEngine;

namespace ElectionTactics
{
    public class TutorialArrow : MonoBehaviour
    {
        public float wobbleDistance = 16f; // pixels forward/backward
        public float wobbleSpeed = 1f;   // cycles per second

        public Vector3 Position;

        void Start()
        {
            Position = transform.position;
        }

        void Update()
        {
            float offset = Mathf.Sin(Time.time * wobbleSpeed * Mathf.PI * 2f) * wobbleDistance;
            Vector3 direction = (transform.rotation * Vector3.left).normalized;
            transform.position = Position + direction * offset;
        }
    }
}
