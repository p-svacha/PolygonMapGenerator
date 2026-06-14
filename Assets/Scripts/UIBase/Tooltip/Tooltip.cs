using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Tooltip : MonoBehaviour
{
    // Singleton
    public static Tooltip Instance;
    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    [Header("Elements")]
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Text;

    public float Width;
    public float Height;

    private const int MAX_WIDTH = 300; // px
    private const int MOUSE_OFFSET = 5; // px
    private const int SCREEN_EDGE_OFFSET = 5; // px

    public void Init(TooltipType type, string title, string text, Color? titleColor = null)
    {
        // Show/hide title text
        Title.gameObject.SetActive(type == TooltipType.TitleAndText);
        Title.text = title;
        Text.text = text;
        if (titleColor != null) Title.color = (Color)titleColor;
        else Title.color = new Color(0.05f, 0.89f, 0.53f);

        RectTransform rect = GetComponent<RectTransform>();

        // 1) Force a max width initially
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MAX_WIDTH);

        // 2) Update the TMP text so it wraps if necessary
        Title.ForceMeshUpdate();
        Text.ForceMeshUpdate();

        // 3) Measure the actual space needed *after* wrapping
        float neededWidth = Mathf.Max(Title.preferredWidth + 10, Text.preferredWidth + 10);

        // 4) Clamp the width if the content is smaller than max width
        float finalWidth = Mathf.Min(neededWidth, MAX_WIDTH);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);

        // Initial placement
        rect.ForceUpdateRectTransforms();
        RepositionTooltip();

        // Show it
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // If the tooltip is active, keep it following the mouse
        if (gameObject.activeSelf)
        {
            RepositionTooltip();
        }
    }

    /// <summary>
    /// Reposition the tooltip near the mouse, clamping to screen edges.
    /// </summary>
    private void RepositionTooltip()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Convert mouse position to canvas local space
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        // Offset from cursor
        localPoint += new Vector2(MOUSE_OFFSET, MOUSE_OFFSET);

        Width = rect.rect.width;
        Height = rect.rect.height;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Clamp within canvas bounds (canvas local space is centered at 0,0)
        float halfW = canvasWidth / 2f;
        float halfH = canvasHeight / 2f;

        if (localPoint.x + Width > halfW - SCREEN_EDGE_OFFSET)
            localPoint.x = halfW - Width - SCREEN_EDGE_OFFSET;
        if (localPoint.x < -halfW + SCREEN_EDGE_OFFSET)
            localPoint.x = -halfW + SCREEN_EDGE_OFFSET;
        if (localPoint.y + Height > halfH - SCREEN_EDGE_OFFSET)
            localPoint.y = halfH - Height - SCREEN_EDGE_OFFSET;
        if (localPoint.y < -halfH + SCREEN_EDGE_OFFSET)
            localPoint.y = -halfH + SCREEN_EDGE_OFFSET;

        rect.anchoredPosition = localPoint;
    }


    public enum TooltipType
    {
        TitleAndText,
        TextOnly
    }
}

