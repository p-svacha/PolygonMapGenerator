using ElectionTactics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Trait : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Elements")]
    public TextMeshProUGUI Text;
    public Image BackgroundImage;
    public TooltipTarget TooltipTarget;

    public void InitGeographyTrait(GeographyTrait trait)
    {
        Text.text = trait.Label;
        TooltipTarget.Init(Tooltip.TooltipType.TitleAndText, trait.LabelWithCategory, trait.Description);

        HoverAction = () => UI_ElectionTactics.Instance.MapControls.ShowGeographyOverlay(trait.Def);
        UnhoverAction = () => UI_ElectionTactics.Instance.MapControls.ClearOverlay();
    }

    public void InitCulturalTrait(CulturalTrait trait)
    {
        Text.text = trait.LabelCapWord;
        TooltipTarget.Init(Tooltip.TooltipType.TitleAndText, trait.LabelCapWord, trait.Description);
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
}
