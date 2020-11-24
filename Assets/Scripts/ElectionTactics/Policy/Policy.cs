using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Policy
    {
        public Party Party;
        public string Name { get; protected set; }
        public int Value { get; protected set; }
        public int OrderNum; // how this policy should be ordered in the policy selection
        public PolicyType Type;

        public PolicyControl UIControl;

        public Policy(Party p, int value)
        {
            Party = p;
            Value = value;
        }

        public void SetValue(int value)
        {
            Value = value;
            UIControl.SetValue(value);
        }
        public void IncreaseValue()
        {
            SetValue(Value + 1);
        }
        public void DecreaseValue()
        {
            SetValue(Value - 1);
        }
    }
}
