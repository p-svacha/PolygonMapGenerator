using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public abstract class Policy
    {
        public Party Party { get; private set; }
        public int Id { get; protected set; } // Id to identify a policy, unique per player
        public string Name { get; protected set; }
        public bool IsActive { get; protected set; } // Determines if the policy is active (and therefore visible) in the current game
        public int MaxValue { get; protected set; }
        public int LockedValue { get; protected set; } // The value that is already locked in from a previous cycle - value can't go below this
        public int Value { get; protected set; }
        public int SortingOrder; // how this policy should be ordered in the policy selection
        public PolicyType Type;

        public PolicyControl UIControl;


        public const int LOW_POPULARITY_IMPACT = 3;
        public const int MEDIUM_POPULARITY_IMPACT = 5;
        public const int HIGH_POPULARITY_IMPACT = 7;
        public const int VERY_HIGH_POPULARITY_IMPACT = 10;

        public Policy(int id, Party p, int maxValue)
        {
            Id = id;
            Party = p;
            IsActive = false;
            MaxValue = maxValue;
            Value = 0;
            LockedValue = 0;
        }

        public void SetValue(int value)
        {
            Value = value;
            if(UIControl != null) UIControl.UpdateValue();
        }
        public void LockValue()
        {
            LockedValue = Value;
        }
        public void IncreaseValue()
        {
            SetValue(Value + 1);
        }
        public void DecreaseValue()
        {
            SetValue(Value - 1);
        }
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Returns how much a single point in this policy affects the popularity of the party in the given district at a base level, purely from the policy excluding any modifierts from district or party traits.
        /// </summary>
        protected abstract int GetSinglePointBaseImpact(District district);

        /// <summary>
        /// Returns how much a single point in this policy affects the popularity of the party in the given district, including all modifiers from district and party traits.
        /// All policy popularity calculations and displays derive from this.
        /// </summary>
        public int GetSinglePointImpactOn(District district)
        {
            int impact = GetSinglePointBaseImpact(district);
            foreach (MentalityTrait trait in district.MentalityTraits)
            {
                // Impact from own traits
                trait.ModifyPolicyPointImpact(this, ref impact);

                // Impact from neighbour traits
                foreach (District neighbour in district.AdjacentDistricts)
                {
                    foreach (MentalityTrait neighbourTrait in neighbour.MentalityTraits)
                    {
                        neighbourTrait.ModifyNeighbourPolicyPointImpact(this, neighbour, ref impact);
                    }
                }
            }
            return impact;
        }
        

        /// <summary>
        /// Returns the full value of how much this policy currently affects the party's popularity in the given district.
        /// </summary>
        public int GetCurrentImpactOn(District district)
        {
            int impact = 0;
            impact += Value * GetSinglePointImpactOn(district);
            return impact;
        }
    }
}
