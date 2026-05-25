using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Religious : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is ReligionPolicy rp) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override string Description => $"Base religion policy impact in this district is doubled.";
    }
}
