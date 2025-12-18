using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Linguistic : MentalityTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, ref int impact)
        {
            if (policy.Type == PolicyType.Language) impact *= 2;
        }
    }
}
