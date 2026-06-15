using UnityEngine;

namespace ElectionTactics
{
    public abstract class NewsEvent
    {
        protected static ElectionTacticsGame Game => ElectionTacticsGame.Instance;
        public int Year { get; protected set; }
        public int Cycle { get; protected set; }

        // Newspaper article content
        public virtual Sprite ArticleIcon => ResourceManager.LoadSprite("ElectionTactics/Newspaper/ArticleSymbol_Star");

        public NewsEvent()
        {
            Year = Game.Year;
            Cycle = Game.ElectionCycle;
        }

        public abstract string GetArticleHeadline();
        public abstract string GetArticleBody();
        public abstract int GetArticlePriority();
    }
}
