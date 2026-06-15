using UnityEngine;

namespace ElectionTactics
{
    public class CT_Imperialistic : CulturalTrait
    {
        public PolicyType PolicyType;

        protected override void OnInit()
        {
            PolicyType = Random.value < 0.5f ? PolicyType.Religion : PolicyType.Language;
            if (!District.HasReligion) PolicyType = PolicyType.Language;
        }

        public override string Label => (PolicyType == PolicyType.Religion ? "Religious" : "Linguistic") + " Imperialism";
        public override string Description => "Newly added districts adjacent to this will have guaranteed the same " + PolicyType.ToString() + ".";
    }
}
