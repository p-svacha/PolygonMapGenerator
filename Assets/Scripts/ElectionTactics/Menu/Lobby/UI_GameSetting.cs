using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace ElectionTactics
{
    public class UI_GameSetting : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public int SettingId { get; private set; }

        [Header("Elements")]
        public TMP_Dropdown Dropdown;

        [Header("Description")]
        [TextArea(3, 10)] public string Description;

        public void Init(int settingId, List<Def> defs)
        {
            SettingId = settingId;
            Dropdown.ClearOptions();
            Dropdown.AddOptions(defs.Select(x => x.LabelCap).ToList());
            Dropdown.onValueChanged.AddListener(_ => AudioManager.PlayStandardClickSound());
            Dropdown.onValueChanged.AddListener((x) => UI_Lobby.Instance.OnRuleChanged(SettingId, x));

            UI_Lobby.Instance.GameSettingDropdowns.Add(SettingId, this);
        }

        public int GetValue() => Dropdown.value;
        public void SetValue(int value) => Dropdown.value = value;
        public void SetValueWithoutNotify(int value) => Dropdown.SetValueWithoutNotify(value);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(Description)) return;
            if (IsDropdownOpen()) return;

            UI_Lobby.Instance.HoveredItemDescriptionText.text = Description;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(Description)) return;
            if (IsDropdownOpen()) return;

            UI_Lobby.Instance.HoveredItemDescriptionText.text = "";
        }

        private bool IsDropdownOpen() => Dropdown.transform.Find("Dropdown List") != null;
    }
}
