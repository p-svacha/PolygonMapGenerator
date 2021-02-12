using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_PartyList : MonoBehaviour
    {
        public GameObject ListContainer;
        public UI_PartyListElement PartyListElementPrefab;

        private List<UI_PartyListElement> ListElements = new List<UI_PartyListElement>();
        private Dictionary<UI_PartyListElement, Vector2> SourcePositions = new Dictionary<UI_PartyListElement, Vector2>();
        private Dictionary<UI_PartyListElement, Vector2> TargetPositions = new Dictionary<UI_PartyListElement, Vector2>();
        List<Party> Parties;

        private bool Dynamic;
        private bool IsAnimating;
        private float AnimationTime;
        private float CurrentAnimationTime;

        void Update()
        {
            if (IsAnimating && CurrentAnimationTime >= AnimationTime)
            {
                UpdatePartyPosition(Parties);
                IsAnimating = false;
            }
            else if (IsAnimating)
            {
                float r = CurrentAnimationTime / AnimationTime;
                foreach (UI_PartyListElement elem in ListElements)
                {
                    elem.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(SourcePositions[elem], TargetPositions[elem], r);
                }
                CurrentAnimationTime += Time.deltaTime;
            }
        }

        public void Init(List<Party> parties, List<string> values, bool dynamic)
        {
            ListElements.Clear();
            Parties = parties;
            Dynamic = dynamic;

            for (int i = 0; i < ListContainer.transform.childCount; i++) Destroy(ListContainer.transform.GetChild(i).gameObject);

            for(int i = 0; i < parties.Count; i++)
            {
                UI_PartyListElement elem = Instantiate(PartyListElementPrefab, ListContainer.transform, false);
                elem.Init(parties[i], values[i]);
                ListElements.Add(elem);
            }

            CalculateTargetPositions(parties);
            UpdatePartyPosition(parties);
        }

        public void MovePositions(List<Party> parties, float time)
        {
            foreach (UI_PartyListElement elem in ListElements) elem.UpdateValues();
            CalculateTargetPositions(parties);
            AnimationTime = time;
            CurrentAnimationTime = 0f;
            IsAnimating = true;
        }

        public void HighlightParty(Party p)
        {
            UI_PartyListElement elem = ListElements.First(x => x.Party == p);
            elem.Background.color = ColorManager.Colors.UiMainColorLighter2;
            elem.transform.SetAsLastSibling();
        }
        public void UnhighlightParty(Party p)
        {
            ListElements.First(x => x.Party == p).Background.color = ColorManager.Colors.UiMainColorLighter1;
        }


        private void UpdatePartyPosition(List<Party> parties)
        {
            foreach (KeyValuePair<UI_PartyListElement, Vector2> kvp in TargetPositions)
            {
                kvp.Key.GetComponent<RectTransform>().anchoredPosition = kvp.Value;
            }
        }

        private void CalculateTargetPositions(List<Party> parties)
        {
            SourcePositions.Clear();
            TargetPositions.Clear();

            int counter = 0;
            List<Party> sortedList = Dynamic ? parties.OrderByDescending(x => x.Seats).ToList() : parties;
            foreach (Party p in sortedList)
            {
                float y = -(counter * 70);
                UI_PartyListElement elem = ListElements.First(x => x.Party == p);
                SourcePositions.Add(elem, elem.GetComponent<RectTransform>().anchoredPosition);
                TargetPositions.Add(elem, new Vector2(0, y));
                counter++;
            }
        }
    }
}
