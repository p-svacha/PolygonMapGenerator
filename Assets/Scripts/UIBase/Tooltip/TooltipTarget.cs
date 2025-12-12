using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Tooltip.TooltipType Type;
    public string Title;
    public string Text;

    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsFocussed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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
        Tooltip.Instance.Init(Type, Title, Text);
    }

    public void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        Tooltip.Instance.gameObject.SetActive(false);
    }
}

