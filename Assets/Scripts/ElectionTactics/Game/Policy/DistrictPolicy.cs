using UnityEngine;

namespace ElectionTactics
{
    public class DistrictPolicy : Policy
    {
        public District District { get; private set; } // District that this policy will increase popularity in

        public DistrictPolicy(int id, Party p, District district, int maxValue) : base(id, p, maxValue)
        {
            District = district;
            Name = district.Name;
            Type = PolicyType.District;
        }

        protected override int GetSinglePointBaseImpact(District district)
        {
            if (District == district) return VERY_HIGH_POPULARITY_IMPACT;

            return 0;
        }
    }
}
