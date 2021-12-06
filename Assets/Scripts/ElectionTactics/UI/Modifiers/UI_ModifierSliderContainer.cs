using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_ModifierSliderContainer : MonoBehaviour
    {
        public UI_ModifierSlider ModifierSliderPrefab;

        // Slide animation
        private UI_ModifierSlider AnimatedSlider;
        private bool IsAnimating;
        private float AnimationTime;
        private float AnimationOffset;
        public float EndWidth;
        public float Height;
        private float AnimationSpeedModifier;
        private Action AnimationCallback;

        void Update()
        {
            if (IsAnimating) {
                if (AnimationOffset >= AnimationTime)
                {
                    IsAnimating = false;
                    AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(EndWidth, Height);
                    AnimatedSlider.DescriptionText.gameObject.SetActive(true);
                    if (AnimationCallback != null) AnimationCallback();
                }
                else
                {
                    float ratio = AnimationOffset / AnimationTime;
                    float width = EndWidth * ratio;
                    AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(width, Height);

                    AnimationOffset += Time.deltaTime * AnimationSpeedModifier;
                }
            }
        }

        public void SetAnimationSpeedModifier(float speed)
        {
            AnimationSpeedModifier = speed;
        }

        /// <summary>
        /// Slides in a modifier in the correct position. Callback gets executed when the animation is done.
        /// </summary>
        public void SlideInModifier(Modifier m, float slideTime, Action callback = null)
        {
            AnimatedSlider = Instantiate(ModifierSliderPrefab, transform);
            AnimatedSlider.Init(m);
            AnimatedSlider.DescriptionText.gameObject.SetActive(false);

            IsAnimating = true;
            AnimationTime = slideTime;
            AnimationOffset = 0f;
            AnimationCallback = callback;
            EndWidth = GetComponent<RectTransform>().rect.width;
            Height = AnimatedSlider.GetComponent<RectTransform>().sizeDelta.y;

            AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(0, Height);
        }

        public void ClearContainer()
        {
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
            IsAnimating = false;
        }
    }
}
