using UnityEngine;

namespace ElectionTactics
{
    public class NewspaperMainArticle
    {
        public Newspaper Newspaper { get; private set; }
        public string Headline { get; private set; }
        public string Chapter1 { get; private set; }
        public string Chapter2 { get; private set; }

        public NewspaperMainArticle(Newspaper newspaper)
        {
            Newspaper = newspaper;
        }

        public void SetHeadline(string headline)
        {
            Headline = headline;
        }

        public void SetChapter1(string text)
        {
            Chapter1 = text;
        }

        public void SetChapter2(string text)
        {
            Chapter2 = text;
        }
    }
}
