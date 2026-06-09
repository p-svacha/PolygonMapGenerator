using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyTraitDef : Def { }

    public class GeographyTraitDefs
    {
        public static List<GeographyTraitDef> Defs => new List<GeographyTraitDef>()
        {
            new GeographyTraitDef()
            {
                DefName = "Coastal",
                Label = "Coastal",
                Description = "Situated along the ocean coast."
            },

            new GeographyTraitDef()
            {
                DefName = "Landlocked",
                Label = "Landlocked",
                Description = "Surrounded entirely by land with no ocean access."
            },

            new GeographyTraitDef()
            {
                DefName = "Island",
                Label = "Island",
                Description = "Located on a small island landmass."
            },

            new GeographyTraitDef()
            {
                DefName = "Tiny",
                Label = "Tiny",
                Description = "Very small district by area."
            },

            new GeographyTraitDef()
            {
                DefName = "Large",
                Label = "Large",
                Description = "Very large district by area."
            },

            new GeographyTraitDef()
            {
                DefName = "East",
                Label = "East",
                Description = "Located in the eastern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "West",
                Label = "West",
                Description = "Located in the western part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "North",
                Label = "North",
                Description = "Located in the northern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "South",
                Label = "South",
                Description = "Located in the southern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "Lake",
                Label = "Lake",
                Description = "Borders an inland lake."
            },

            new GeographyTraitDef()
            {
                DefName = "Core",
                Label = "Core",
                Description = "One of the very first districts of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "New",
                Label = "New",
                Description = "Recently joined the country. This trait loses significance and fades over time."
            },

            new GeographyTraitDef()
            {
                DefName = "River",
                Label = "River",
                Description = "Situated along a river."
            },
        };
    }

    [DefOf]
    public static class GeographyTraitDefOf
    {
        public static GeographyTraitDef Coastal;
        public static GeographyTraitDef Landlocked;
        public static GeographyTraitDef Island;
        public static GeographyTraitDef Tiny;
        public static GeographyTraitDef Large;
        public static GeographyTraitDef North;
        public static GeographyTraitDef South;
        public static GeographyTraitDef East;
        public static GeographyTraitDef West;
        public static GeographyTraitDef Lake;
        public static GeographyTraitDef River;
        public static GeographyTraitDef Core;
        public static GeographyTraitDef New;
    }
}
