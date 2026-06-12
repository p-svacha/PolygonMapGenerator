using UnityEngine;

namespace ElectionTactics
{
    public class NewspaperMinorArticle
    {
        public Newspaper Newspaper { get; private set; }
        public Sprite IconSprite { get; private set; }
        public string Headline { get; private set; }
        public string BodyText { get; private set; } // max 50 characters when 2 articles in same column. Else max 230 characters. For <= 3 minor articles, each one gets its own column. if 4, articles 3 & 4 share a column. for 5, articles 2&3 share, and 4&5. for 6, it's just 2 per column. Max 300 characters if there's only 2 articles. Max 500 characters if there's only 1.
        public int Priority { get; set; }

        public NewspaperMinorArticle(Newspaper newspaper)
        {
            Newspaper = newspaper;
        }

        public void SetSprite(Sprite sprite)
        {
            IconSprite = sprite;
        }

        public void SetHeadline(string headline)
        {
            Headline = headline;
        }

        public void SetBodyText(string bodytext)
        {
            BodyText = bodytext;
        }
    }
}
