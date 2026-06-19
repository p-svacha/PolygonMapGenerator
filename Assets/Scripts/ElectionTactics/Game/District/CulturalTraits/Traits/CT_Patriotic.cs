using UnityEngine;

namespace ElectionTactics
{
    public class CT_Patriotic : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is DistrictPolicy) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override Policy GetOnClickPolicy()
        {
            return Game.LocalPlayerParty.GetPolicy(District);
        }

        public override string Description => $"Base district policy impact in this district is doubled.";
    }
}
