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
            OnInit();
        }

        #region Mentality effect

        /// <summary>
        /// Called once when initializing the trait.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Gets called at the very end of a turn after an election has ended.
        /// </summary>
        public virtual void OnPostElection() { }

        /// <summary>
        /// Gets called when calculating the impact of a single policy point of a policy on a district with this trait.
        /// <br/>Allows to modify the value by reference.
        /// </summary>
        public virtual void ModifyPolicyPointImpact(Policy policy, ref int impact) { }

        /// <summary>
        /// Gets called when calculating the impact of a single policy point of a policy on a neighbouring district of a district with this trait.
        /// <br/>Allows to modify the value by reference.
        /// </summary>
        public virtual void ModifyNeighbourPolicyPointImpact(Policy policy, District sourceWithTrait, ref int impact) { }

        #endregion

        public virtual string Label => Def.Label;
        public string LabelCapWord => Label.CapitalizeEachWord();
        public virtual string Description => Def.Description;
    }
}
