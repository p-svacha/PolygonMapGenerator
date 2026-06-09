using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeographyTrait
    {
        public GeographyTraitDef Def { get; private set; }
        public int Category { get; private set; }
        public string Label { get; private set; }
        public string LabelWithCategory { get; private set; }
        public string Description { get; protected set; }

        public GeographyTrait(GeographyTraitDef def, int category)
        {
            Def = def;
            Category = category;
            Label = def.Label;
            Description = def.Description;
            string romanianNumber = category == 1 ? "I" : category == 2 ? "II" : "III";
            LabelWithCategory = Label + " " + romanianNumber;
        }
    }
}
