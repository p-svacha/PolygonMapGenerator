using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class StartingDistrictsDef : Def
    {
        public int Value { get; init; }
    }

    public class StartingDistrictsDefs
    {
        public static List<StartingDistrictsDef> Defs => new List<StartingDistrictsDef>()
        {
            new StartingDistrictsDef()
            {
                DefName = "One",
                Label = "1",
                Value = 1,
            },

            new StartingDistrictsDef()
            {
                DefName = "Two",
                Label = "2",
                Value = 2,
            },

            new StartingDistrictsDef()
            {
                DefName = "Three",
                Label = "3 (Standard)",
                Value = 3,
            },

            new StartingDistrictsDef()
            {
                DefName = "Five",
                Label = "5",
                Value = 5,
            },

            new StartingDistrictsDef()
            {
                DefName = "Ten",
                Label = "10",
                Value = 10,
            },

            new StartingDistrictsDef()
            {
                DefName = "Twenty",
                Label = "20",
                Value = 20,
            },

            new StartingDistrictsDef()
            {
                DefName = "All",
                Label = "All (not recommended)",
                Value = -1,
            },
        };
    }

    [DefOf]
    public static class StartingDistrictsDefOf
    {
        public static StartingDistrictsDef One;
        public static StartingDistrictsDef Two;
        public static StartingDistrictsDef Three;
        public static StartingDistrictsDef Five;
        public static StartingDistrictsDef Ten;
        public static StartingDistrictsDef Twenty;
        public static StartingDistrictsDef All;
    }
}
