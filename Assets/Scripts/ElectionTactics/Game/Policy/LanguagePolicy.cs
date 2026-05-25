using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class LanguagePolicy : Policy
    {
        public LanguageDef Language;

        public LanguagePolicy(int id, Party p, LanguageDef language, int maxValue) : base(id, p, maxValue)
        {
            Language = language;
            Name = language.Label;
            Type = PolicyType.Language;
        }

        public override int GetSinglePointBaseImpact(District district)
        {
            if (Language != district.Language) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}
