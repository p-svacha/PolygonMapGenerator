using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_DistrictRemoved : RandomEvent
    {
        public District District { get; private set; }

        protected override void ExecuteEffect()
        {
            District = GetCandidates().RandomElement();
            District.Deactivate();
        }

        public override bool CanExecute()
        {
            return GetCandidates().Count() > 0;
        }

        private List<District> GetCandidates()
        {
            return Game.ActiveDistricts.Where(d => d != Game.Capital).ToList();
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 120;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Leaves the Nation",
                $"Farewell to {District.Name}",
                $"{District.Name} Secedes",
                $"{District.Name} Bows Out",
                $"The Map Shrinks: {District.Name} Departs",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"{District.Name} has left the nation and will no longer take part in elections.",
                $"In a stunning turn, {District.Name} has broken away from the country for good.",
                $"The district of {District.Name} is no more — it has formally departed the union.",
            };

            var pool = new List<string>
            {
                "Parties scramble to rewrite strategies built around the lost district.",
                "Its representatives have vacated their seats in parliament.",
                $"The {District.Density.Label.ToLower()}-density region leaves a notable gap on the map.",
                "Neighbouring districts brace for the ripple effects.",
                "Historians are already debating how it came to this.",
                $"The fate of its {District.Economy1.Label.ToLower()} industry hangs in the balance.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
