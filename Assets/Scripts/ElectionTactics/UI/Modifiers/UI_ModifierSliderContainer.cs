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

        void Update()
        {
            if (IsAnimating) {
                if (AnimationOffset >= AnimationTime)
                {
                    IsAnimating = false;
                    AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(EndWidth, Height);
                    AnimatedSlider.DescriptionText.gameObject.SetActive(true);
                }
                else
                {
                    float ratio = AnimationOffset / AnimationTime;
                    float width = EndWidth * ratio;
                    AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(width, Height);

                    AnimationOffset += Time.deltaTime;
                }
            }
        }

        public void SlideInModifier(Modifier m, float slideTime)
        {
            AnimatedSlider = Instantiate(ModifierSliderPrefab, transform);
            AnimatedSlider.Init(m);
            AnimatedSlider.DescriptionText.gameObject.SetActive(false);

            IsAnimating = true;
            AnimationTime = slideTime;
            AnimationOffset = 0f;
            EndWidth = GetComponent<RectTransform>().rect.width;
            Height = AnimatedSlider.GetComponent<RectTransform>().sizeDelta.y;

            AnimatedSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(0, Height);
        }

        public void ClearContainer()
        {
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        }
    }
}
