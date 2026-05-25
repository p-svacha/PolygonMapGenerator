using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Settings : MonoBehaviour
    {
        [Header("Elements")]
        public Toggle DebugMode;
        public Button QuitToDesktopButton;

        void Start()
        {
            OnSettingChanged();
            DebugMode.onValueChanged.AddListener(x => OnSettingChanged());
            //QuitToDesktopButton.onClick.AddListener(() => Application.Quit());
            QuitToDesktopButton.onClick.AddListener(() => MenuNavigator.Instance.SwitchToMainMenuScreen());
        }

        public void OnSettingChanged()
        {
            GlobalSettings.Update(
                debugMode: DebugMode.isOn
                );
        }
    }
}
