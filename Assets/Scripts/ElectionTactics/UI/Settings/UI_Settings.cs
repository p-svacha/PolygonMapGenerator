using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Settings : MonoBehaviour
    {
        public Toggle DebugMode;

        void Start()
        {
            OnSettingChanged();
            DebugMode.onValueChanged.AddListener(x => OnSettingChanged());
        }

        public void OnSettingChanged()
        {
            GlobalSettings.Update(
                debugMode: DebugMode.isOn
                );
        }
    }
}
