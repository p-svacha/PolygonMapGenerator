using UnityEngine;

namespace ElectionTactics
{
    public class CT_Linguistic : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is LanguagePolicy lp) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override Policy GetOnClickPolicy()
        {
            return Game.LocalPlayerParty.GetPolicy(District.Language);
        }

        public override string Description => $"Base language policy impact in this district is doubled.";
    }
}
