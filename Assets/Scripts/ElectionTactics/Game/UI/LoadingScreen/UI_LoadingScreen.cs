using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_LoadingScreen : MonoBehaviour
    {
        public static UI_LoadingScreen Instance;

        public TextMeshProUGUI LoadingScreenText;
        public TextMeshProUGUI LoadingScreenStepText;

        private void Awake()
        {
            Instance = this;
        }
    }
}
