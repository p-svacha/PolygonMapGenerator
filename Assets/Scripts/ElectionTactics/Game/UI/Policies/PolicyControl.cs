using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class PolicyControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public TextMeshProUGUI Label;
        public TooltipTarget LabelTooltipTarget;
        public GameObject ValueContainer;
        public Button MinusButton;
        public Button PlusButton;
        public Image Icon;

        public Policy Policy;

        // Start is called before the first frame update
        void Start()
        {
            PlusButton.onClick.AddListener(PlusButton_OnClick);
            MinusButton.onClick.AddListener(MinusButton_OnClick);
        }

        private void PlusButton_OnClick()
        {
            if (Policy.Party.Game.CanIncreaseLocalPlayerPolicy(Policy))
            {
                AudioManager.PlayStandardClickSound(pitch: 1.3f);
                Policy.Party.Game.IncreaseLocalPlayerPolicy(Policy);
            }
            else AudioManager.PlaySound(AudioManager.Instance.ButtonClick_NoEffect, volume: 0.6f);

            ShowPolicyMapOverlay();
        }

        private void MinusButton_OnClick()
        {
            if (Policy.Party.Game.CanDecreaseLocalPlayerPolicy(Policy))
            {
                AudioManager.PlayStandardClickSound(pitch: 0.85f);
                Policy.Party.Game.DecreaseLocalPlayerPolicy(Policy);
            }
            else AudioManager.PlaySound(AudioManager.Instance.ButtonClick_NoEffect, volume: 0.6f);

            ShowPolicyMapOverlay();
        }

        public void Init(Policy policy)
        {
            Policy = policy;
            Label.text = policy.Name;
            Icon.sprite = policy.Sprite;
            Icon.gameObject.SetActive(policy.Sprite != null);
            if (!string.IsNullOrEmpty(policy.Description))
            {
                LabelTooltipTarget.Init(policy.Name, policy.Description);
            }
            UpdateValue();
            policy.UIControl = this;
        }

        public void UpdateValue()
        {
            for (int i = 0; i < Policy.MaxValue; i++)
            {
                if (i < Policy.Value)
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

        public void OnPointerClick(PointerEventData eventData)
        {
            // Ignore clicks that originated on the plus/minus buttons themselves
            if (eventData.pointerPress == MinusButton.gameObject ||
                eventData.pointerPress == PlusButton.gameObject) return;

            if (eventData.button == PointerEventData.InputButton.Left) PlusButton_OnClick();
            else if (eventData.button == PointerEventData.InputButton.Right) MinusButton_OnClick();
        }
    }
}
