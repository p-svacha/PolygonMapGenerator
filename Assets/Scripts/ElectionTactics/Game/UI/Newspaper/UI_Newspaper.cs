using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Newspaper : MonoBehaviour
    {
        [Header("Elements")]
        public GameObject NewspaperContainer; // Containing everything including the background dimmer
        public GameObject Newspaper; // The newspaper including ALL its content
        public Button ConfirmButton;
        public Button CloseButton;

        public TextMeshProUGUI EditionText;
        public TextMeshProUGUI MainArticleHeadline;
        public TextMeshProUGUI MainArticleText;
        public GameObject ParliamentPreview; // todo: replace with own component

        public GameObject MinorArticlesDividerLeft; // used if there's 3+ minor articles
        public GameObject MinorArticlesDividerCenter; // used only if there's 2 minor articles
        public GameObject MinorArticlesDividerRight; // used if there's 3+ minor articles

        public UI_NewspaperMinorArticle MinorArticle1; // if 1 article: x-anchors need to be 0-1. if 2 articles: x-anchors need to be 0-0.48. if 3+articles: x-anchors need to be 0-0.32 | if <6 articles: y-anchors need to be 0-0.33. Else 0.16-0.33
        public UI_NewspaperMinorArticle MinorArticle2; // if 2 articles: x-anchors need to be 0.52-1. if 3+articles: x-anchors need to be 0.35-0.67 | if <5 articles: y-anchors need to be 0-0.33. Else 0.16-0.33
        public UI_NewspaperMinorArticle MinorArticle3; // x anchors always same | if <4 articles: y-anchors need to be 0-0.33. Else 0.16-0.33
        public UI_NewspaperMinorArticle MinorArticle4; // anchors always same
        public UI_NewspaperMinorArticle MinorArticle5; // anchors always same
        public UI_NewspaperMinorArticle MinorArticle6; // anchors always same

        /// <summary>
        /// Called once
        /// </summary>
        public void Init()
        {
            ConfirmButton.onClick.AddListener(Hide);
            CloseButton.onClick.AddListener(Hide);
        }

        public void ShowNewspaper(Newspaper newspaper, bool withAnimation = false)
        {
            // todo: fancy animation where the newspaper spins onto the screen

            EditionText.text = $"{newspaper.Year} Edition";
            MainArticleHeadline.text = newspaper.MainArticle.Headline;
            MainArticleText.text = $"{newspaper.MainArticle.Chapter1}\n\n{newspaper.MainArticle.Chapter2}";
            // todo: Parliament view

            // todo: minor articles layout
            // todo: show minor articles
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }
}
