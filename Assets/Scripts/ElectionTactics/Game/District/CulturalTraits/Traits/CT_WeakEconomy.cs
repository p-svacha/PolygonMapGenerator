using UnityEngine;

namespace ElectionTactics
{
    public class CT_WeakEconomy : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy.Type == PolicyType.Economy)
            {
                int baseImpact = policy.GetSinglePointBaseImpact(targetDistrict);
                int reduction = baseImpact / 2;
                impact -= reduction;
            }
        }

        public override string Description => $"Base economy policy impact in this district is halved.";
    }
}
