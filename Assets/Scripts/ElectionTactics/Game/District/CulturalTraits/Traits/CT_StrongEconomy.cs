using UnityEngine;

namespace ElectionTactics
{
    public class CT_StrongEconomy : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is EconomyPolicy ep) impact += policy.GetSinglePointBaseImpact(targetDistrict); 
        }

        public override Policy GetOnClickPolicy()
        {
            return Game.LocalPlayerParty.GetPolicy(District.Economy1);
        }

        public override string Description => $"Base economy policy impact in this district is doubled.";
    }
}
