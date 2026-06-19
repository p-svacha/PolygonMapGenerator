using UnityEngine;

namespace ElectionTactics
{
    public abstract class NewsEvent
    {
        protected static ElectionTacticsGame Game => ElectionTacticsGame.Instance;
        public int Year { get; protected set; }
        public int Cycle { get; protected set; }

        public NewsEvent()
        {
            Year = Game.Year;
            Cycle = Game.ElectionCycle;
        }

        public abstract string GetArticleIconName();
        public abstract string GetArticleHeadline();
        public abstract string GetArticleBody();
        public abstract int GetArticlePriority();
    }
}
