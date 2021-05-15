using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        // Moving
        public List<Vector3> WalkPath = new List<Vector3>();
        private float WalkPathLength;
        private int CurrentPathPointIndex;
        private float LastPathPointDistance;
        private float NextPathPointDistance;
        private float TimeElapsed;
        public float TimeTarget;

        private bool ReachedTarget;

        public void Init(Army army, List<Vector2> walkPath, float time)
        {
            Army = army;
            Army.VisualArmy = this;
            foreach (Vector2 pathPoint in walkPath) WalkPath.Add(new Vector3(pathPoint.x, 0f, pathPoint.y));
            WalkPathLength = 0f;
            for(int i = 0; i < WalkPath.Count - 1; i++) WalkPathLength += Vector3.Distance(WalkPath[i], WalkPath[i + 1]);
            CurrentPathPointIndex = 0;
            LastPathPointDistance = 0f;
            NextPathPointDistance = Vector3.Distance(WalkPath[CurrentPathPointIndex], WalkPath[CurrentPathPointIndex + 1]);
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
                
            }
            else // Move
            {
                if (TimeElapsed >= TimeTarget)
                {
                    transform.position = WalkPath.Last();
                    ReachedTarget = true;
                }
                else
                {
                    float relativeDistance = TimeElapsed / TimeTarget;
                    float absDistance = relativeDistance * WalkPathLength;
                    if(absDistance >= NextPathPointDistance)
                    {
                        CurrentPathPointIndex++;
                        LastPathPointDistance = NextPathPointDistance;
                        NextPathPointDistance += Vector3.Distance(WalkPath[CurrentPathPointIndex], WalkPath[CurrentPathPointIndex + 1]);
                    }
                    float relPositionPathPoints = (absDistance - LastPathPointDistance) / (NextPathPointDistance - LastPathPointDistance);
                    transform.position = Vector3.Lerp(WalkPath[CurrentPathPointIndex], WalkPath[CurrentPathPointIndex + 1], relPositionPathPoints);
                }
            }
        }
    }
}
