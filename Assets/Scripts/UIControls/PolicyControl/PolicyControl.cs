using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class PolicyControl : MonoBehaviour
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
    }
}
