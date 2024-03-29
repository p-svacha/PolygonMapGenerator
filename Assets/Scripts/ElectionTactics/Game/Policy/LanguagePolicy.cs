﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class LanguagePolicy : Policy
    {
        public Language Language;

        public LanguagePolicy(int id, Party p, Language language, int maxValue) : base(id, p, maxValue)
        {
            Language = language;
            Name = EnumHelper.GetDescription(language);
            Type = PolicyType.Language;
        }
    }
}
