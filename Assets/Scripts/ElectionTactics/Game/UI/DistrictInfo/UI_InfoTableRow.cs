using ElectionTactics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InfoTableRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    public TooltipTarget LabelTooltipTarget;
    public TooltipTarget ValueTooltipTarget;

    public void Init(string label, string value)
    {
        SetLabel(label);
        SetValue(value);
    }

    public void SetLabel(string label) => LabelText.text = label;
    public void SetValue(string value) => ValueText.text = value;


    public Action HoverAction { get; private set; }
    public Action UnhoverAction { get; private set; }
    public void SetHoverAction(Action action) => HoverAction = action;
    public void SetUnhoverAction(Action action) => UnhoverAction = action;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HoverAction != null) HoverAction.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UnhoverAction != null) UnhoverAction.Invoke();
    }
}
