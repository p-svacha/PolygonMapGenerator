using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTraitDef : Def
    {
        /// <summary>
        /// Class of the actual trait that will be instantiated.
        /// </summary>
        public System.Type TraitClass { get; init; } = typeof(MentalityTrait);

        /// <summary>
        /// How likely this trait is to appear in a district. 100 is default.
        /// </summary>
        public int Commonness { get; init; }

        /// <summary>
        /// This mentality trait can not be adopted if the district has a trait with DefName in this list.
        /// </summary>
        public List<string> ForbiddenMentalityTraits { get; init; } = new List<string>();

        /// <summary>
        /// This mentality trait can only be adopted if the district follows a religion.
        /// </summary>
        public bool RequiresReligion { get; init; }
    }
}
