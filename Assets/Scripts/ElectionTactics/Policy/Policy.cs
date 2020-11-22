using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Policy
    {
        public string Name { get; protected set; }
        public int Value { get; protected set; }
        public PolicyType Type;

        public void SetValue(int value)
        {
            //Debug.Log("Setting policy value " + value + " for " + Name);
            Value = value;
        }
    }
}
