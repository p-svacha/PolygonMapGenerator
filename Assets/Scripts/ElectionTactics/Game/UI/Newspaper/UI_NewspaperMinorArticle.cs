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

        public void ShowArticle(NewspaperMinorArticle article, bool showBody)
        {
            Headline.text = article.Headline;

            Body.gameObject.SetActive(showBody && !string.IsNullOrEmpty(article.BodyText));
            if (showBody) Body.text = article.BodyText;

            // Show icon only when there's room: title-only short headline, or body present with a short headline
            bool headlineShort = article.Headline == null ? true : article.Headline.Length <= 24;
            ArticleIcon.gameObject.SetActive(true);
            ArticleIcon.sprite = article.IconSprite;
        }
    }
}
