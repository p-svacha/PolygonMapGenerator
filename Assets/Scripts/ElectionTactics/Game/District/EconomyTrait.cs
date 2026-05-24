using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ElectionTactics
{
    public class EconomicSectorDef : Def
    {
    }

    public static class EconomicSectorDefs
    {
        public static List<EconomicSectorDef> Defs => new List<EconomicSectorDef>()
        {
            new EconomicSectorDef()
            {
                DefName = "Mining",
                Label = "Mining",
            },

            new EconomicSectorDef()
            {
                DefName = "Fishing",
                Label = "Fishing",
            },

            new EconomicSectorDef()
            {
                DefName = "Forestry",
                Label = "Forestry",
            },

            new EconomicSectorDef()
            {
                DefName = "Pharmacy",
                Label = "Pharmacy",
            },

            new EconomicSectorDef()
            {
                DefName = "Defense",
                Label = "Defense",
            },

            new EconomicSectorDef()
            {
                DefName = "Health",
                Label = "Health",
            },

            new EconomicSectorDef()
            {
                DefName = "Aerospace",
                Label = "Aerospace",
            },

            new EconomicSectorDef()
            {
                DefName = "Electronics",
                Label = "Electronics",
            },

            new EconomicSectorDef()
            {
                DefName = "Textiles",
                Label = "Textiles",
            },

            new EconomicSectorDef()
            {
                DefName = "FossilFuels",
                Label = "Fossil Fuels",
            },

            new EconomicSectorDef()
            {
                DefName = "Renewables",
                Label = "Renewables",
            },

            new EconomicSectorDef()
            {
                DefName = "Manufacturing",
                Label = "Manufacturing",
            }
        };
    }

    [DefOf]
    public static class EconomicSectorDefOf
    {
        public static EconomicSectorDef Mining;
        public static EconomicSectorDef Fishing;
        public static EconomicSectorDef Forestry;
        public static EconomicSectorDef Pharmacy;
        public static EconomicSectorDef Arts;
        public static EconomicSectorDef Defense;
        public static EconomicSectorDef Health;
        public static EconomicSectorDef Aerospace;
        public static EconomicSectorDef Electronics;
        public static EconomicSectorDef Textiles;
        public static EconomicSectorDef FossilFuels;
        public static EconomicSectorDef Renewables;
        public static EconomicSectorDef Manufacturing;
    }
}
