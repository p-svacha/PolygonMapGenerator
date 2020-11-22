using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Care. The prefab with this script has to be placed in a container of the same size with anchors [0,1],[0,1].
/// </summary>
public class WindowGraph : MonoBehaviour
{
    public bool Initialized;
    public float GraphWidth;
    public float GraphHeight;

    public RectTransform GraphContainer;
    public Sprite CircleSprite;
    public Font font;

    void Start()
    {
        Initialized = false;
    }

    void Update()
    {
        if(!Initialized)
        {
            GraphWidth = GraphContainer.rect.width;
            GraphHeight = GraphContainer.rect.height;
        }
        //if (Input.GetKeyDown(KeyCode.Space)) ShowRandomGraph();
    }

    private void ShowRandomGraph()
    {
        int n = Random.Range(3, 9);
        int maxValue = Random.Range(4, 13) * 10;
        float spacing = Random.Range(1, 4);
        int step = Random.Range(1, 3) * 10;
        List<GraphDataPoint> testList = new List<GraphDataPoint>();
        for(int i = 0; i < n; i++)
        {
            testList.Add(new GraphDataPoint("P" + i, Random.Range(0, maxValue), new Color(Random.value, Random.value, Random.value)));
        }
        ShowBarGraph(testList, maxValue, step, spacing * 0.1f, Color.white, Color.grey, font);
    }

    private void CreateCircle(Vector2 anchoredPos, float size)
    {
        GameObject circleObject = new GameObject("circle", typeof(Image));
        circleObject.transform.SetParent(GraphContainer, false);
        circleObject.GetComponent<Image>().sprite = CircleSprite;
        RectTransform rect = circleObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(size, size);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    private void DrawRectangle(Vector2 centerPos, Vector2 dimensions, Color color)
    {
        GameObject obj = new GameObject("rect", typeof(Image));
        obj.transform.SetParent(GraphContainer, false);
        obj.GetComponent<Image>().color = color;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = centerPos;
        rect.sizeDelta = dimensions;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
    }

    private void DrawText(string text, Vector2 centerPos, Vector2 dimensions, Color c, Font font, int size)
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
    }

    private void CreateBar(float x, float width, float height, Color c)
    {
        Vector2 position = new Vector2(x, height / 2);
        Vector2 dimensions = new Vector2(width, height);
        DrawRectangle(position, dimensions, c);
    }

    
    public void ShowBarGraph(List<GraphDataPoint> dataPoints, float yMax, float yStep, float barSpacing, Color axisColor, Color axisStepColor, Font font)
    {
        foreach (Transform t in GraphContainer) Destroy(t.gameObject);

        float xStep = GraphWidth / (dataPoints.Count + 1);
        float barWidth = xStep * (1 - barSpacing);
        float axisWidth = Mathf.Min(GraphWidth, GraphHeight) * 0.01f;
        int fontSize = (int)(GraphHeight * 0.07f);
        float yAxisTopMargin = GraphHeight * 0.05f;

        // Axis origin
        DrawRectangle(new Vector2(-axisWidth / 2, -axisWidth / 2), new Vector2(axisWidth, axisWidth), axisColor);

        // X-axis
        DrawRectangle(new Vector2(GraphWidth / 2, -axisWidth / 2), new Vector2(GraphWidth, axisWidth), axisColor);

        // Y-axis
        DrawRectangle(new Vector2(-axisWidth / 2, GraphHeight / 2), new Vector2(axisWidth, GraphHeight), axisColor);
        DrawRectangle(new Vector2(GraphWidth / 2, GraphHeight - yAxisTopMargin), new Vector2(GraphWidth, axisWidth), axisStepColor);
        DrawText(yMax.ToString(), new Vector2(-fontSize, GraphHeight - yAxisTopMargin), new Vector2(4 * fontSize, fontSize), axisColor, font, fontSize);
        int yAxisSteps = (int)(yMax/yStep);
        if (yMax % yStep == 0) yAxisSteps--;
        for (int i = 0; i < yAxisSteps; i++)
        {
            float yStepValue = (i + 1) * yStep;
            float y = (yStepValue / yMax) * (GraphHeight - yAxisTopMargin);
            DrawRectangle(new Vector2(GraphWidth / 2, y), new Vector2(GraphWidth, axisWidth), axisStepColor);
            string label = yStepValue.ToString();
            DrawText(label, new Vector2(-fontSize, y), new Vector2(4 * fontSize, fontSize), axisColor, font, fontSize);
        }

        // Bars and bar labels
        for (int i = 0; i < dataPoints.Count; i++)
        {
            float xPos = (i + 1) * xStep + 1;
            float height = (dataPoints[i].Value / yMax) * (GraphHeight - yAxisTopMargin);
            CreateBar(xPos, barWidth, height, dataPoints[i].Color); // Bars
            DrawText(dataPoints[i].Label, new Vector2(xPos, -fontSize), new Vector2(barWidth, fontSize), dataPoints[i].Color, font, fontSize); // X-axis labels
            DrawText(dataPoints[i].Value.ToString("0.0"), new Vector2(xPos, height + fontSize), new Vector2(barWidth, 4 * fontSize), dataPoints[i].Color, font, fontSize); // Bar value labels
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
            float circleSize = Mathf.Min(GraphWidth, GraphHeight) * 0.05f;
            CreateCircle(new Vector2(xPos, yPos), circleSize);
        }
    }
}
