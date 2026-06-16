using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_AgeGroupChange : RandomEvent
    {
        public District District { get; private set; }
        public AgeGroupDef OldAgeGroup { get; private set; }
        public AgeGroupDef NewAgeGroup { get; private set; }

        protected override void ExecuteEffect()
        {
            District = Game.ActiveDistricts.RandomElement();
            OldAgeGroup = District.AgeGroup;
            NewAgeGroup = DefDatabase<AgeGroupDef>.AllDefs.Where(def => def != OldAgeGroup).ToList().RandomElement();

            District.AgeGroup = NewAgeGroup;
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Sees a Generational Shift",
                $"The Faces of {District.Name} Change",
                $"A New Generation in {District.Name}",
                $"{District.Name} Ages... or Doesn't",
                $"Demographic Turn in {District.Name}",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"The dominant generation in {District.Name} has shifted from {OldAgeGroup.Label.ToLower()} to {NewAgeGroup.Label.ToLower()}.",
                $"Census figures show {District.Name} is now characterised by a {NewAgeGroup.Label.ToLower()} population, where {OldAgeGroup.Label.ToLower()} once prevailed.",
                $"{District.Name} has changed face: its {OldAgeGroup.Label.ToLower()} majority has given way to a {NewAgeGroup.Label.ToLower()} one.",
            };

            var pool = new List<string>
            {
                "Parties will need to rethink which issues resonate here.",
                "Local culture is slowly adapting to the change.",
                "Schools and services face shifting demands.",
                $"The {District.Economy1.Label.ToLower()} sector may feel the effects first.",
                "Campaign strategists are watching the district closely.",
                "What it means for the next election is anyone's guess.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
