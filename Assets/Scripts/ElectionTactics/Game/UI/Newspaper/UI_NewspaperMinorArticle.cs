using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_NewspaperMinorArticle : MonoBehaviour
    {
        [Header("Elements")]
        public GameObject IconFrame;
        public Image ArticleIcon;
        public TextMeshProUGUI Headline;
        public TextMeshProUGUI Body;

        public void ShowArticle(NewspaperMinorArticle article)
        {
            Headline.text = article.Headline;

            Body.gameObject.SetActive(true);
            Body.text = article.BodyText;

            // Show icon only when there's room: title-only short headline, or body present with a short headline
            bool headlineShort = article.Headline == null ? true : article.Headline.Length <= 24;
            ArticleIcon.gameObject.SetActive(true);
            ArticleIcon.sprite = article.IconSprite;

            if (article.IsSmallVersion)
            {
                IconFrame.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                Headline.GetComponent<LayoutElement>().minHeight = 50;
                Headline.fontSizeMax = 30;
            }
            else
            {
                IconFrame.transform.localScale = Vector3.one;
                Headline.GetComponent<LayoutElement>().minHeight = 70;
                Headline.fontSizeMax = 40;
            }
        }
    }
}
