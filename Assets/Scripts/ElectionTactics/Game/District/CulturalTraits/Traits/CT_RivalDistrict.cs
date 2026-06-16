using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class CT_RivalDistrict : CulturalTrait
    {
        private const float FACTOR = 0.2f;

        public District Rival { get; private set; }

        public void SetRival(District rival)
        {
            Rival = rival;
        }

        protected override void OnInit()
        {
            // Select partner
            Rival = Game.AllDistricts.Where(d => d != District).ToList().RandomElement();

            // Apply same trait to partner with this district as partner
            CT_RivalDistrict partnerTrait = Rival.AddCulturalTrait(CulturalTraitDefOf.RivalDistrict, skipOnInit: true) as CT_RivalDistrict;
            partnerTrait.SetRival(District);
        }

        public override void OnRemoved()
        {
            CulturalTrait connectedTrait = Rival.CulturalTraits.FirstOrDefault(t => t is CT_RivalDistrict rival && rival.Rival == District);
            if (connectedTrait != null) Rival.RemoveCulturalTrait(connectedTrait);
        }

        public override Dictionary<string, int> GetPopularityChangeFromOtherDistrictPopularities(Party p)
        {
            int popularity = Rival.GetPartyPopularity(p, includeOtherDistrictPopularityInfluence: false);
            int popularityPenalty = (int)(FACTOR * popularity);
            return new Dictionary<string, int>()
            {
                { $"{Rival.Name} Rivalry", -popularityPenalty }
            };
        }

        public override string Label => $"Rivalry with {Rival.Name}";
        public override string Description => $"This district has a rivalry with {Rival.Name}.\n\n20% of your popularity in {Rival.Name} is applied as a popularity penalty here, and vice versa.";
        public override bool IsActive => District.IsActive && Rival.IsActive;
    }
}
