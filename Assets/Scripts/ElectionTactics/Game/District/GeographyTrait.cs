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
        public string Description { get; protected set; }

        public GeographyTrait(GeographyTraitType type, int category)
        {
            Type = type;
            Category = category;
            BaseName = EnumHelper.GetDescription(type);
            string romanianNumber = category == 1 ? "I" : category == 2 ? "II" : "III";
            FullName = BaseName + " " + romanianNumber;
            
            switch(type)
            {
                case GeographyTraitType.Coastal:
                    Description = "This district lies on the coast.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Core:
                    Description = "This district was one of the first to be added to the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.New:
                    Description = "This district was one of the latest to be added to the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Eastern:
                    Description = "This district is located in the east of the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Western:
                    Description = "This district is located in the west of the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Southern:
                    Description = "This district is located in the south of the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Northern:
                    Description = "This district is located in the north of the country.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Tiny:
                    Description = "This district has a very small area.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Large:
                    Description = "This district has a very big area.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Landlocked:
                    Description = "This district has no ocean coastlines.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Lakeside:
                    Description = "This district is adjacent to a lake.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                case GeographyTraitType.Island:
                    Description = "This district is on an island.\nThe " + BaseName + " policy will have a " + GetImpactAdjective() + " impact on this district.";
                    break;

                default:
                    throw new System.Exception("GeographyTraitType " + type.ToString() + " not handled");
            }
        }

        private string GetImpactAdjective()
        {
            if (Category == 1) return "low";
            if (Category == 2) return "medium";
            if (Category == 3) return "high";
            throw new System.Exception();
        }
    }
}
