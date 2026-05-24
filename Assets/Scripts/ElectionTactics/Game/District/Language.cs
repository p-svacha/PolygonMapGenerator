using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class LanguageDef : Def
    {
        public Color Color { get; init; }
    }

    public static class LanguageDefs
    {
        public static List<LanguageDef> Defs => new List<LanguageDef>()
        {
            new LanguageDef()
            {
                DefName = "English",
                Label = "English",
                Color = new Color(0.30f, 0.45f, 0.75f),
            },

            new LanguageDef()
            {
                DefName = "Mandarin",
                Label = "Mandarin",
                Color = new Color(0.80f, 0.30f, 0.30f),
            },

            new LanguageDef()
            {
                DefName = "Hindi",
                Label = "Hindi",
                Color = new Color(0.85f, 0.55f, 0.20f),
            },

            new LanguageDef()
            {
                DefName = "Spanish",
                Label = "Spanish",
                Color = new Color(0.80f, 0.70f, 0.25f),
            },

            new LanguageDef()
            {
                DefName = "French",
                Label = "French",
                Color = new Color(0.45f, 0.65f, 0.80f),
            },

            new LanguageDef() 
            {
                DefName = "Arabic",
                Label = "Arabic",
                Color = new Color(0.35f, 0.65f, 0.45f),
            },

            new LanguageDef() 
            {
                DefName = "Bengali",
                Label = "Bengali",
                Color = new Color(0.70f, 0.40f, 0.65f),
            },

            new LanguageDef() 
            {
                DefName = "Russian",
                Label = "Russian",
                Color = new Color(0.55f, 0.75f, 0.55f),
            },

            new LanguageDef() 
            {
                DefName = "Portuguese",
                Label = "Portuguese",
                Color = new Color(0.75f, 0.50f, 0.40f),
            },

            new LanguageDef() 
            {
                DefName = "German",
                Label = "German",
                Color = new Color(0.60f, 0.60f, 0.65f),
            },
        };
    }

    [DefOf]
    public static class LanguageDefOf
    {
        public static LanguageDef English;
        public static LanguageDef Mandarin;
        public static LanguageDef Hindi;
        public static LanguageDef Spanish;
        public static LanguageDef French;
        public static LanguageDef Arabic;
        public static LanguageDef Bengali;
        public static LanguageDef Russian;
        public static LanguageDef Portuguese;
        public static LanguageDef German;
    }
}
