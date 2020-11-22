using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AgeGroupPolicy : Policy
    {
        public AgeGroup AgeGroup;

        public AgeGroupPolicy(AgeGroup ageGroup, int value)
        {
            AgeGroup = ageGroup;
            Name = EnumHelper.GetDescription(ageGroup);
            Value = value;
            Type = PolicyType.AgeGroup;
        }
    }
}