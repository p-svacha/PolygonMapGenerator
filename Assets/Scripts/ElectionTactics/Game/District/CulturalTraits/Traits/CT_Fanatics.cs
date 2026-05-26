using UnityEngine;

namespace ElectionTactics
{
    public class CT_Fanatics : CulturalTrait
    {
        private const int PositiveImpact = Policy.VERY_HIGH_POPULARITY_IMPACT;
        private const int NegativeImpact = Policy.LOW_POPULARITY_IMPACT;

        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is ReligionPolicy religionPolicy) // Only relevant for religion policies
            {
                if (religionPolicy.Religion == District.Religion) impact += PositiveImpact;
                else impact -= NegativeImpact;
            }
        }

        public override string Label => District.Religion.LabelCapWord + " Fanatics";
        public override string Description => $"{District.Religion.Label} policy impact is increased by {PositiveImpact}. All other religion policies reduce popularity by {NegativeImpact}.";
    }
}
