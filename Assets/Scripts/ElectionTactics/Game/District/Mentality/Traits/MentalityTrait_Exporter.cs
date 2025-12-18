using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Exporter : MentalityTrait
    {
        private const int INCREASE = Policy.HIGH_POPULARITY_IMPACT;
        private const int NEIGHBOUR_IMPACT = Policy.LOW_POPULARITY_IMPACT;

        public override void ModifyPolicyPointImpact(Policy policy, ref int impact)
        {
            if (policy is EconomyPolicy econPolicy && econPolicy.Trait == District.Economy1) impact += INCREASE;
        }

        public override void ModifyNeighbourPolicyPointImpact(Policy policy, District neighbourWithTrait, ref int impact)
        {
            if (policy is EconomyPolicy econPolicy && econPolicy.Trait == neighbourWithTrait.Economy1) impact += NEIGHBOUR_IMPACT;
        }

        public override string Label => $"{District.Economy1} exporter";
        public override string Description => $"{District.Economy1} Policy impact is increased by {INCREASE} and also increases popularity in neighbouring districts by {NEIGHBOUR_IMPACT}.";
    }
}
