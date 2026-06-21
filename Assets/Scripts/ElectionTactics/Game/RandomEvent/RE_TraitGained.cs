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

        public override string GetArticleIconName() => "Plus";
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} gains Cultural Trait: {GainedTrait.Label}",
            };

            if (!string.IsNullOrEmpty(GainedTrait.Adjective))
            {
                templates.Add($"{District.Name} turns {GainedTrait.Adjective.CapitalizeEachWord()}");
            }

            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"{District.Name} has taken on a new defining cultural trait: {GainedTrait.LabelCapWord}.",
                $"Observers note a shift in {District.Name}, which now carries the cultural trait {GainedTrait.LabelCapWord}.",
                $"The character of {District.Name} has changed. It is now marked by the cultural trait {GainedTrait.LabelCapWord}.",
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
