using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class LanguagePolicy : Policy
    {
        public Language Language;

        public LanguagePolicy(Language language, int value)
        {
            Language = language;
            Name = EnumHelper.GetDescription(language);
            Value = value;
            Type = PolicyType.Language;
        }
    }
}
