using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Random events represent random rare events that can happen at the end of an election in a district.
    /// </summary>
    public class RandomEvent
    {
        protected ElectionTacticsGame Game => ElectionTacticsGame.Instance;

        public RandomEventDef Def { get; private set; }
        public int Year { get; private set; }
        public int Cycle { get; private set; }

        public RandomEvent(RandomEventDef def)
        {
            Def = def;
        }

        public void Execute()
        {
            Year = Game.Year;
            Cycle = Game.ElectionCycle;
            Def.Execute();
        }
    }
}
