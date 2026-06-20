using UnityEngine;

namespace ElectionTactics
{
    public class CT_Religious : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is ReligionPolicy rp) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }

        public override Policy GetOnClickPolicy()
        {
            return Game.LocalPlayerParty.GetPolicy(District.Religion);
        }

        public override string Description => $"Base religion policy impact in this district is doubled.";

        public override string Describer => $"with a high focus on their {District.Religion.Label.ToLower()} religion";
    }
}
