using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ScoreToken : MonoBehaviour
    {
        [Header("Elements")]
        public Image Background;
        public TextMeshProUGUI ValueText;

        public Vector2 StartPosition;
        public Vector2 TargetPosition;

        public float StartDelay { get; set; }
        public bool HasArrived { get; set; }

        public Action ArrivalCallback;
    }
}
