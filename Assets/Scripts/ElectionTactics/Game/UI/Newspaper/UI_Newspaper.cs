using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Newspaper : MonoBehaviour
    {
        private Newspaper CurrentNewspaper;
        private int CurrentNewspaperIndex;

        [Header("Elements")]
        public GameObject NewspaperContainer; // Containing everything including the background dimmer
        public GameObject Newspaper; // The newspaper including ALL its content
        public Button ConfirmButton;
        public Button CloseButton;

        public TextMeshProUGUI EditionText;
        public TextMeshProUGUI MainArticleHeadline;
        public TextMeshProUGUI MainArticleText;

        public UI_HemicycleChart HemiCycleChart;
        public TextMeshProUGUI ChartText;
        public GameObject PartyListContainer;

        public GameObject MinorArticlesDividerLeft; // used if there's 3+ minor articles
        public GameObject MinorArticlesDividerCenter; // used only if there's 2 minor articles
        public GameObject MinorArticlesDividerRight; // used if there's 3+ minor articles

        public UI_NewspaperMinorArticle MinorArticle1;
        public UI_NewspaperMinorArticle MinorArticle2;
        public UI_NewspaperMinorArticle MinorArticle3;
        public UI_NewspaperMinorArticle MinorArticle4;
        public UI_NewspaperMinorArticle MinorArticle5;
        public UI_NewspaperMinorArticle MinorArticle6;

        public Button PrevNewspaperButton;
        public Button NextNewspaperButton;

        [Header("Prefabs")]
        public UI_NewspaperPartyRow PartyRowPrefab;

        private const float SHOW_ANIMATION_DURATION = 2f;
        private const float SHOW_ANIMATION_ROTATIONS = 3f; // full turns while spiraling in
        private Coroutine showAnimationCoroutine;

        /// <summary>
        /// Called once
        /// </summary>
        public void Init()
        {
            ConfirmButton.onClick.AddListener(ButtonHide);
            CloseButton.onClick.AddListener(ButtonHide);

            PrevNewspaperButton.onClick.AddListener(GoToPrevNewspaper);
            NextNewspaperButton.onClick.AddListener(GoToNextNewspaper);
        }

        private UI_NewspaperMinorArticle[] Slots => new[]
        { MinorArticle1, MinorArticle2, MinorArticle3, MinorArticle4, MinorArticle5, MinorArticle6 };

        public void ShowNewspaper(Newspaper newspaper, bool withAnimation = false)
        {
            CurrentNewspaper = newspaper;
            CurrentNewspaperIndex = ElectionTacticsGame.Instance.Newspapers.IndexOf(CurrentNewspaper);

            gameObject.SetActive(true);
            PrevNewspaperButton.gameObject.SetActive(false);
            NextNewspaperButton.gameObject.SetActive(false);

            // Header
            EditionText.text = $"{newspaper.Year} Edition";

            // Main article
            GeneralElectionResult electionResult = newspaper.ElectionResult;
            MainArticleHeadline.text = newspaper.MainArticle.Headline;
            MainArticleText.text = $"{newspaper.MainArticle.Chapter1}\n\n{newspaper.MainArticle.Chapter2}";
            HemiCycleChart.ShowParliament(electionResult);

            ChartText.text = $"Parliament of {electionResult.Year + 1}\n{electionResult.GetTotalSeats()} Seats";
            HelperFunctions.DestroyAllChildredImmediately(PartyListContainer);
            Dictionary<Party, int> partyList = electionResult.SeatsWon.Where(x => x.Value > 0).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (var party in partyList)
            {
                UI_NewspaperPartyRow elem = GameObject.Instantiate(PartyRowPrefab, PartyListContainer.transform);
                elem.Init(party.Key, party.Value);
            }

            // Minor articles
            LayoutMinorArticles(newspaper.MinorArticles);

            RectTransform newspaperRect = Newspaper.GetComponent<RectTransform>();

            if (withAnimation)
            {
                if (showAnimationCoroutine != null) StopCoroutine(showAnimationCoroutine);
                showAnimationCoroutine = StartCoroutine(SpiralIn(newspaperRect));
            }
            else
            {
                AudioManager.PlaySound(AudioManager.Instance.NewspaperRustle1, volume: 0.4f);
                newspaperRect.localScale = Vector3.one;
                newspaperRect.localRotation = Quaternion.identity;
                RefreshCycleButtons();
            }
        }

        private IEnumerator SpiralIn(RectTransform rect)
        {
            float t = 0f;
            float totalAngle = 360f * SHOW_ANIMATION_ROTATIONS;

            rect.localScale = Vector3.zero;
            rect.localRotation = Quaternion.Euler(0, 0, totalAngle);

            AudioManager.PlaySound(AudioManager.Instance.NewspaperSpin, volume: 0.7f);

            while (t < SHOW_ANIMATION_DURATION)
            {
                t += Time.deltaTime;
                float r = Mathf.Clamp01(t / SHOW_ANIMATION_DURATION);

                // Ease-out so it decelerates as it settles — feels like it "lands"
                float eased = 1f - Mathf.Pow(1f - r, 3f);

                rect.localScale = Vector3.one * eased;
                rect.localRotation = Quaternion.Euler(0, 0, totalAngle * (1f - eased));

                yield return null;
            }

            AudioManager.PlaySound(AudioManager.Instance.NewspaperSpinEnd, volume: 0.8f);

            // Snap to final state
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            showAnimationCoroutine = null;

            RefreshCycleButtons();
        }

        private void LayoutMinorArticles(List<NewspaperMinorArticle> articles)
        {
            int n = Mathf.Clamp(articles.Count, 0, 6);

            // Dividers: 2 articles -> center divider only; 3+ -> left & right dividers
            MinorArticlesDividerCenter.SetActive(n == 2);
            MinorArticlesDividerLeft.SetActive(n >= 3);
            MinorArticlesDividerRight.SetActive(n >= 3);

            // Hide all slots first
            foreach (var slot in Slots) slot.gameObject.SetActive(false);

            // Column assignment: which slot index goes in which column, and whether it's alone (gets body)
            // Returns per-article: (xMin, xMax, yMin, yMax, showBody)
            var placements = GetPlacements(n);

            for (int i = 0; i < n; i++)
            {
                UI_NewspaperMinorArticle slot = Slots[i];
                slot.gameObject.SetActive(true);

                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(placements[i].xMin, placements[i].yMin);
                rt.anchorMax = new Vector2(placements[i].xMax, placements[i].yMax);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                slot.ShowArticle(articles[i]);
            }
        }

        private struct Placement { public float xMin, xMax, yMin, yMax; public bool showBody; }

        private Placement[] GetPlacements(int n)
        {
            // Column x-ranges
            float[] x1 = { 0f, 1f };
            float[][] x2 = { new[] { 0f, 0.48f }, new[] { 0.52f, 1f } };
            float[][] x3 = { new[] { 0f, 0.32f }, new[] { 0.35f, 0.65f }, new[] { 0.68f, 1f } };

            // y-ranges: top row vs bottom row when a column holds two
            float[] yFull = { 0f, 1f };
            float[] yTop = { 0.51f, 1f };
            float[] yBot = { 0f, 0.49f };

            var p = new Placement[n];

            switch (n)
            {
                case 1:
                    p[0] = Make(x1[0], x1[1], yFull, isSmallVersion: false);
                    break;
                case 2:
                    p[0] = Make(x2[0][0], x2[0][1], yFull, isSmallVersion: false);
                    p[1] = Make(x2[1][0], x2[1][1], yFull, isSmallVersion: false);
                    break;
                case 3:
                    for (int i = 0; i < 3; i++) p[i] = Make(x3[i][0], x3[i][1], yFull, isSmallVersion: false);
                    break;
                case 4:
                    // [1][1][2]: col3 holds articles 3 & 4 (title-only)
                    p[0] = Make(x3[0][0], x3[0][1], yFull, isSmallVersion: false);
                    p[1] = Make(x3[1][0], x3[1][1], yFull, isSmallVersion: false);
                    p[2] = Make(x3[2][0], x3[2][1], yTop, isSmallVersion: true);
                    p[3] = Make(x3[2][0], x3[2][1], yBot, isSmallVersion: true);
                    break;
                case 5:
                    // [1][2][2]: col2 holds 2&3, col3 holds 4&5
                    p[0] = Make(x3[0][0], x3[0][1], yFull, isSmallVersion: false);
                    p[1] = Make(x3[1][0], x3[1][1], yTop, isSmallVersion: true);
                    p[2] = Make(x3[1][0], x3[1][1], yBot, isSmallVersion: true);
                    p[3] = Make(x3[2][0], x3[2][1], yTop, isSmallVersion: true);
                    p[4] = Make(x3[2][0], x3[2][1], yBot, isSmallVersion: true);
                    break;
                case 6:
                    // [2][2][2]
                    for (int c = 0; c < 3; c++)
                    {
                        p[c * 2] = Make(x3[c][0], x3[c][1], yTop, isSmallVersion: true);
                        p[c * 2 + 1] = Make(x3[c][0], x3[c][1], yBot, isSmallVersion: true);
                    }
                    break;
            }
            return p;
        }

        private Placement Make(float xMin, float xMax, float[] y, bool isSmallVersion)
            => new Placement { xMin = xMin, xMax = xMax, yMin = y[0], yMax = y[1], showBody = isSmallVersion };

        private void ButtonHide()
        {
            AudioManager.PlaySound(AudioManager.Instance.NewspaperRustle2, volume: 0.4f);
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void GoToNextNewspaper()
        {
            ShowNewspaper(ElectionTacticsGame.Instance.Newspapers[CurrentNewspaperIndex + 1]);
        }

        private void GoToPrevNewspaper()
        {
            ShowNewspaper(ElectionTacticsGame.Instance.Newspapers[CurrentNewspaperIndex - 1]);
        }

        private void RefreshCycleButtons()
        {
            PrevNewspaperButton.gameObject.SetActive(CurrentNewspaperIndex > 0);
            NextNewspaperButton.gameObject.SetActive(ElectionTacticsGame.Instance.Newspapers.Count > CurrentNewspaperIndex + 1);
        }
    }
}
