using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ReligionPolicy : Policy
    {
        public Religion Religion;

        public ReligionPolicy(Party p, Religion religion, int value) : base(p, value)
        {
            Religion = religion;
            Name = EnumHelper.GetDescription(religion);
            Type = PolicyType.Religion;
        }
    }
}