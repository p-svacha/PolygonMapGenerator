using UnityEngine;

namespace ElectionTactics
{
    public class CT_NonLinguistic : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is LanguagePolicy lp)
            {
                int baseImpact = policy.GetSinglePointBaseImpact(targetDistrict);
                int reduction = baseImpact / 2;
                impact -= reduction;
            }
        }

        public override string Description => $"Base language policy impact in this district is halved.";
    }
}
