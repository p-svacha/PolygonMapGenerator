using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Newspaper
    {
        public int Year { get; private set; }
        public int Cycle { get; private set; }
        public NewspaperMainArticle MainArticle { get; private set; }
        public List<NewspaperMinorArticle> MinorArticles { get; private set; } // Each newspaper has 1-6 of those

        public Newspaper(int year, int cycle)
        {
            Year = year;
            Cycle = cycle;
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
