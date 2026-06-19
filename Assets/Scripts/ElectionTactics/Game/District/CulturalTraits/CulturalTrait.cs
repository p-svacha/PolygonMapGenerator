using System.Collections.Generic;
using UnityEngine.Rendering;

namespace ElectionTactics
{
    public class CulturalTrait
    {
        /// <summary>
        /// Empty constructor is needed for System.Activator.
        /// </summary>
        public CulturalTrait() { }

        public CulturalTraitDef Def { get; private set; }
        public District District { get; private set; }
        public ElectionTacticsGame Game => District.Game;

        public void Init(CulturalTraitDef def, District district, bool skipOnInit = false)
        {
            Def = def;
            District = district;
            if (!skipOnInit) OnInit();
        }

        /// <summary>
        /// Called once when initializing the trait.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Called once when this trait gets removed from a district.
        /// </summary>
        public virtual void OnRemoved() { }

        /// <summary>
        /// Gets called when the district this trait belongs to gets activated.
        /// </summary>
        public virtual void OnDistrictActivated() { }

        /// <summary>
        /// Gets called right before the party popularities get calculated during the district election.
        /// </summary>
        public virtual void OnPreElection() { }

        /// <summary>
        /// Gets called at the very end of a turn after an election has ended.
        /// </summary>
        public virtual void OnPostElection() { }

        /// <summary>
        /// Returns all modifiers within the popularity calculation for the given party.
        /// </summary>
        public virtual List<(string Label, int Value)> GetPopularityChange(Party p) => new List<(string Label, int Value)>();

        /// <summary>
        /// Returns all modifiers within the popularity calculation, that change the popularity of a party in this district based on the popularity of the party in other districts. Defined this specific way to avoid chained/circular dependencies.
        /// </summary>
        public virtual List<(string Label, int Value)> GetPopularityChangeFromOtherDistrictPopularities(Party p) => new List<(string Label, int Value)>();

        /// <summary>
        /// Returns all modifiers within the popularity calculation, that are applied to neighbouring districts, that change the popularity of a party in the neighbour district based on the popularity of the party in other districts. Defined this specific way to avoid chained/circular dependencies.
        /// </summary>
        public virtual List<(string Label, int Value)> GetPopularityChangeInNeighbours(Party p) => new List<(string Label, int Value)>();

        /// <summary>
        /// Gets called when calculating the impact of a single policy point of a policy on a district with this trait.
        /// <br/>Allows to modify the value by reference.
        /// </summary>
        public virtual void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact) { }

        /// <summary>
        /// Gets called when calculating the impact of a single policy point of a policy on a neighbouring district of a district with this trait.
        /// <br/>Allows to modify the value by reference.
        /// </summary>
        public virtual void ModifyNeighbourPolicyPointImpact(District targetDistrict, Policy policy, District neighbourWithTrait, ref int impact) { }

        /// <summary>
        /// The policy jumped to when clicking on this cultural trait in the district info. Can return null for no shortcut.
        /// </summary>
        public virtual Policy GetOnClickPolicy() => null;

        /// <summary>
        /// The policy jumped to when clicking on this cultural trait in the district info. Can return null for no shortcut.
        /// </summary>
        public virtual District GetOnClickDistrict() => null;

        public virtual string Label => Def.Label;
        public string LabelCapWord => Label.CapitalizeEachWord();
        public virtual string Description => Def.Description;

        /// <summary>
        /// Flag if this trait is currently active.
        /// </summary>
        public virtual bool IsActive => true;
    }
}
