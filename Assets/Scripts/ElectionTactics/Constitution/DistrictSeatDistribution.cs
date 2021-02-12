using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class DistrictSeatDistribution : ConstitutionChapter
    {
        public DistrictSeatDistributionType Type;

        // Values for all types
        private int MinSeats;
        private const int MinMinSeats = 1;
        private const int MaxMinSeats = 5;
        private const int StepMinSeats = 5;

        private bool HasUpperLimit;
        private int MaxSeats;
        private const int MinMaxSeats = 5;
        private const int MaxMaxSeats = 40;
        private const int StepMaxSeats = 1;

        // Values for proportional
        private int PropPopulationPerSeat;
        private const int MinPropPopulationPerSeat = 30000;
        private const int MaxPropPopulationPerSeat = 120000;
        private const int StepPropPopulationPerSeat = 5000;

        // Values for logarithmic (the population requirement per seat gets linearly higher for every seat. The value for the first seat is double the increase.)
        private int LogPopulationPerSeat;
        private const int MinLogPopulationPerSeat = 10000;
        private const int MaxLogPopulationPerSeat = 30000;
        private const int StepLogPopulationPerSeat = 1000;

        // Values for fixed
        private int FixedSeatAmount;
        private const int MinFixedSeatAmount = 1;
        private const int MaxFixedSeatAmount = 10;
        private const int StepFixedSeatAmount = 1;

        public DistrictSeatDistribution(ElectionTacticsGame game) : base(game)
        {
            ChapterTitle = "District Seat Distribution";
        }

        protected override void SetInitialRandomValues()
        {
            Type = GetRandomType();

            MinSeats = GetRandomConditionValue(MinMinSeats, MaxMinSeats, StepMinSeats);
            HasUpperLimit = GetRandomBool();
            MaxSeats = GetRandomConditionValue(MinMaxSeats, MaxMaxSeats, StepMaxSeats);
            PropPopulationPerSeat = GetRandomConditionValue(MinPropPopulationPerSeat, MaxPropPopulationPerSeat, StepPropPopulationPerSeat);
            LogPopulationPerSeat = GetRandomConditionValue(MinLogPopulationPerSeat, MaxLogPopulationPerSeat, StepLogPopulationPerSeat);
            FixedSeatAmount = GetRandomConditionValue(MinFixedSeatAmount, MaxFixedSeatAmount, StepFixedSeatAmount);
        }

        public int GetSeatAmountFor(int population)
        {
            // TODO: Add fixed parliament size type (i.e. exactly 100 seats in total)
            return 0;
        }

        public override string GetConstitutionText()
        {
            string s = "";
            if(Type == DistrictSeatDistributionType.Proportional)
            {
                s += "The district seat distribution is proportional.";
                s += "Every district gets a seat for every " + PropPopulationPerSeat + " inhabitants.";
                s += "\nEvery district gets at least " + MinSeats + " seats.";
                if (HasUpperLimit) s += "\nEvery district gets a maximum of " + MaxSeats + " seats.";
            }
            else if (Type == DistrictSeatDistributionType.Exponential)
            {
                s += "The district seat distribution is exponential";
                s += "A district needs " + (LogPopulationPerSeat * 2) + " inhabitants for a seat and " + LogPopulationPerSeat + " more for every additional seat";
                s += "\nEvery district gets at least " + MinSeats + " seats.";
                if (HasUpperLimit) s += "\nEvery district gets a maximum of " + MaxSeats + " seats.";
            }
            else if (Type == DistrictSeatDistributionType.Fixed)
            {
                s += "Every district gets a fixed amount of " + FixedSeatAmount + " seats.";
            }
            return s;
        }

        private static DistrictSeatDistributionType GetRandomType()
        {
            Array values = Enum.GetValues(typeof(DistrictSeatDistributionType));
            return (DistrictSeatDistributionType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }

    public enum DistrictSeatDistributionType
    {
        Proportional,
        Exponential,
        Fixed
    }
}
