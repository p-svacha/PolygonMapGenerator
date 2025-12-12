using ElectionTactics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameSettingDropdownOptionListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Key is used to specify what type of content should be displayed in the lobby description field.
    /// </summary>
    public string Key;
    private TMP_Dropdown Dropdown;
    private string Label;

    private void Start()
    {
        Dropdown = GetComponentInParent<TMP_Dropdown>();
        Label = GetComponentInChildren<TextMeshProUGUI>().text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Key == "") return;
        UI_Lobby.Instance.OnHoveredGameSettingsOptionChanged(Key, Label);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Lobby.Instance.OnHoveredGameSettingsOptionChanged(Key, "");
    }

    private void OnDestroy()
    {
        UI_Lobby.Instance.OnHoveredGameSettingsOptionChanged(Key, "");
    }
}
