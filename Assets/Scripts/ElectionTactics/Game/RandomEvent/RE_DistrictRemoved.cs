using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_DistrictRemoved : RandomEvent
    {
        protected override void ExecuteEffect()
        {
            Debug.Log("RANDOM EVENT HAPPENING (not implemented)");
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Count > 3;
        }
    }
}
