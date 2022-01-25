using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomyPolicy : Policy
    {
        public EconomyTrait Trait;

        public EconomyPolicy(int id, Party p, EconomyTrait trait, int maxValue) : base(id, p, maxValue)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Type = PolicyType.Economy;
        }
    }
}
