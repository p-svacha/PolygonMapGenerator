﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AgeGroupPolicy : Policy
    {
        public AgeGroup AgeGroup;

        public AgeGroupPolicy(Party p, AgeGroup ageGroup, int value) : base(p, value)
        {
            AgeGroup = ageGroup;
            Name = EnumHelper.GetDescription(ageGroup);
            Type = PolicyType.AgeGroup;
            switch (ageGroup)
            {
                case AgeGroup.GenerationZ:
                    OrderNum = 0;
                    break;
                case AgeGroup.Millenials:
                    OrderNum = 1;
                    break;
                case AgeGroup.GenerationX:
                    OrderNum = 2;
                    break;
                case AgeGroup.Boomers:
                    OrderNum = 3;
                    break;
            }
        }
    }
}