using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyPolicy : Policy
    {
        public GeographyTraitType Trait;

        public GeographyPolicy(int id, Party p, GeographyTraitType trait, int maxValue) : base(id, p, maxValue)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Type = PolicyType.Geography;
        }

        protected override int GetSinglePointBaseImpact(District district)
        {
            // Check if district has geography trait of this policy
            GeographyTrait trait = district.Geography.FirstOrDefault(g => g.Type == Trait);
            if (trait != null)
            {
                if (trait.Category == 3) return HIGH_POPULARITY_IMPACT;
                if (trait.Category == 2) return MEDIUM_POPULARITY_IMPACT;
                if (trait.Category == 1) return LOW_POPULARITY_IMPACT;
                throw new System.Exception("Geography traits with a category outside 1,2,3 is not allowed.");
            }

            // No impact if district doesn't have the trait
            return 0;
        }
    }
}
