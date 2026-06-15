using System;
using System.Collections.Generic;
using System.Linq;
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
        private float TokenTravelTime;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!IsAnimating) return;

            if (Tokens.Count == 0)
            {
                StopAnimation();
                if (Callback != null) Callback();
            }
            else
            {
                foreach (UI_ScoreToken token in Tokens)
                {
                    float tokenTime = CurrentAnimationTime - token.StartDelay;
                    if (tokenTime < 0f)
                    {
                        token.transform.position = token.StartPosition;
                        continue;
                    }

                    if (!token.gameObject.activeSelf)
                    {
                        token.gameObject.SetActive(true);
                        AudioManager.PlaySound(AudioManager.Instance.Swoosh, volume: 0.2f, applySpeedModifier: true);
                    }

                    float ratio = Mathf.Clamp01(tokenTime / TokenTravelTime);
                    token.transform.position = Vector2.Lerp(token.StartPosition, token.TargetPosition, ratio);
                    if (!token.HasArrived && ratio >= 1f)
                    {
                        token.HasArrived = true;
                        token.ArrivalCallback?.Invoke();
                        GameObject.Destroy(token.gameObject);
                    }
                }

                Tokens = Tokens.Where(t => !t.HasArrived).ToList(); // Remove arrived tokens
                CurrentAnimationTime += Time.deltaTime * AnimationSpeedModifier;

            }
        }

        public void AddTokenToNextAnimation(Vector2 startPosition, Party party, int value)
        {
            if (IsAnimating) StopAnimation();

            UI_ScoreToken token = GameObject.Instantiate(ScoreTokenPrefab, TokenContainer.transform);
            token.StartPosition = startPosition;
            token.TargetPosition = UI_ElectionTactics.Instance.StandingsPanel.GetElementCenter(party);
            token.ValueText.text = value.ToString();
            token.Background.color = value > 0 ? ColorManager.Instance.HighImpactColor : ColorManager.Instance.NegativeImpactColor;
            token.gameObject.SetActive(false);

            Tokens.Add(token);
        }

        public void AddSeatTokenToNextAnimation(Vector2 startPosition, Vector2 targetPosition, float startDelay, Action arrivalCallback = null)
        {
            if (IsAnimating) StopAnimation();
            UI_ScoreToken token = Instantiate(ScoreTokenPrefab, TokenContainer.transform);
            token.gameObject.SetActive(false);
            token.StartPosition = startPosition;
            token.TargetPosition = targetPosition;
            token.ValueText.text = "";
            token.Background.color = Color.white;
            token.StartDelay = startDelay;
            token.ArrivalCallback = arrivalCallback;
            Tokens.Add(token);
        }

        public void StartAnimation(float duration, Action callback = null)
        {
            if (Tokens.Count == 0) throw new System.Exception("Can't start animation when there are no tokens.");

            Callback = callback;
            IsAnimating = true;
            CurrentAnimationTime = 0f;
            TokenTravelTime = duration;

            float maxStartDelay = Tokens.Max(t => t.StartDelay);
            AnimationTime = duration + maxStartDelay;

            foreach (UI_ScoreToken token in Tokens)
            {
                token.transform.position = token.StartPosition;
            }
        }

        /// <summary>
        /// Stops the current animation without calling the callback.
        /// </summary>
        private void StopAnimation()
        {
            IsAnimating = false;
            foreach (UI_ScoreToken token in Tokens) GameObject.Destroy(token.gameObject);
            Tokens.Clear();
        }

        public void SetAnimationSpeedModifier(float speed)
        {
            AnimationSpeedModifier = speed;
        }
    }
}
