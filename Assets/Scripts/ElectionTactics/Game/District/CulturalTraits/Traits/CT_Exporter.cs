using System.Text.RegularExpressions;
using UnityEngine;

namespace ElectionTactics
{
    public class CT_Exporter : CulturalTrait
    {
        public const int INCREASE = 3;
        public const int NEIGHBOUR_IMPACT = 3;

        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is EconomyPolicy econPolicy && econPolicy.Trait == District.Economy1) impact += INCREASE;
        }

        public override void ModifyNeighbourPolicyPointImpact(District targetDistrict, Policy policy, District neighbourWithTrait, ref int impact)
        {
            if (policy is EconomyPolicy econPolicy && econPolicy.Trait == neighbourWithTrait.Economy1) impact += NEIGHBOUR_IMPACT;
        }

        public override Policy GetOnClickPolicy()
        {
            return Game.LocalPlayerParty.GetPolicy(District.Economy1);
        }

        public override string Label => $"{District.Economy1.LabelCapWord} Exporter";
        public override string Description => $"{District.Economy1} Policy impact is increased by {INCREASE} and also increases popularity in neighbouring districts by {NEIGHBOUR_IMPACT}.";
    }
}
