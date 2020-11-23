using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Parliament : MonoBehaviour
    {
        public GameObject PartyListContainer;
        public UI_PartyListElement PartyListElementPrefab;

        public GameObject CurrentElectionContainer;
        public Text CurrentElectionTitle;
        public WindowGraph CurrentElectionGraph;

        private List<UI_PartyListElement> ListElements = new List<UI_PartyListElement>();
        private Dictionary<UI_PartyListElement, Vector2> SourcePositions = new Dictionary<UI_PartyListElement, Vector2>();
        private Dictionary<UI_PartyListElement, Vector2> TargetPositions = new Dictionary<UI_PartyListElement, Vector2>();
        List<Party> Parties;

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
                foreach(UI_PartyListElement elem in ListElements)
                {
                    elem.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(SourcePositions[elem], TargetPositions[elem], r);
                }
                CurrentAnimationTime += Time.deltaTime;
            }
        }

        public void Init(List<Party> parties)
        {
            ListElements.Clear();
            Parties = parties;

            for (int i = 0; i < PartyListContainer.transform.childCount; i++) Destroy(PartyListContainer.transform.GetChild(i).gameObject);

            foreach (Party p in parties.OrderByDescending(x => x.Seats))
            {
                UI_PartyListElement elem = Instantiate(PartyListElementPrefab, PartyListContainer.transform, false);
                elem.Init(p);
                ListElements.Add(elem);
            }
            CalculateTargetPositions(parties);
            UpdatePartyPosition(parties);
        }

        private void UpdatePartyPosition(List<Party> parties)
        {
            foreach(KeyValuePair<UI_PartyListElement, Vector2> kvp in TargetPositions)
            {
                kvp.Key.GetComponent<RectTransform>().anchoredPosition = kvp.Value;
            }
        }

        public void MovePositions(List<Party> parties, float time)
        {
            foreach (UI_PartyListElement elem in ListElements) elem.UpdateValues();
            CalculateTargetPositions(parties);
            AnimationTime = time;
            CurrentAnimationTime = 0f;
            IsAnimating = true;
        }

        private void CalculateTargetPositions(List<Party> parties)
        {
            SourcePositions.Clear();
            TargetPositions.Clear();

            int counter = 0;
            foreach (Party p in parties.OrderByDescending(x => x.Seats))
            {
                float y = -(counter * 70);
                UI_PartyListElement elem = ListElements.First(x => x.Party == p);
                SourcePositions.Add(elem,elem.GetComponent<RectTransform>().anchoredPosition);
                TargetPositions.Add(elem,new Vector2(0, y));
                counter++;
            }
        }
    }
}
