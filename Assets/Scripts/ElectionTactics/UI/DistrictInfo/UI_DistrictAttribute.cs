using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictAttribute : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UI_ElectionTactics UI;
        public Text DisplayText;
        public TooltipTarget TooltipTarget;
        public Action HoverAction;

        public void Init(UI_ElectionTactics ui, string displayName, string tooltipTitle, string tooltipText)
        {
            UI = ui;
            DisplayText = GetComponent<Text>();
            DisplayText.text = displayName;
            TooltipTarget = GetComponent<TooltipTarget>();
            TooltipTarget.Title = tooltipTitle;
            TooltipTarget.Text = tooltipText;
        }

        public void Init(UI_ElectionTactics ui, GeographyTrait gt)
        {
            HoverAction = () => { UI.MapControls.ShowGeographyOverlay(gt.Type); };
            Init(ui, gt.FullName, gt.BaseName, "");
        }
        public void Init(UI_ElectionTactics ui, EconomyTrait et)
        {
            HoverAction = () => { UI.MapControls.ShowEconomyOverlay(et); };
            Init(ui, EnumHelper.GetDescription(et), EnumHelper.GetDescription(et), "");
        }
        public void Init(UI_ElectionTactics ui, Density d)
        {
            HoverAction = () => { UI.MapControls.ShowDensityOverlay(d); };
            Init(ui, EnumHelper.GetDescription(d), EnumHelper.GetDescription(d), "");
        }
        public void Init(UI_ElectionTactics ui, AgeGroup age)
        {
            HoverAction = () => { UI.MapControls.ShowAgeOverlay(age); };
            Init(ui, EnumHelper.GetDescription(age), EnumHelper.GetDescription(age), "");
        }
        public void Init(UI_ElectionTactics ui, Religion r)
        {
            HoverAction = () => { UI.MapControls.ShowReligionOverlay(r); };
            Init(ui, EnumHelper.GetDescription(r), EnumHelper.GetDescription(r), "");
        }
        public void Init(UI_ElectionTactics ui, Language l)
        {
            HoverAction = () => { UI.MapControls.ShowLanguageOverlay(l); };
            Init(ui, EnumHelper.GetDescription(l), EnumHelper.GetDescription(l), "");
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if(HoverAction != null) HoverAction.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UI.MapControls.ClearOverlay();
        }
    }
}
