using System.Collections.Generic;

namespace ElectionTactics
{
    public class CT_FocalPoint : CulturalTrait
    {
        public const int THRESHOLD = 5;
        public const int POPULARITY_BONUS = 30;

        public PolicyType PolicyType;

        protected override void OnInit()
        {
            List<PolicyType> candidates = new List<PolicyType>()
            {
                PolicyType.District,
                PolicyType.Economy,
                PolicyType.Density,
                PolicyType.AgeGroup,
                PolicyType.Language,
                PolicyType.Religion
            };
            PolicyType = candidates.RandomElement();
        }

        public override Dictionary<string, int> GetPopularityChange(Party p)
        {
            if (p.Policies.Count == 0) return base.GetPopularityChange(p);

            Policy policy = GetRelevantPolicy(p);
            if (policy.Value >= THRESHOLD)
            {
                return new Dictionary<string, int>()
                {
                    { $"Focal Point", POPULARITY_BONUS }
                };
            }
            return base.GetPopularityChange(p);
        }

        public Policy GetRelevantPolicy(Party p)
        {
            return PolicyType switch
            {
                PolicyType.District => p.GetPolicy(District),
                PolicyType.Economy => p.GetPolicy(District.Economy1),
                PolicyType.Density => p.GetPolicy(District.Density),
                PolicyType.AgeGroup => p.GetPolicy(District.AgeGroup),
                PolicyType.Language => p.GetPolicy(District.Language),
                PolicyType.Religion => p.GetPolicy(District.Religion),
                _ => throw new System.NotImplementedException(),
            };
        }

        public override string Label => $"Focal Point: {PolicyType}";
        public override string Description => $"Parties that have invested at least {THRESHOLD} policy points into the {GetRelevantPolicy(Game.LocalPlayerParty).Name} policy, gain a +{POPULARITY_BONUS} popularity bonus.";
    }
}
