using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_EconomicPowerhouse : MentalityTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, District district, ref int impact)
        {
            if (policy.Type == PolicyType.Economy) impact *= 2;
        }
    }
}
