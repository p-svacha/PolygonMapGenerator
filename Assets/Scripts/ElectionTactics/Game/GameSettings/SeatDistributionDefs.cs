using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class SeatDistributionGameSettingDef : Def
    {
        public SeatAllocationMethodDef AllocationMethod { get; init; }
    }

    public class SeatDistributionGameSettingDefs
    {
        public static List<SeatDistributionGameSettingDef> Defs => new List<SeatDistributionGameSettingDef>()
        {
            new SeatDistributionGameSettingDef()
            {
                DefName = "Mixed",
                Label = "Mixed (Standard)",
                AllocationMethod = null,
            },

            new SeatDistributionGameSettingDef()
            {
                DefName = "WTA",
                Label = "Winner Takes All",
                AllocationMethod = SeatAllocationMethodDefOf.WinnerTakesAll,
            },

            new SeatDistributionGameSettingDef()
            {
                DefName = "Hamilton",
                Label = "Proportional Representation",
                AllocationMethod = SeatAllocationMethodDefOf.HamiltonPR,
            },

            new SeatDistributionGameSettingDef()
            {
                DefName = "DHondt",
                Label = "Majority Representation",
                AllocationMethod = SeatAllocationMethodDefOf.DHondtPR,
            },
        };
    }

    [DefOf]
    public static class SeatDistributionGameSettingDefOf
    {
        public static SeatDistributionGameSettingDef Mixed;
        public static SeatDistributionGameSettingDef WTA;
        public static SeatDistributionGameSettingDef Hamilton;
        public static SeatDistributionGameSettingDef DHondt;
    }
}
