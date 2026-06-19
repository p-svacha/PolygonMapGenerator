using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for chart components that draw primitives into a UI container.
/// The prefab with a derived script must be placed in a container of the same size with anchors [0,1],[0,1].
/// </summary>
public abstract class GraphBase : MonoBehaviour
{
    public RectTransform GraphContainer;
    public Sprite CircleSprite;
    public Font Font;

    protected float GraphWidth;
    protected float GraphHeight;

    /// <summary>
    /// Reads the current container dimensions into GraphWidth/GraphHeight. Call before laying out.
    /// </summary>
    protected void MeasureContainer()
    {
        GraphWidth = GraphContainer.rect.width;
        GraphHeight = GraphContainer.rect.height;
    }

    /// <summary>
    /// Destroys all children of the container.
    /// </summary>
    protected void ClearContainer()
    {
        foreach (Transform t in GraphContainer) Destroy(t.gameObject);
    }

    #region Primitives

    protected Image CreateCircle(Vector2 anchoredPos, float size, Color color)
    {
        GameObject obj = new GameObject("circle", typeof(Image));
        obj.transform.SetParent(GraphContainer, false);
        Image img = obj.GetComponent<Image>();
        img.sprite = CircleSprite;
        img.color = color;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(size, size);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        return img;
    }

    protected GameObject DrawRectangle(Vector2 centerPos, Vector2 dimensions, Color color)
    {
        GameObject obj = new GameObject("rect", typeof(Image));
        obj.transform.SetParent(GraphContainer, false);
        obj.GetComponent<Image>().color = color;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = centerPos;
        rect.sizeDelta = dimensions;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        return obj;
    }

    protected Text DrawText(string text, Vector2 centerPos, Vector2 dimensions, Color c, Font font, int size)
    {
        GameObject obj = new GameObject(text, typeof(Text));
        obj.transform.SetParent(GraphContainer, false);
        Text textObj = obj.GetComponent<Text>();
        textObj.text = text;
        textObj.color = c;
        textObj.font = font;
        textObj.fontSize = size;
        textObj.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = centerPos;
        rect.sizeDelta = dimensions;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        return textObj;
    }

    protected Image DrawImage(Sprite sprite, Vector2 centerPos, Vector2 dimensions, string tooltipTitle = "", string tooltipText = "")
    {
        GameObject obj = new GameObject("Icon", typeof(Image));
        obj.transform.SetParent(GraphContainer, false);
        Image imgObj = obj.GetComponent<Image>();
        imgObj.sprite = sprite;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = centerPos;
        rect.sizeDelta = dimensions;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);

        if (tooltipTitle != "")
        {
            TooltipTarget tooltipTarget = obj.AddComponent<TooltipTarget>();
            tooltipTarget.Title = tooltipTitle;
            tooltipTarget.Text = tooltipText;
        }
        return imgObj;
    }

    #endregion
}
