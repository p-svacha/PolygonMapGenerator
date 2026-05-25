using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Linguistic : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is LanguagePolicy lp) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override string Description => $"Base language policy impact in this district is doubled.";
    }
}
