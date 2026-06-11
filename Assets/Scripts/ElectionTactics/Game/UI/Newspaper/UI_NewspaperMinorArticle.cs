using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_NewspaperMinorArticle : MonoBehaviour
    {
        [Header("Elements")]
        public Image ArticleIcon;
        public TextMeshProUGUI Headline;
        public TextMeshProUGUI Body;

        public void ShowArticle(NewspaperMinorArticle article)
        {
            // todo: show icon if headline is short enough to not overlap
            ArticleIcon.sprite = article.IconSprite;
            Headline.text = article.Headline;
            Body.text = article.BodyText;
        }
    }
}
