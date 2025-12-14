using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Secular : MentalityTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, District district, ref int impact)
        {
            if (policy.Type == PolicyType.Religion) impact /= 2;
        }
    }
}
