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
                    OrderNum = 0;
                    break;
                case Density.Mixed:
                    OrderNum = 1;
                    break;
                case Density.Urban:
                    OrderNum = 2;
                    break;
            }
        }
    }
}