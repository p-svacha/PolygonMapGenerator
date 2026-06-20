using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomyPolicy : Policy
    {
        public EconomicSectorDef Trait;

        public EconomyPolicy(int id, int localId, Party p, EconomicSectorDef trait, int maxValue) : base(id, localId, p, maxValue)
        {
            Trait = trait;
            Name = trait.Label;
            Sprite = trait.Sprite;
            Description = trait.Description;
            Type = PolicyType.Economy;
        }

        public override int GetSinglePointBaseImpact(District district)
        {
            if (Trait == district.Economy1) return HIGH_POPULARITY_IMPACT;
            if (Trait == district.Economy2) return MEDIUM_POPULARITY_IMPACT;
            if (Trait == district.Economy3) return LOW_POPULARITY_IMPACT;

            // No impact if district doesn't have economy sector
            return 0;
        }
    }
}
