using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_NonLinguistic : MentalityTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, District district, ref int impact)
        {
            if (policy.Type == PolicyType.Language) impact /= 2;
        }
    }
}
