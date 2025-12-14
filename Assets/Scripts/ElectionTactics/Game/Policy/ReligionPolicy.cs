using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ReligionPolicy : Policy
    {
        public Religion Religion;

        public ReligionPolicy(int id, Party p, Religion religion, int maxValue) : base(id, p, maxValue)
        {
            Religion = religion;
            Name = EnumHelper.GetDescription(religion);
            Type = PolicyType.Religion;
        }

        protected override int GetSinglePointBaseImpact(District district)
        {
            if (Religion != district.Religion) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}