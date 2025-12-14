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

        protected override int GetSinglePointBaseImpact(District district)
        {
            if (Trait == district.Economy1) return HIGH_POPULARITY_IMPACT;
            if (Trait == district.Economy2) return MEDIUM_POPULARITY_IMPACT;
            if (Trait == district.Economy3) return LOW_POPULARITY_IMPACT;

            // No impact if district doesn't have economy sector
            return 0;
        }
    }
}
