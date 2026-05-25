using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class DensityPolicy : Policy
    {
        public DensityDef Density;

        public DensityPolicy(int id, Party p, DensityDef density, int maxValue) : base(id, p, maxValue)
        {
            Density = density;
            Name = density.Label;
            Type = PolicyType.Density;
            SortingOrder = density.SortingOrder;
        }

        public override int GetSinglePointBaseImpact(District district)
        {
            if (Density != district.Density) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}