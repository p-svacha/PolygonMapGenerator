using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_DistrictAdded : NewsEvent
    {
        public District District { get; private set; }

        public NewsEvent_DistrictAdded(District district) : base()
        {
            District = district;

            // Reduce by 1 because the news gets added after the year is advanced, like this it is consistent with all other news events.
            Year--;
            Cycle--;
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 30;

        public override string GetArticleHeadline()
        {
            // Template pool — {0} is the district name. Picked by stable per-district variety.
            List<string> templates = new List<string>()
            {
                "{0} Joins the Nation",
                "Welcome, {0}!",
                "A New Voice: {0}",
                "{0} Enters the Fold",
                "{0} Joins the Republic",
                "New District: {0}",
                "{0} Comes to the Table",
                "The Map Grows: {0}",
            };

            return string.Format(templates.RandomElement(), District.Name);
        }

        public override string GetArticleBody()
        {
            // Sentence 1: the arrival (always present, varied phrasing)
            List<string> openers = new List<string>()
            {
                $"The district of {District.Name} has officially joined the nation",
                $"{District.Name} has been welcomed into the country",
                $"Citizens of {District.Name} will head to the polls for the first time",
                $"{District.Name} formally enters national politics",
            };
            string opener = openers.RandomElement();

            // Sentence 2: a characterful description built from the district's most distinctive trait
            string flavour = GetFlavourSentence();

            // Sentence 3: the economy hook (varied phrasing)
            List<string> economy = new List<string>()
            {
                $"Locally, the {District.Economy1.Label.ToLower()} sector dominates the economy.",
                $"Its economy leans heavily on {District.Economy1.Label.ToLower()}.",
                $"{District.Economy1.Label} is the lifeblood of the regional economy.",
                $"Most here make their living in {District.Economy1.Label.ToLower()}.",
            };
            string econ = economy.RandomElement();

            return $"{opener}, and will take part in elections from next cycle onward. {flavour} {econ}";
        }

        /// <summary>
        /// Builds a sentence about the district's single most distinctive characteristic,
        /// so remarkable districts get remarked upon and plain ones get varied filler.
        /// </summary>
        private string GetFlavourSentence()
        {
            var candidates = new List<string>();

            // Geography-driven flavour (only if the district actually has a notable trait)
            if (HasGeo(GeographyTraitDefOf.Island))
                candidates.Add($"Cut off by the sea, {District.Name} brings an island perspective to the table.");
            if (HasGeo(GeographyTraitDefOf.Tiny))
                candidates.Add($"What it lacks in size, {District.Name} makes up for in local pride.");
            if (HasGeo(GeographyTraitDefOf.Large))
                candidates.Add($"Sprawling across a vast area, it is one of the country's larger districts.");
            if (HasGeo(GeographyTraitDefOf.Coastal))
                candidates.Add($"Its long coastline has long shaped the character of the region.");
            if (HasGeo(GeographyTraitDefOf.Landlocked))
                candidates.Add($"Far from any shore, it is a thoroughly inland community.");
            if (HasGeo(GeographyTraitDefOf.River))
                candidates.Add($"Life here has always followed the rhythm of the river.");
            if (HasGeo(GeographyTraitDefOf.Lake))
                candidates.Add($"Its lakeside setting draws visitors and locals alike.");

            // Demographic flavour
            if (District.IsMinorityLanguage)
                candidates.Add($"Notably, {District.Language.Label} is the dominant tongue here, setting it apart from its neighbours.");
            if (District.IsMinorityReligion)
                candidates.Add($"The district stands out for its {District.Religion.Label} majority.");

            candidates.Add($"Home to a largely {District.AgeGroup.Label.ToLower()} population, it has a distinct character.");
            candidates.Add($"With a {District.Density.Label.ToLower()}-density population, daily life moves at its own pace.");

            // Generic fallbacks so there's always variety even for a featureless district
            candidates.Add($"Observers are keen to see which way its voters will lean.");
            candidates.Add($"Its political leanings remain, for now, an open question.");

            return candidates.RandomElement();
        }

        private bool HasGeo(GeographyTraitDef def) => District.Geography.Any(g => g.Def == def);
    }
}
