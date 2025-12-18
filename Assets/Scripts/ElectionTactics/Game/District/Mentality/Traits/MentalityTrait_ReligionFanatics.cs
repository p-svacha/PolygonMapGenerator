using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_ReligionFanatics : MentalityTrait
    {
        private const int NegativeImpact = Policy.LOW_POPULARITY_IMPACT;

        public override void ModifyPolicyPointImpact(Policy policy, ref int impact)
        {
            if (policy is ReligionPolicy relPolicy && relPolicy.Religion == District.Religion) impact *= 3;
            else impact -= NegativeImpact;
        }

        public override string Label => District.Religion + " Fanatics";
        public override string Description => $"Effectiveness of {District.Religion} policy is tripled. All other religion policies reduce popularity by {NegativeImpact}.";
    }
}
