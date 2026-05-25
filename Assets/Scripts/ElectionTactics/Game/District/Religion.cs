using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ReligionDef : Def
    {
        public Color Color { get; init; }
    }

    public static class ReligionDefs
    {
        public static List<ReligionDef> Defs => new List<ReligionDef>()
        {
            new ReligionDef()
            {
                DefName = "None",
                Label = "-",
                Color = new Color(0.55f, 0.55f, 0.55f),
            },

            new ReligionDef()
            {
                DefName = "Christian",
                Label = "Christian",
                Color = new Color(0.45f, 0.50f, 0.80f),
            },

            new ReligionDef()
            {
                DefName = "Muslim",
                Label = "Muslim",
                Color = new Color(0.35f, 0.70f, 0.55f),
            },

            new ReligionDef()
            {
                DefName = "Hindu",
                Label = "Hindu",
                Color = new Color(0.85f, 0.55f, 0.20f),
            },

            new ReligionDef()
            {
                DefName = "Buddhist",
                Label = "Buddhist",
                Color = new Color(0.80f, 0.70f, 0.25f),
            },

            new ReligionDef()
            {
                DefName = "Jewish",
                Label = "Jewish",
                Color = new Color(0.60f, 0.45f, 0.70f),
            },
        };
    }

    [DefOf]
    public static class ReligionDefOf
    {
        public static ReligionDef None;
        public static ReligionDef Christian;
        public static ReligionDef Muslim;
        public static ReligionDef Hindu;
        public static ReligionDef Buddhist;
        public static ReligionDef Jewish;
    }
}
