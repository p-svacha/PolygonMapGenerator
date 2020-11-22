using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyPolicy : Policy
    {
        public GeographyTrait Trait;

        public GeographyPolicy(GeographyTrait trait, int value)
        {
            Trait = trait;
            Name = EnumHelper.GetDescription(trait);
            Value = value;
            Type = PolicyType.Geography;
        }
    }
}
