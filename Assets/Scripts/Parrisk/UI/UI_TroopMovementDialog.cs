using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ParriskGame
{
    public class UI_TroopMovementDialog : MonoBehaviour
    {
        [Header("Prefabs")]
        public Button TroopAmountButtonPrefab;

        [Header("UI Elements")]
        public GameObject QuickSelectionContainer;
        public InputField ManualAmountInputField;
        public Button ManualAmountMinusButton;
        public Button ManualAmountPlusButton;
        public Button ManualAmountOkButton;

        public Button CloseButton;

        public List<Button> QuickSelectionButtons = new List<Button>();

        public Action<int> Callback; // Gets called when map generation is done

        private int MaxTroops;

        public void Init(int maxTroops, int initTroops, Action<int> callback)
        {
            MaxTroops = maxTroops;
            Callback = callback;

            CloseButton.onClick.AddListener(() => Callback(0));

            // Manual selection
            ManualAmountInputField.text = initTroops.ToString();
            ManualAmountMinusButton.onClick.AddListener(DecreaseManualAmount);
            ManualAmountPlusButton.onClick.AddListener(IncreaseManualAmount);
            ManualAmountOkButton.onClick.AddListener(ConfirmManualAmount);

            // Quick selection
            if (maxTroops <= 5)
            {
                for(int i = 1; i <= maxTroops; i++)
                {
                    AddQuickSelectionButton(i);
                }
            }
            else
            {
                AddQuickSelectionButton(1);
                AddQuickSelectionButton(Mathf.RoundToInt(maxTroops * 0.25f));
                AddQuickSelectionButton(Mathf.RoundToInt(maxTroops * 0.5f));
                AddQuickSelectionButton(Mathf.RoundToInt(maxTroops * 0.75f));
                AddQuickSelectionButton(maxTroops);
            }
        }

        private void IncreaseManualAmount()
        {
            int value = int.Parse(ManualAmountInputField.text);
            if (value < MaxTroops) ManualAmountInputField.text = (value + 1).ToString();
        }

        private void DecreaseManualAmount()
        {
            int value = int.Parse(ManualAmountInputField.text);
            if (value > 1) ManualAmountInputField.text = (value - 1).ToString();
        }

        private void ConfirmManualAmount()
        {
            Callback(int.Parse(ManualAmountInputField.text));
        }

        private void AddQuickSelectionButton(int value)
        {
            Button quickSelectionButton = Instantiate(TroopAmountButtonPrefab);
            quickSelectionButton.GetComponentInChildren<Text>().text = value.ToString();
            quickSelectionButton.transform.SetParent(QuickSelectionContainer.transform);
            quickSelectionButton.onClick.AddListener(() => Callback(value));
        }
    }
}
