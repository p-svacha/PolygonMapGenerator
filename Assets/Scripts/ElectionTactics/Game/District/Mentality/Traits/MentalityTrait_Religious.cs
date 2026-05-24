using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Religious : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(Policy policy, ref int impact)
        {
            if (policy.Type == PolicyType.Religion) impact *= 2;
        }
    }
}
