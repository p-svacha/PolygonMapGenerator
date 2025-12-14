using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class DensityPolicy : Policy
    {
        public Density Density;

        public DensityPolicy(int id, Party p, Density density, int maxValue) : base(id, p, maxValue)
        {
            Density = density;
            Name = EnumHelper.GetDescription(density);
            Type = PolicyType.Density;
            switch(density)
            {
                case Density.Rural:
                    SortingOrder = 0;
                    break;
                case Density.Suburban:
                    SortingOrder = 1;
                    break;
                case Density.Urban:
                    SortingOrder = 2;
                    break;
            }
        }

        protected override int GetSinglePointBaseImpact(District district)
        {
            if (Density != district.Density) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}