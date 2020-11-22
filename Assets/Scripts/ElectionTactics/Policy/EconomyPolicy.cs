using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomyPolicy : Policy
    {
        public EconomyTrait Trait;

        public EconomyPolicy(EconomyTrait trait, int value)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Value = value;
            Type = PolicyType.Economy;
        }
    }
}
