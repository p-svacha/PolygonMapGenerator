using UnityEngine;

namespace ElectionTactics
{
    public class CT_Patriotic : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is DistrictPolicy) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override string Description => $"Base district policy impact in this district is doubled.";
    }
}
