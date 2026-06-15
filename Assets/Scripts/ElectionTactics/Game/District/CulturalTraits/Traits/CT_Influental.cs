using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class CT_Influental : CulturalTrait
    {
        private const float FACTOR = 0.1f;

        public override Dictionary<string, int> GetPopularityChangeInNeighbours(Party p)
        {
            int popularity = District.GetPartyPopularity(p, includeOtherDistrictPopularityInfluence: false);
            int bonusPopularity = (int)(FACTOR * popularity);
            return new Dictionary<string, int>()
            {
                { $"{District.Name} Influence", bonusPopularity }
            };
        }
    }
}
