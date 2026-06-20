using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AgeGroupPolicy : Policy
    {
        public AgeGroupDef AgeGroup;

        public AgeGroupPolicy(int id, int localId, Party p, AgeGroupDef ageGroup, int maxValue) : base(id, localId, p, maxValue)
        {
            AgeGroup = ageGroup;
            Name = ageGroup.Label;
            Type = PolicyType.AgeGroup;
            SortingOrder = ageGroup.SortingOrder;
        }

        public override int GetSinglePointBaseImpact(District district)
        {
            if (AgeGroup != district.AgeGroup) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}