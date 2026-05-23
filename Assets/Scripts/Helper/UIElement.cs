using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class that can be inherited from. Contains some useful functions for UI Elements.
/// </summary>
public class UIElement : MonoBehaviour
{
    // Slide Animation
    private bool IsSliding;
    private Vector2 SourcePos;
    private Vector2 TargetPos;
    private Vector2 SourceOffsetMin;
    private Vector2 SourceOffsetMax;
    private Vector2 TargetOffsetMin;
    private Vector2 TargetOffsetMax;
    private bool IsStretched;
    private float SlideTime;
    private float SlideDelay;

    protected virtual void Update()
    {
        UpdateSlide();
    }

    private RectTransform Rect => GetComponent<RectTransform>();

    private void UpdateSlide()
    {
        if (!IsSliding) return;

        if (SlideDelay >= SlideTime)
        {
            IsSliding = false;
            if (IsStretched)
            {
                Rect.offsetMin = TargetOffsetMin;
                Rect.offsetMax = TargetOffsetMax;
            }
            else
            {
                Rect.anchoredPosition = TargetPos;
            }
        }
        else
        {
            float r = SlideDelay / SlideTime;
            if (IsStretched)
            {
                Rect.offsetMin = Vector2.Lerp(SourceOffsetMin, TargetOffsetMin, r);
                Rect.offsetMax = Vector2.Lerp(SourceOffsetMax, TargetOffsetMax, r);
            }
            else
            {
                Rect.anchoredPosition = Vector2.Lerp(SourcePos, TargetPos, r);
            }
            SlideDelay += Time.deltaTime;
        }
    }

    public void Slide(Vector2 targetPosition, float time)
    {
        SlideTime = time;
        SlideDelay = 0f;
        IsSliding = true;

        RectTransform rt = Rect;
        IsStretched = (rt.anchorMin != rt.anchorMax);

        if (IsStretched)
        {
            SourceOffsetMin = rt.offsetMin;
            SourceOffsetMax = rt.offsetMax;
            Vector2 delta = targetPosition - rt.anchoredPosition;
            TargetOffsetMin = SourceOffsetMin + delta;
            TargetOffsetMax = SourceOffsetMax + delta;
        }
        else
        {
            SourcePos = rt.anchoredPosition;
            TargetPos = targetPosition;
        }
    }
}