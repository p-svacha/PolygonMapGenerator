using ElectionTactics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Trait : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Elements")]
    public TextMeshProUGUI Text;
    public Image BackgroundImage;
    public TooltipTarget TooltipTarget;

    public void InitGeographyTrait(GeographyTrait trait)
    {
        Text.text = trait.Label;
        TooltipTarget.Init(trait.LabelWithCategory, trait.Description);

        HoverAction = () => UI_ElectionTactics.Instance.MapControls.ShowGeographyOverlay(trait.Def);
        UnhoverAction = () => UI_ElectionTactics.Instance.MapControls.ClearOverlay();
    }

    public void InitCulturalTrait(CulturalTrait trait)
    {
        Color traitColor = trait.Def.Category.Color;

        Text.text = trait.LabelCapWord;
        Color titleColor = new Color(traitColor.r + 0.4f, traitColor.g + 0.4f, traitColor.b + 0.4f);

        TooltipTarget.Init(trait.LabelCapWord, trait.Description, titleColor);

        BackgroundImage.color = traitColor;
    }

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
