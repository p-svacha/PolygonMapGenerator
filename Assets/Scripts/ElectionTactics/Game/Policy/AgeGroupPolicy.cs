using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AgeGroupPolicy : Policy
    {
        public AgeGroup AgeGroup;

        public AgeGroupPolicy(int id, Party p, AgeGroup ageGroup, int maxValue) : base(id, p, maxValue)
        {
            AgeGroup = ageGroup;
            Name = EnumHelper.GetDescription(ageGroup);
            Type = PolicyType.AgeGroup;
            switch (ageGroup)
            {
                case AgeGroup.GenerationZ:
                    SortingOrder = 0;
                    break;
                case AgeGroup.Millenials:
                    SortingOrder = 1;
                    break;
                case AgeGroup.GenerationX:
                    SortingOrder = 2;
                    break;
                case AgeGroup.Boomers:
                    SortingOrder = 3;
                    break;
            }
        }

        protected override int GetSinglePointBaseImpact(District district)
        {
            if (AgeGroup != district.AgeGroup) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}