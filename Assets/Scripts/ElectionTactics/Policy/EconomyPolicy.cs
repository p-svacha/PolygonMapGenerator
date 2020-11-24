using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomyPolicy : Policy
    {
        public EconomyTrait Trait;

        public EconomyPolicy(Party p, EconomyTrait trait, int value) : base(p, value)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Type = PolicyType.Economy;
        }
    }
}
