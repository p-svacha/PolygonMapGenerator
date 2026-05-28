using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class PolicyControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private int MaxValue = 8;

        public TextMeshProUGUI Label;
        public GameObject ValueContainer;
        public Button MinusButton;
        public Button PlusButton;

        public Policy Policy;

        // Start is called before the first frame update
        void Start()
        {
            PlusButton.onClick.AddListener(PlusButton_OnClick);
            PlusButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound(pitch: 1.3f));

            MinusButton.onClick.AddListener(MinusButton_OnClick);
            MinusButton.onClick.AddListener(() => AudioManager.PlayStandardClickSound(pitch: 0.85f));
        }

        private void PlusButton_OnClick()
        {
            Policy.Party.Game.IncreaseLocalPlayerPolicy(Policy);
            ShowPolicyMapOverlay();
        }

        private void MinusButton_OnClick()
        {
            Policy.Party.Game.DecreaseLocalPlayerPolicy(Policy);
            ShowPolicyMapOverlay();
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
                    if (i < Policy.LockedValue) ValueContainer.transform.GetChild(i).GetComponent<Image>().color = ColorManager.Instance.UiMainLighter2;
                    else ValueContainer.transform.GetChild(i).GetComponent<Image>().color = ColorManager.Instance.UiText;
                }
                else
                {
                    ValueContainer.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowPolicyMapOverlay();
        }

        private void ShowPolicyMapOverlay()
        {
            Policy.Party.Game.UI.MapControls.ShowPolicyImpact(Policy);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Policy.Party.Game.UI.MapControls.ClearOverlay();
        }
    }
}
