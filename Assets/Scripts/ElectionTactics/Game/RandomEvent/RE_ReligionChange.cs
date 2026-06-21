using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_ReligionChange : RandomEvent
    {
        public District District { get; private set; }
        public ReligionDef OldReligion { get; private set; }
        public ReligionDef NewReligion { get; private set; }

        protected override void ExecuteEffect()
        {
            District = GetCandidates().RandomElement();
            OldReligion = District.Religion;
            NewReligion = DefDatabase<ReligionDef>.AllDefs.Where(def => def != OldReligion && def != ReligionDefOf.None).ToList().RandomElement();

            District.Religion = NewReligion;
        }

        public override bool CanExecute()
        {
            return GetCandidates().Count() > 0;
        }

        private List<District> GetCandidates()
        {
            return Game.ActiveDistricts.Where(d => d.HasReligion && !d.HasCulturalTrait(CulturalTraitDefOf.Fanatics)).ToList();
        }

        public override string GetArticleIconName() => "YingYang";
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Embraces {NewReligion.Noun}",
                $"A Spiritual Shift in {District.Name}",
                $"{NewReligion.Label} Takes Hold in {District.Name}",
                $"New Faith for {District.Name}: {NewReligion.Noun}",
                $"{District.Name} Turns to {NewReligion.Noun}",
                $"Religion change in {District.Name}: {OldReligion.Noun} -> {NewReligion.Noun}",
                $"{District.Name} Turns {NewReligion.Label}",
                $"{District.Name} Turns Away From {OldReligion.Noun}",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"The dominant faith in {District.Name} has shifted from {OldReligion.Noun} to {NewReligion.Noun}.",
                $"{NewReligion.Noun} has become the prevailing religion in {District.Name}, displacing {OldReligion.Noun}.",
                $"Once a stronghold of {OldReligion.Noun}, {District.Name} now leans toward {NewReligion.Noun}.",
            };

            var pool = new List<string>
            {
                "New places of worship are appearing across the district.",
                "Community leaders speak of a peaceful transition.",
                "The shift is already reshaping local customs and holidays.",
                "Parties courting the faithful must adjust their message.",
                "Some families remain quietly devoted to the old ways.",
                "Observers debate what sparked the sudden change.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
