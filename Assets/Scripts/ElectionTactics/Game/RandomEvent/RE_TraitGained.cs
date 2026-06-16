using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_TraitGained : RandomEvent
    {
        public District District { get; private set; }
        public CulturalTrait GainedTrait { get; private set; }

        protected override void ExecuteEffect()
        {
            District = GetCandidates().RandomElement();
            CulturalTraitDef traitDefToAdd = Game.GetRandomAdoptableCulturalTraitDef(District);
            GainedTrait = District.AddCulturalTrait(traitDefToAdd);
        }

        public override bool CanExecute()
        {
            return GetCandidates().Count() > 0;
        }

        private List<District> GetCandidates()
        {
            return Game.ActiveDistricts.Where(d => d.CulturalTraits.Count < ElectionTacticsGame.MAX_CULTURAL_TRAITS).ToList();
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Takes On New Character",
                $"Something Shifts in {District.Name}",
                $"{District.Name}: A District Transformed",
                $"A New Trait Defines {District.Name}",
                $"Change Comes to {District.Name}",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"{District.Name} has taken on a new defining trait: {GainedTrait.LabelCapWord}.",
                $"Observers note a shift in {District.Name}, which now carries the hallmark of {GainedTrait.LabelCapWord}.",
                $"The character of {District.Name} has changed — it is now marked by {GainedTrait.LabelCapWord}.",
            };

            var pool = new List<string>
            {
                "Parties will have to account for the new dynamic.",
                "Locals say the change has been brewing for some time.",
                "Its effects on the next election remain to be seen.",
                "The district feels different to those who know it well.",
                "Strategists are already recalculating their approach.",
                "Whether it lasts is a question for the cycles ahead.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
