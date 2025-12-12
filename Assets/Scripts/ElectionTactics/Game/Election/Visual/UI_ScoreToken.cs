using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionTactics
{
    public class UI_ScoreToken : MonoBehaviour
    {
        [Header("Elements")]
        public Image Background;
        public TextMeshProUGUI ValueText;

        public Vector2 StartPosition;
        public Vector2 TargetPosition;
    }
}
