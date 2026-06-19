using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Newspaper
    {
        public int Year => ElectionResult.Year;
        public int Cycle => ElectionResult.ElectionCycle;
        public GeneralElectionResult ElectionResult { get; private set; }
        public NewspaperMainArticle MainArticle { get; private set; }
        public List<NewspaperMinorArticle> MinorArticles { get; private set; } // Each newspaper has 1-6 of those

        public Newspaper(GeneralElectionResult electionResult)
        {
            ElectionResult = electionResult;
        }

        public void SetMainArticle(NewspaperMainArticle mainArticle)
        {
            MainArticle = mainArticle;
        }

        public void SetMinorArticles(List<NewspaperMinorArticle> minorArticles)
        {
            MinorArticles = minorArticles;
        }
    }
}
