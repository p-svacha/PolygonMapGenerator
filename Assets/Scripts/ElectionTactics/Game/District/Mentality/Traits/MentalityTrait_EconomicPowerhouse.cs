using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_EconomicPowerhouse : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is EconomyPolicy ep) impact += policy.GetSinglePointBaseImpact(targetDistrict); 
        }

        public override string Description => $"Base economy policy impact in this district is doubled.";
    }
}
