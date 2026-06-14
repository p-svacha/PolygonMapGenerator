using ElectionTactics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InfoTableRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public Image ValueIcon;
    public TextMeshProUGUI ValueText;

    public TooltipTarget LabelTooltipTarget;
    public TooltipTarget ValueTooltipTarget;

    public void Init(string label, string value, Sprite sprite = null)
    {
        SetLabel(label);
        SetValue(value);

        if (ValueIcon != null)
        {
            ValueIcon.gameObject.SetActive(sprite != null);
            ValueIcon.sprite = sprite;
        }
    }

    public void InitDefWithSprite(Def def)
    {
        SetValue(def.Label);
        SetValueIcon(def.Sprite);
        ValueIcon.gameObject.SetActive(def.Sprite != null);
    }

    public void SetLabel(string label) => LabelText.text = label;
    public void SetValue(string value) => ValueText.text = value;
    public void SetValueIcon(Sprite sprite) => ValueIcon.sprite = sprite;




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

    public Action ClickAction { get; private set; }
    public void SetClickAction(Action action) => ClickAction = action;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ClickAction != null) ClickAction.Invoke();
    }
}
