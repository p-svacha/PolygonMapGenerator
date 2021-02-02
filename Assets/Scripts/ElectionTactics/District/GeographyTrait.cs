using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyTrait
    {
        public GeographyTraitType Type;
        public int Category;
        public string BaseName;
        public string FullName;

        public GeographyTrait(GeographyTraitType type, int category)
        {
            Type = type;
            Category = category;
            BaseName = EnumHelper.GetDescription(type);
            string romanianNumber = category == 1 ? "I" : category == 2 ? "II" : "III";
            FullName = BaseName + " " + romanianNumber;
        }
    }
}
