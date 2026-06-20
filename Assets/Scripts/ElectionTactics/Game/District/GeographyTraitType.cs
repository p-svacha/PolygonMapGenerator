using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyTraitDef : Def
    {
        /// <summary>
        /// Adjective that can appear in the newspaper to describe districts with this trait. Can be empty.
        /// </summary>
        public string Adjective { get; init; } = "";

        /// <summary>
        /// Describing string that can appear in the newspaper to describe districts with this trait. Should be used to show what comes after "the district ..." Can be empty.
        /// </summary>
        public string Describer { get; init; } = "";
    }

    public class GeographyTraitDefs
    {
        public static List<GeographyTraitDef> Defs => new List<GeographyTraitDef>()
        {
            new GeographyTraitDef()
            {
                DefName = "Coastal",
                Label = "Coastal",
                Adjective = "coastal",
                Describer = "with long coastlines",
                Description = "Situated along the ocean coast."
            },

            new GeographyTraitDef()
            {
                DefName = "Landlocked",
                Label = "Landlocked",
                Adjective = "landlocked",
                Describer = "enclosed by land",
                Description = "Surrounded entirely by land with no ocean access."
            },

            new GeographyTraitDef()
            {
                DefName = "Island",
                Label = "Island",
                Adjective = "insular",
                Describer = "located on an island",
                Description = "Located on a small island landmass."
            },

            new GeographyTraitDef()
            {
                DefName = "Tiny",
                Label = "Tiny",
                Adjective = "tiny",
                Describer = "with a tiny area",
                Description = "Very small district by area."
            },

            new GeographyTraitDef()
            {
                DefName = "Large",
                Label = "Large",
                Adjective = "large",
                Describer = "with a large area",
                Description = "Very large district by area."
            },

            new GeographyTraitDef()
            {
                DefName = "East",
                Label = "East",
                Adjective = "eastern",
                Describer = "located in the far east",
                Description = "Located in the eastern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "West",
                Label = "West",
                Adjective = "western",
                Describer = "located in the far west",
                Description = "Located in the western part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "North",
                Label = "North",
                Adjective = "northern",
                Describer = "located in the far north",
                Description = "Located in the northern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "South",
                Label = "South",
                Adjective = "southern",
                Describer = "located in the far south",
                Description = "Located in the southern part of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "Lake",
                Label = "Lake",
                Adjective = "lakeside",
                Describer = "located next to a lake",
                Description = "Borders an inland lake."
            },

            new GeographyTraitDef()
            {
                DefName = "Core",
                Label = "Core",
                Adjective = "core",
                Describer = "that has been around forever",
                Description = "One of the very first districts of the country."
            },

            new GeographyTraitDef()
            {
                DefName = "New",
                Label = "New",
                Adjective = "new",
                Describer = "that has joined the country just recently",
                Description = "Recently joined the country. This trait loses significance and fades over time."
            },

            new GeographyTraitDef()
            {
                DefName = "River",
                Label = "River",
                Adjective = "riverside",
                Describer = "located along a river",
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
