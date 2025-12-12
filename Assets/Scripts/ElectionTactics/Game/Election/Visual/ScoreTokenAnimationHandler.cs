using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ScoreTokenAnimationHandler : MonoBehaviour
    {
        public static ScoreTokenAnimationHandler Instance;

        [Header("Elements")]
        public GameObject TokenContainer;

        [Header("Prefabs")]
        public UI_ScoreToken ScoreTokenPrefab;

        private List<UI_ScoreToken> Tokens = new List<UI_ScoreToken>();

        private bool IsAnimating;
        private float AnimationTime;
        private float CurrentAnimationTime;
        private float AnimationSpeedModifier = 1f;
        private Action Callback;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!IsAnimating) return;

            if (CurrentAnimationTime >= AnimationTime)
            {
                IsAnimating = false;
                foreach(UI_ScoreToken token in Tokens) GameObject.Destroy(token.gameObject);
                Tokens.Clear();
                if (Callback != null) Callback();
            }
            else
            {
                float ratio = CurrentAnimationTime / AnimationTime;
                foreach (UI_ScoreToken token in Tokens) token.transform.position = Vector2.Lerp(token.StartPosition, token.TargetPosition, ratio);

                CurrentAnimationTime += Time.deltaTime * AnimationSpeedModifier;
            }
        }

        public void AddTokenToNextAnimation(District district, Party party, int value)
        {
            if (IsAnimating) throw new Exception("Can't add new tokens mid-animation");

            UI_ScoreToken token = GameObject.Instantiate(ScoreTokenPrefab, TokenContainer.transform);
            token.StartPosition = district.MapLabel.SeatsText.transform.position;
            token.TargetPosition = UI_ElectionTactics.Instance.StandingsPanel.GetElementCenter(party);
            token.ValueText.text = value.ToString();
            token.Background.color = value > 0 ? ColorManager.Instance.HighImpactColor : ColorManager.Instance.NegativeImpactColor;
            token.gameObject.SetActive(false);

            Tokens.Add(token);
        }

        public void StartAnimation(float duration, Action callback = null)
        {
            if (Tokens.Count == 0) throw new System.Exception("Can't start animation when there are no tokens.");

            Callback = callback;
            IsAnimating = true;
            CurrentAnimationTime = 0f;
            AnimationTime = duration;

            foreach (UI_ScoreToken token in Tokens)
            {
                token.gameObject.SetActive(true);
                token.transform.position = token.StartPosition;
            }
        }

        public void SetAnimationSpeedModifier(float speed)
        {
            AnimationSpeedModifier = speed;
        }
    }
}
