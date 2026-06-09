using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class CulturalTraitDef : Def
    {
        /// <summary>
        /// Class of the actual trait that will be instantiated.
        /// </summary>
        public System.Type TraitClass { get; init; } = typeof(CulturalTrait);

        /// <summary>
        /// The category of this trait. Used to color code in the district info UI.
        /// </summary>
        public CulturalTraitCategoryDef Category { get; init; }

        /// <summary>
        /// How likely this trait is to appear in a district. 100 is default.
        /// </summary>
        public int Commonness { get; init; }

        /// <summary>
        /// This cultural trait can not be adopted if the district has a trait with DefName in this list.
        /// </summary>
        public List<string> ForbiddenCulturalTraits { get; init; } = new List<string>();

        /// <summary>
        /// This cultural trait can only be adopted if the district follows a religion.
        /// </summary>
        public bool RequiresReligion { get; init; }

        /// <summary>
        /// Flag for if this trait affects how seats are distributed. Only one such a trait can exist per district and they are displayed during an election if present.
        /// </summary>
        public bool IsSeatDistributionTrait { get; init; }

        /// <summary>
        /// Additive modifier in % that gets applied to the districts population growth rate.
        /// </summary>
        public float PopulationGrowthRateModifier { get; init; } = 0f;


        public override bool Validate()
        {
            if (Category == null) throw new System.Exception("Category must be set.");
            return base.Validate();
        }

        public override void ResolveReferences()
        {
            foreach(string s in ForbiddenCulturalTraits)
            {
                if (s == DefName) throw new System.Exception("Cannot have own DefName in ForbiddenCulturalTraits");
                if (!DefDatabase<CulturalTraitDef>.ContainsDef(s)) throw new System.Exception($"ForbiddenCulturalTraits has a trait defined that does not exist: {s}");
            }
        }
    }
}
