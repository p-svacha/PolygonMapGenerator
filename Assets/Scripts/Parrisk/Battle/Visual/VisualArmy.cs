using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    /// <summary>
    /// An army represents a group of troops that move from one territory to another at a specific time. It is the visual representation of the army data structure.
    /// </summary>
    public class VisualArmy : MonoBehaviour
    {
        public TextMesh Text;

        public Army Army;

        public Vector3 SourcePosition;
        public Vector3 TargetPosition;
        private float TimeElapsed;
        public float TimeTarget;

        private bool ReachedTarget;

        public void Init(Army army, Vector2 sourcePosition, Vector2 targetPosition, float time)
        {
            Army = army;
            Army.VisualArmy = this;
            SourcePosition = new Vector3(sourcePosition.x, 0f, sourcePosition.y);
            TargetPosition = new Vector3(targetPosition.x, 0f, targetPosition.y);
            TimeTarget = time;
            TimeElapsed = 0f;
            GetComponent<MeshRenderer>().material.color = army.SourcePlayer.Color;
            Text.text = army.NumTroops.ToString();
        }

        void Update()
        {
            TimeElapsed += Time.deltaTime;
            if(ReachedTarget)
            {
                // Fight
            }
            else
            {
                if(TimeElapsed >= TimeTarget)
                {
                    transform.position = TargetPosition;
                    ReachedTarget = true;
                }
                else transform.position = Vector3.Lerp(SourcePosition, TargetPosition, TimeElapsed / TimeTarget);
            }
        }
    }
}
