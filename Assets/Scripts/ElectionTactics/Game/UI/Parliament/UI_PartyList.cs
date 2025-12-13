using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace ElectionTactics
{
    /// <summary>
    /// UI element to display a dynamic party list that can change ordering of elements with an animation.
    /// </summary>
    public class UI_PartyList : MonoBehaviour
    {
        private float ListElementWidth; // read dynamically from prefab
        private float ListElementHeight; // read dynamically from prefab

        [Header("Elements")]
        public GameObject ListContainer;
        public UI_PartyListElement PartyListElementPrefab;

        [Header("Settings")]
        public float Spacing = 10f; // Between elements
        public float Padding = 5f; // Towards edge of container
        public bool UsePartyAcronyms;
        public bool ListIsHorizontal;
        public bool ResizeContainer; // If true, size of ListContainer is dynamically set according to the elements + PADDING on each side.
        public bool ShowEliminatedParties = true;

        private Dictionary<Party, UI_PartyListElement> ListElements = new Dictionary<Party, UI_PartyListElement>();
        private Dictionary<UI_PartyListElement, Vector2> SourcePositions = new Dictionary<UI_PartyListElement, Vector2>();
        private Dictionary<UI_PartyListElement, Vector2> TargetPositions = new Dictionary<UI_PartyListElement, Vector2>();
        List<Party> Parties;

        private bool Dynamic;
        private bool IsAnimating;
        private float AnimationTime;
        private float CurrentAnimationTime;
        private float AnimationSpeedModifier = 1f;
        private Action AnimationCallback;

        void Update()
        {
            if (IsAnimating && CurrentAnimationTime >= AnimationTime)
            {
                UpdatePartyPosition(Parties);
                IsAnimating = false;
                if (AnimationCallback != null) AnimationCallback();
            }
            else if (IsAnimating)
            {
                float r = CurrentAnimationTime / AnimationTime;
                foreach (UI_PartyListElement elem in ListElements.Values)
                {
                    elem.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(SourcePositions[elem], TargetPositions[elem], r);
                }
                CurrentAnimationTime += Time.deltaTime * AnimationSpeedModifier;
            }
        }

        public void SetAnimationSpeedModifier(float speed)
        {
            AnimationSpeedModifier = speed;
        }

        public void Init(Dictionary<Party, int> values, bool dynamic)
        {
            // Reset current content
            for (int i = 0; i < ListContainer.transform.childCount; i++) Destroy(ListContainer.transform.GetChild(i).gameObject);
            ListElements.Clear();

            // Measure dimensions
            RectTransform prefabRect = PartyListElementPrefab.GetComponent<RectTransform>();
            ListElementHeight = prefabRect.sizeDelta.y;
            ListElementWidth = prefabRect.sizeDelta.x;

            IsAnimating = false;
            Parties = values.Keys.ToList();
            Dynamic = dynamic;

            // Filter values
            if (!ShowEliminatedParties) values = values.Where(x => !x.Key.IsEliminated).ToDictionary(x => x.Key, x => x.Value);

            // Create list elements
            foreach (KeyValuePair<Party, int> entry in values)
            {
                UI_PartyListElement elem = Instantiate(PartyListElementPrefab, ListContainer.transform, false);
                elem.Init(entry.Key, entry.Value.ToString(), UsePartyAcronyms);
                ListElements.Add(entry.Key, elem);
            }

            CalculateTargetPositions(values);
            UpdatePartyPosition(Parties);
            UpdateContainerSize(values.Count);
        }


        /// <summary>
        /// Starts an animation that reorders the parties according to the given values. Callback gets executed when the animation is done.
        /// </summary>
        public void MovePositionsAnimated(Dictionary<Party, int> values, float time, Action callback = null)
        {
            // Filter values
            if (!ShowEliminatedParties) values = values.Where(x => !x.Key.IsEliminated).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<Party, int> value in values) ListElements[value.Key].UpdateValue(value.Value.ToString());

            CalculateTargetPositions(values);
            AnimationTime = time;
            CurrentAnimationTime = 0f;
            IsAnimating = true;
            AnimationCallback = callback;
        }

        public void HighlightParty(Party p)
        {
            UI_PartyListElement elem = ListElements[p];
            elem.Background.color = ColorManager.Instance.UiMainLighter2;
            elem.transform.SetAsLastSibling();
        }
        public void UnhighlightParty(Party p)
        {
            ListElements[p].Background.color = ColorManager.Instance.UiMainLighter1;
        }


        private void UpdatePartyPosition(List<Party> parties)
        {
            foreach (KeyValuePair<UI_PartyListElement, Vector2> kvp in TargetPositions)
            {
                kvp.Key.GetComponent<RectTransform>().anchoredPosition = kvp.Value;
            }
        }

        private void CalculateTargetPositions(Dictionary<Party, int> values)
        {
            SourcePositions.Clear();
            TargetPositions.Clear();

            int counter = 0;
            Dictionary<Party, int> sortedValues = Dynamic ? values.OrderBy(x => x.Key.FinalRank).ThenByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value) : values;

            foreach (KeyValuePair<Party, int> entry in sortedValues)
            {
                UI_PartyListElement elem = ListElements[entry.Key];
                SourcePositions.Add(elem, elem.GetComponent<RectTransform>().anchoredPosition);

                Vector2 targetPos;

                if (ListIsHorizontal)
                {
                    // Horizontal: move right (+X)
                    float x = counter * (ListElementWidth + Spacing);
                    targetPos = new Vector2(x + Padding, Padding);
                }
                else
                {
                    // Vertical: move down (-Y)
                    float y = -(counter * (ListElementHeight + Spacing));
                    targetPos = new Vector2(Padding, y + Padding);
                }

                TargetPositions.Add(elem, targetPos);
                counter++;
            }
        }

        private void UpdateContainerSize(int elementCount)
        {
            if (!ResizeContainer) return;

            RectTransform containerRect = ListContainer.GetComponent<RectTransform>();
            Vector2 newSize = containerRect.sizeDelta;

            if (ListIsHorizontal)
            {
                float totalWidth = elementCount * ListElementWidth + (Mathf.Max(0, elementCount - 1) * Spacing);
                newSize.x = totalWidth + 2 * Padding;
                newSize.y = ListElementHeight + 2 * Padding;
            }
            else
            {
                float totalHeight = elementCount * ListElementHeight + (Mathf.Max(0, elementCount - 1) * Spacing);
                newSize.x = ListElementWidth + 2 * Padding;
                newSize.y = totalHeight + 2 * Padding;
            }

            containerRect.sizeDelta = newSize;
        }

        /// <summary>
        /// Returns the screen position center of the element displaying the provided party.
        /// </summary>
        public Vector3 GetElementCenter(Party party)
        {
            return ListElements[party].transform.position + new Vector3(ListElementWidth * 0.5f, ListElementHeight * 0.5f, 0f);
        }
    }
}
