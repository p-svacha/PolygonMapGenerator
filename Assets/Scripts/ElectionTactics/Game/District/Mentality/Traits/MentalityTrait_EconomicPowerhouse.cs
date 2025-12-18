using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_EconomicPowerhouse : MentalityTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, ref int impact)
        {
            if (policy.Type == PolicyType.Economy) impact *= 2;
        }

        public override string Description => $"Economy policy effectiveness in this district is doubled.";
    }
}
