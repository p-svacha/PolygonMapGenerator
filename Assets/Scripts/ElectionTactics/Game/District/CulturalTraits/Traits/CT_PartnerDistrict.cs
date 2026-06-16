using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class CT_PartnerDistrict : CulturalTrait
    {
        private const float FACTOR = 0.2f;

        public District Partner { get; private set; }

        public void SetPartner(District partner)
        {
            Partner = partner;
        }

        protected override void OnInit()
        {
            // Select partner
            Partner = Game.AllDistricts.Where(d => d != District).ToList().RandomElement();

            // Apply same trait to partner with this district as partner
            CT_PartnerDistrict partnerTrait = Partner.AddCulturalTrait(CulturalTraitDefOf.PartnerDistrict, skipOnInit: true) as CT_PartnerDistrict;
            partnerTrait.SetPartner(District);
        }

        public override void OnRemoved()
        {
            CulturalTrait connectedTrait = Partner.CulturalTraits.FirstOrDefault(t => t is CT_PartnerDistrict partner && partner.Partner == District);
            if (connectedTrait != null) Partner.RemoveCulturalTrait(connectedTrait);
        }

        public override Dictionary<string, int> GetPopularityChangeFromOtherDistrictPopularities(Party p)
        {
            int popularity = Partner.GetPartyPopularity(p, includeOtherDistrictPopularityInfluence: false);
            int bonusPopularity = (int)(FACTOR * popularity);
            return new Dictionary<string, int>()
            {
                { $"{Partner.Name} Partnership", bonusPopularity }
            };
        }

        public override string Label => $"Partnership with {Partner.Name}";
        public override string Description => $"This district has a partnership with {Partner.Name}.\n\n20% of your popularity in {Partner.Name} is applied as a popularity bonus here, and vice versa.";
        public override bool IsActive => District.IsActive && Partner.IsActive;
    }
}
