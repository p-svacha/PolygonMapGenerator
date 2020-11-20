using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour
{
    public float GraphWidth;
    public float GraphHeight;

    public RectTransform GraphContainer;
    public Sprite CircleSprite;

    void Start()
    {
        GraphWidth = GraphContainer.rect.width;
        GraphHeight = GraphContainer.rect.height;

        ShowBarGraph(new List<float>() { 40.6f, 38.5f, 15.4f, 6.4f, 2.5f }, new List<Color>() { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan}, 100, 0.1f);
    }

    void Update()
    {
        
    }

    private void CreateCircle(Vector2 anchoredPos)
    {
        GameObject circleObject = new GameObject("circle", typeof(Image));
        circleObject.transform.SetParent(GraphContainer, false);
        circleObject.GetComponent<Image>().sprite = CircleSprite;
        RectTransform rect = circleObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(60, 60);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    private void CreateBar(float x, float width, float height, Color c)
    {
        GameObject barObject = new GameObject("bar", typeof(Image));
        barObject.transform.SetParent(GraphContainer, false);
        barObject.GetComponent<Image>().color = c;
        RectTransform rect = barObject.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, height/2);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    private void ShowBarGraph(List<float> valueList, List<Color> colors, float yMax, float barSpacing)
    {
        float xStep = GraphWidth / (valueList.Count + 1);
        float width = xStep * (1 - barSpacing);
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPos = (i + 1) * xStep + 1;
            float height = (valueList[i] / yMax) * GraphHeight;
            CreateBar(xPos, width, height, colors[i]);
        }
    }

    private void ShowLineGraph(List<float> valueList)
    {
        float yMaximum = 100f;
        float xStep = GraphWidth / (valueList.Count+1);
        for(int i = 0; i < valueList.Count; i++)
        {
            float xPos = (i + 1) * xStep + 1;
            float yPos = (valueList[i] / yMaximum) * GraphHeight;
            CreateCircle(new Vector2(xPos, yPos));
        }
    }
}
