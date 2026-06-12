using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_TraitLost : RandomEvent
    {
        protected override void ExecuteEffect()
        {
            Debug.Log("RANDOM EVENT HAPPENING (not implemented)");
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Any(d => d.CulturalTraits.Count > 0);
        }
    }
}
