﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Policy
    {
        public Party Party;
        public int Id { get; protected set; } // Id to identify a policy, unique per player
        public string Name { get; protected set; }
        public bool IsActive { get; protected set; } // Determines if the policy is active (and therefore visible) in the current game
        public int MaxValue { get; protected set; }
        public int LockedValue { get; protected set; } // The value that is already locked in from a previous cycle - value can't go below this
        public int Value { get; protected set; }
        public int OrderNum; // how this policy should be ordered in the policy selection
        public PolicyType Type;

        public PolicyControl UIControl;

        public Policy(int id, Party p, int maxValue)
        {
            Id = id;
            Party = p;
            IsActive = false;
            MaxValue = maxValue;
            Value = 0;
            LockedValue = 0;
        }

        public void SetValue(int value)
        {
            Value = value;
            if(UIControl != null) UIControl.UpdateValue();
        }
        public void LockValue()
        {
            LockedValue = Value;
        }
        public void IncreaseValue()
        {
            SetValue(Value + 1);
        }
        public void DecreaseValue()
        {
            SetValue(Value - 1);
        }
        public void Activate()
        {
            IsActive = true;
        }
    }
}
