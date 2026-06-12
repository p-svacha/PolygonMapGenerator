using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_TraitGained : RandomEvent
    {
        protected override void ExecuteEffect()
        {
            Debug.Log("RANDOM EVENT HAPPENING (not implemented)");
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Any(d => d.CulturalTraits.Count < ElectionTacticsGame.MAX_CULTURAL_TRAITS);
        }
    }
}
