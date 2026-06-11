using UnityEngine;

namespace ElectionTactics
{
    public class RE_Disaster : RandomEventDef
    {
        public override bool CanExecute()
        {
            return true;
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
