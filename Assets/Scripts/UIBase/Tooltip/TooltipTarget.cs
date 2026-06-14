using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Tooltip.TooltipType Type;
    public string Title;
    [TextArea(3, 10)] public string Text;
    public Color? TitleColor;
    public bool InstantTooltip = false;


    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void Init(string title, string text, Color? titleColor = null)
    {
        Title = title;
        TitleColor = titleColor;
        Text = text;

        if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Text)) Type = Tooltip.TooltipType.TitleAndText;
        else Type = Tooltip.TooltipType.TextOnly;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Title == "" && Text == "") return;
        IsFocussed = true;
        if (InstantTooltip) CurrentDelay = Delay;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Title == "" && Text == "") return;
        HideTooltip();
    }

    private void Update()
    {
        if(IsFocussed)
        {
            if(CurrentDelay < Delay) CurrentDelay += Time.deltaTime;
            else ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        if (Tooltip.Instance.gameObject.activeSelf) return;

        Tooltip.Instance.gameObject.SetActive(true);
        Tooltip.Instance.Init(Type, Title, Text, TitleColor);
    }

    public void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        Tooltip.Instance.gameObject.SetActive(false);
    }
}

