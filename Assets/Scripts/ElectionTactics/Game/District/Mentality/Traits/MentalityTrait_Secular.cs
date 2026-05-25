using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Secular : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is ReligionPolicy rp)
            {
                int baseImpact = policy.GetSinglePointBaseImpact(targetDistrict);
                int reduction = baseImpact / 2;
                impact -= reduction;
            }
        }

        public override string Description => $"Base religion policy impact in this district is halved.";
    }
}
