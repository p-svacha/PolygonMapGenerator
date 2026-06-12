using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Random events represent random rare events that can happen at the end of an election in a district.
    /// </summary>
    public abstract class RandomEvent
    {
        protected static ElectionTacticsGame Game => ElectionTacticsGame.Instance;

        public RandomEventDef Def { get; private set; }
        public int Year { get; private set; }
        public int Cycle { get; private set; }

        // Newspaper
        public Sprite ArticleIcon { get; private set; }
        public string ArticleHeadline { get; protected set; }
        public string ArticleBody { get; protected set; }


        public RandomEvent() { } // Empty constructor for activator

        public void Init(RandomEventDef def)
        {
            Def = def;
        }

        public void Execute()
        {
            Year = Game.Year;
            Cycle = Game.ElectionCycle;
            ExecuteEffect();
        }

        /// <summary>
        /// Checks if this random event can happen for the current game state.
        /// <br/>This function is not allowed to depend on any internal event state and returns purely on game state.
        /// </summary>
        public virtual bool CanExecute() => true;
        protected abstract void ExecuteEffect();
    }
}
