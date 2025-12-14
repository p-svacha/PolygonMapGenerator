using UnityEngine;

namespace ElectionTactics
{
    public abstract class MentalityTrait
    {
        /// <summary>
        /// Empty constructor is needed for System.Activator.
        /// </summary>
        public MentalityTrait() { }

        public MentalityTraitDef Def { get; private set; }
        public District District { get; private set; }
        public ElectionTacticsGame Game => District.Game;

        public void Init(MentalityTraitDef def, District district)
        {
            Def = def;
            District = district;
        }

        // Mentality logic

        /// <summary>
        /// Gets called at the very end of a turn after an election has ended.
        /// </summary>
        public virtual void OnPostElection() { }

        public virtual string Label => Def.Label;
        public string LabelCapWord => Label.CapitalizeEachWord();
        public virtual string Description => Def.Description;
    }
}
