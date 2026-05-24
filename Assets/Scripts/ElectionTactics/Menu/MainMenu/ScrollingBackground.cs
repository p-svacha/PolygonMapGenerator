using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 20f; // pixels per second

    private RectTransform rect;
    private float imageWidth;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        imageWidth = rect.sizeDelta.x;
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        if (rect.anchoredPosition.x <= -imageWidth)
        {
            rect.anchoredPosition += new Vector2(imageWidth * 2f, 0f);
        }
    }
}