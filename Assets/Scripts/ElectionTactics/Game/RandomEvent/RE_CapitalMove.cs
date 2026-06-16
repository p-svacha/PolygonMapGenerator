using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_CapitalMove : RandomEvent
    {
        public District OldCapital { get; private set; }
        public District NewCapital { get; private set; }

        protected override void ExecuteEffect()
        {
            OldCapital = Game.Capital;
            NewCapital = Game.ActiveDistricts.Where(d => d != OldCapital).ToList().RandomElement();

            OldCapital.RemoveCulturalTrait(CulturalTraitDefOf.Capital);
            NewCapital.AddCulturalTrait(CulturalTraitDefOf.Capital);

            Game.Capital = NewCapital;
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Count > 1 && Game.Capital.IsActive;
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 100;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"Capital Moves to {NewCapital.Name}",
                $"{NewCapital.Name} Named New Capital",
                $"A New Seat of Power: {NewCapital.Name}",
                $"{OldCapital.Name} Loses Capital Status",
                $"The Capital Relocates",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"In a historic decision, the nation's capital has been moved from {OldCapital.Name} to {NewCapital.Name}.",
                $"{NewCapital.Name} has been declared the new capital, stripping {OldCapital.Name} of the honour it long held.",
                $"The seat of government has officially relocated to {NewCapital.Name}, leaving {OldCapital.Name} behind.",
            };

            var pool = new List<string>
            {
                $"Residents of {NewCapital.Name} celebrated the announcement in the streets.",
                $"In {OldCapital.Name}, the mood is one of quiet resentment.",
                "The additional parliamentary seats will follow the move.",
                "Civil servants face an uncertain relocation in the months ahead.",
                "Analysts expect the shift to reshape the political map.",
                $"Property prices in {NewCapital.Name} are already climbing.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
