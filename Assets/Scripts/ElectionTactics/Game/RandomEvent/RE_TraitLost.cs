using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_TraitLost : RandomEvent
    {
        public District District { get; private set; }
        public CulturalTrait LostTrait { get; private set; }

        protected override void ExecuteEffect()
        {
            District = GetCandidates().RandomElement();
            LostTrait = District.ActiveCulturalTraits.Where(t => t.Def.Commonness != 0).ToList().RandomElement();

            District.RemoveCulturalTrait(LostTrait);
        }

        public override bool CanExecute()
        {
            return GetCandidates().Count() > 0;
        }

        private List<District> GetCandidates()
        {
            return Game.ActiveDistricts.Where(d => d.ActiveCulturalTraits.Count > 0 && d.ActiveCulturalTraits.Any(t => t.Def.Commonness != 0)).ToList();
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Sheds an Old Trait",
                $"An Era Ends in {District.Name}",
                $"{District.Name} Moves On",
                $"Change of Character in {District.Name}",
                $"{District.Name} Turns a Page",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"{District.Name} has shed a defining trait: it is no longer known for {LostTrait.LabelCapWord}.",
                $"The {LostTrait.LabelCapWord} character of {District.Name} has faded into history.",
                $"What once made {District.Name} distinctive — its {LostTrait.LabelCapWord} — is no more.",
            };

            var pool = new List<string>
            {
                "Some residents mourn the loss of what set them apart.",
                "Parties that relied on the old dynamic must adapt.",
                "The district enters an uncertain new chapter.",
                "Others welcome the chance for a fresh identity.",
                "Its political consequences are not yet clear.",
                "Neighbouring districts take note of the change.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
