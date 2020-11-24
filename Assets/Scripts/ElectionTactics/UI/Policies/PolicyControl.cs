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

        public int Value;

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
            SetValue(p.Value);
            p.UIControl = this;
        }

        public void SetValue(int value)
        {
            value = Mathf.Clamp(value, 0, MaxValue);
            Value = value;
            for (int i = 0; i < MaxValue; i++)
                ValueContainer.transform.GetChild(i).gameObject.SetActive(i < value);
        }

        // Update is called once per frame
        void Update()
        {

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
            Policy.Party.Game.UI.MapControls.UpdateMapDisplay();
        }
    }
}
