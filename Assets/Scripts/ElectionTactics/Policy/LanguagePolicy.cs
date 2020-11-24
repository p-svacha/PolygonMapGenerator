using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class LanguagePolicy : Policy
    {
        public Language Language;

        public LanguagePolicy(Party p, Language language, int value): base(p, value)
        {
            Language = language;
            Name = EnumHelper.GetDescription(language);
            Type = PolicyType.Language;
        }
    }
}
