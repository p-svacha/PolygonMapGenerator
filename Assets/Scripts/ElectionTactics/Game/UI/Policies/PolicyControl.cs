using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class PolicyControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private int MaxValue = 8;

        public Text Label;
        public GameObject ValueContainer;
        public Button MinusButton;
        public Button PlusButton;

        public Policy Policy;

        // Start is called before the first frame update
        void Start()
        {
            PlusButton.onClick.AddListener(() => Policy.Party.Game.IncreasePolicy(Policy));
            MinusButton.onClick.AddListener(() => Policy.Party.Game.DecreasePolicy(Policy));
        }

        public void Init(Policy p)
        {
            Policy = p;
            Label.text = p.Name;
            UpdateValue();
            p.UIControl = this;
        }

        public void UpdateValue()
        {
            for (int i = 0; i < MaxValue; i++)
            {
                if(i < Policy.Value)
                {
                    ValueContainer.transform.GetChild(i).gameObject.SetActive(true);
                    if (i < Policy.LockedValue) ValueContainer.transform.GetChild(i).GetComponent<Image>().color = ColorManager.Colors.UiMainColorLighter1;
                    else ValueContainer.transform.GetChild(i).GetComponent<Image>().color = ColorManager.Colors.TextColor;
                }
                else
                {
                    ValueContainer.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            switch(Policy.Type)
            {
                case PolicyType.Geography:
                    Policy.Party.Game.UI.MapControls.ShowGeographyOverlay(((GeographyPolicy)Policy).Trait);
                    break;
                case PolicyType.Economy:
                    Policy.Party.Game.UI.MapControls.ShowEconomyOverlay(((EconomyPolicy)Policy).Trait);
                    break;
                case PolicyType.Density:
                    Policy.Party.Game.UI.MapControls.ShowDensityOverlay(((DensityPolicy)Policy).Density);
                    break;
                case PolicyType.AgeGroup:
                    Policy.Party.Game.UI.MapControls.ShowAgeOverlay(((AgeGroupPolicy)Policy).AgeGroup);
                    break;
                case PolicyType.Language:
                    Policy.Party.Game.UI.MapControls.ShowLanguageOverlay(((LanguagePolicy)Policy).Language);
                    break;
                case PolicyType.Religion:
                    Policy.Party.Game.UI.MapControls.ShowReligionOverlay(((ReligionPolicy)Policy).Religion);
                    break;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Policy.Party.Game.UI.MapControls.ClearOverlay();
        }
    }
}
