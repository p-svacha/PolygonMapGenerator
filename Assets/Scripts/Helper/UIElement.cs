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
    private float SlideTime;
    private float SlideDelay;

    void Update()
    {
        UpdateSlide();
    }

    private void UpdateSlide()
    {
        if (IsSliding)
        {
            if (SlideDelay >= SlideTime)
            {
                IsSliding = false;
                GetComponent<RectTransform>().anchoredPosition = TargetPos;
            }
            else
            {
                float r = SlideDelay / SlideTime;
                GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(SourcePos, TargetPos, r);
                SlideDelay += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Starts sliding the element to a position over time.
    /// </summary>
    public void Slide(Vector2 targetPosition, float time)
    {
        SlideTime = time;
        SlideDelay = 0f;
        IsSliding = true;
        SourcePos = GetComponent<RectTransform>().anchoredPosition;
        TargetPos = targetPosition;
    }
}
