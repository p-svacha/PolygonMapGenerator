using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_LanguageChange : RandomEvent
    {
        public District District { get; private set; }
        public LanguageDef OldLanguage { get; private set; }
        public LanguageDef NewLanguage { get; private set; }

        protected override void ExecuteEffect()
        {
            District = Game.ActiveDistricts.RandomElement();
            OldLanguage = District.Language;
            NewLanguage = DefDatabase<LanguageDef>.AllDefs.Where(def => def != OldLanguage).ToList().RandomElement();

            District.Language = NewLanguage;
        }

        public override Sprite GetArticleIcon() => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");
        public override int GetArticlePriority() => 70;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{District.Name} Changes Its Tongue",
                $"A New Language for {District.Name}",
                $"{NewLanguage.Label} Rises in {District.Name}",
                $"Linguistic Shift in {District.Name}",
                $"{District.Name} Finds Its Voice Anew",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"The prevailing language of {District.Name} has shifted from {OldLanguage.Label} to {NewLanguage.Label}.",
                $"{NewLanguage.Label} has overtaken {OldLanguage.Label} as the dominant tongue in {District.Name}.",
                $"Where {OldLanguage.Label} was once heard on every corner of {District.Name}, {NewLanguage.Label} now prevails.",
            };

            var pool = new List<string>
            {
                "Signage and schooling are slowly being updated to match.",
                "Older residents lament the fading of the old tongue.",
                "Campaign materials will need translating before the next vote.",
                "Cultural ties to neighbouring districts are quietly realigning.",
                "Some welcome the change as a fresh chapter.",
                "Linguists are fascinated by how quickly it took hold.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
