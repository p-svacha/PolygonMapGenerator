using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyPolicy : Policy
    {
        public GeographyTrait Trait;

        public GeographyPolicy(Party p, GeographyTrait trait, int maxValue) : base(p, maxValue)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Type = PolicyType.Geography;
        }
    }
}
