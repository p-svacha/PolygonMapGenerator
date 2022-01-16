using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_PartyList : MonoBehaviour
    {
        private float ListElementHeight;
        private const float PADDING = 10f;

        public GameObject ListContainer;
        public UI_PartyListElement PartyListElementPrefab;

        private Dictionary<Party, UI_PartyListElement> ListElements = new Dictionary<Party, UI_PartyListElement>();
        private Dictionary<UI_PartyListElement, Vector2> SourcePositions = new Dictionary<UI_PartyListElement, Vector2>();
        private Dictionary<UI_PartyListElement, Vector2> TargetPositions = new Dictionary<UI_PartyListElement, Vector2>();
        List<Party> Parties;

        private bool Dynamic;
        private bool IsAnimating;
        private float AnimationTime;
        private float CurrentAnimationTime;
        private float AnimationSpeedModifier;
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
            ListElements.Clear();
            ListElementHeight = PartyListElementPrefab.GetComponent<RectTransform>().sizeDelta.y;
            IsAnimating = false;
            Parties = values.Keys.ToList();
            Dynamic = dynamic;

            for (int i = 0; i < ListContainer.transform.childCount; i++) Destroy(ListContainer.transform.GetChild(i).gameObject);

            foreach(KeyValuePair<Party, int> entry in values)
            {
                UI_PartyListElement elem = Instantiate(PartyListElementPrefab, ListContainer.transform, false);
                elem.Init(entry.Key, entry.Value.ToString());
                ListElements.Add(entry.Key, elem);
            }

            CalculateTargetPositions(values);
            UpdatePartyPosition(Parties);
        }


        /// <summary>
        /// Starts an animation that reorders the parties according to the given values. Callback gets executed when the animation is done.
        /// </summary>
        public void MovePositionsAnimated(Dictionary<Party, int> values, float time, Action callback = null)
        {
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
            elem.Background.color = ColorManager.Singleton.UiMainLighter2;
            elem.transform.SetAsLastSibling();
        }
        public void UnhighlightParty(Party p)
        {
            ListElements[p].Background.color = ColorManager.Singleton.UiMainLighter1;
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
            Dictionary<Party, int> sortedValues = Dynamic ? values.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value) : values;
            foreach (KeyValuePair<Party, int> entry in sortedValues)
            {
                float y = -(counter * (ListElementHeight + PADDING));
                UI_PartyListElement elem = ListElements[entry.Key];
                SourcePositions.Add(elem, elem.GetComponent<RectTransform>().anchoredPosition);
                TargetPositions.Add(elem, new Vector2(0, y));
                counter++;
            }
        }
    }
}
