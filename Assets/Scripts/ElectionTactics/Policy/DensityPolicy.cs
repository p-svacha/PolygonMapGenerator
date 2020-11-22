using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class DensityPolicy : Policy
    {
        public Density Density;

        public DensityPolicy(Density density, int value)
        {
            Density = density;
            Name = EnumHelper.GetDescription(density);
            Value = value;
            Type = PolicyType.Density;
        }
    }
}