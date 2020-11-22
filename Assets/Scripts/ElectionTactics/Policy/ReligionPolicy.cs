using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ReligionPolicy : Policy
    {
        public Religion Religion;

        public ReligionPolicy(Religion religion, int value)
        {
            Religion = religion;
            Name = EnumHelper.GetDescription(religion);
            Value = value;
            Type = PolicyType.Religion;
        }
    }
}