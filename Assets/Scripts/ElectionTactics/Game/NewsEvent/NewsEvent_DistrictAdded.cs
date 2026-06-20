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

        public override string GetArticleIconName() => "District";
        public override int GetArticlePriority() => 30;

        public override string GetArticleHeadline()
        {
            if (District.IsCapital) return $"The Capital Joins: {District.Name}";

            int numDistrict = Game.ActiveDistricts.Count;

            List<string> candidates = new List<string>()
            {
                $"{District.Name} Joins the Nation",
                $"Welcome, {District.Name}!",
                $"A New Voice: {District.Name}",
                $"{District.Name} Joins the Republic",
                $"New District: {District.Name}",
                $"{District.Name} Comes to the Table",
                $"The Map Grows with {District.Name}",
                $"The Next District is {District.Name}",
                $"{District.Name} Becomes the {numDistrict.ToOrdinal()} District",
                $"District #{numDistrict}: {District.Name}",
                $"{District.Name} Enters the Fold",
                $"{District.Name}: The Nation's {numDistrict.ToOrdinal()} District",
                $"Introducing {District.Name}",
                $"The Nation Expands to {District.Name}",
            };

            return candidates.RandomElement();
        }

        public override string GetArticleBody()
        {
            // Sentence 1: the arrival (always present, varied phrasing)
            List<string> openers = new List<string>()
            {
                $"The district of {District.Name} has officially joined the nation.",
                $"{District.Name} has been welcomed into the country.",
                $"Citizens of {District.Name} will head to the polls for the first time.",
                $"{District.Name} formally enters national politics.",
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

            return $"{opener} {flavour} {econ}";
        }

        /// <summary>
        /// Builds a sentence about the district's single most distinctive characteristic,
        /// so remarkable districts get remarked upon and plain ones get varied filler.
        /// </summary>
        private string GetFlavourSentence()
        {
            var candidates = new List<string>();

            bool firstLanguage = Game.ActiveDistricts.Where(d => d.Language == District.Language).Count() == 1;
            bool firstReligion = District.HasReligion && Game.ActiveDistricts.Where(d => d.Religion == District.Religion).Count() == 1;
            bool firstAgeGroup = Game.ActiveDistricts.Where(d => d.AgeGroup == District.AgeGroup).Count() == 1;
            bool firstDensity = Game.ActiveDistricts.Where(d => d.Density == District.Density).Count() == 1;

            bool firstAny = firstLanguage || firstReligion || firstAgeGroup || firstDensity;
            if (firstAny)
            {
                if (firstLanguage) candidates.Add($"The {District.GetDescripiveLabel()} is the first to speak {District.Language.Label}.");
                if (firstReligion) candidates.Add($"The {District.GetDescripiveLabel()} is the first with a {District.Religion.Label} faith.");
                if (firstAgeGroup) candidates.Add($"The {District.GetDescripiveLabel()} is the first with the primary age group {District.AgeGroup.Label}.");
                if (firstAgeGroup) candidates.Add($"The {District.GetDescripiveLabel()} is the first with a {District.Density.Label} density.");

                return candidates.RandomElement();
            }

            // Geography-driven flavour (only if the district actually has a notable trait)
            if (HasGeo(GeographyTraitDefOf.Island))
                candidates.Add($"Cut off by the sea, the {District.GetDescripiveLabel()} brings an island perspective to the table.");
            if (HasGeo(GeographyTraitDefOf.Tiny))
                candidates.Add($"What it lacks in size, the {District.GetDescripiveLabel()} makes up for in local pride.");
            if (HasGeo(GeographyTraitDefOf.Large))
                candidates.Add($"Sprawling across a vast area, the {District.GetDescripiveLabel()} is one of the country's larger districts.");
            if (HasGeo(GeographyTraitDefOf.Coastal))
                candidates.Add($"The long coastline of the {District.GetDescripiveLabel()} has long shaped the character of the region.");
            if (HasGeo(GeographyTraitDefOf.Landlocked))
                candidates.Add($"Far from any shore, the {District.GetDescripiveLabel()} is a thoroughly inland community.");
            if (HasGeo(GeographyTraitDefOf.River))
                candidates.Add($"Life in the {District.GetDescripiveLabel()} has always followed the rhythm of the river.");
            if (HasGeo(GeographyTraitDefOf.Lake))
                candidates.Add($"The lakeside setting of the {District.GetDescripiveLabel()} draws visitors and locals alike.");

            // Demographic flavour
            if (District.IsMinorityLanguage)
                candidates.Add($"Notably, {District.Language.Label} is the dominant tongue in the {District.GetDescripiveLabel()}, setting it apart from many other districts.");
            if (District.IsMinorityReligion)
                candidates.Add($"The {District.GetDescripiveLabel()} stands out for its {District.Religion.Label} faith, which is uncommon.");

            candidates.Add($"Home to a largely {District.AgeGroup.Label.ToLower()} population, it has a distinct character.");
            candidates.Add($"With a {District.Density.Label.ToLower()}-density population, daily life moves at its own pace.");

            // Generic fallbacks so there's always variety even for a featureless district
            candidates.Add($"Observers are keen to see which way voters of the {District.GetDescripiveLabel()} will lean.");
            candidates.Add($"The political leanings of the the {District.GetDescripiveLabel()} remain, for now, an open question.");

            return candidates.RandomElement();
        }

        private bool HasGeo(GeographyTraitDef def) => District.Geography.Any(g => g.Def == def);
    }
}
