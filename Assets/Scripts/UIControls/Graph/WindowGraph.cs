using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Current graph attributes
    private List<GraphDataPoint> DataPoints;
    public Font Font;
    private float XStep;
    private float BarSpacing;
    private float BarWidth;
    private int FontSize;
    private float YMax;
    private float YStep;
    private float YMarginTop;
    private float AxisWidth;
    private Color AxisColor;
    private Color AxisStepColor;

    // Animation
    private List<GameObject> Bars = new List<GameObject>();
    private List<Text> BarLabels = new List<Text>();
    private float MaxValue;
    private float MaxBarHeight;
    private bool IsAnimating;
    private float AnimationTime;
    private float AnimationDelay;

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
        
        if(IsAnimating && AnimationDelay >= AnimationTime)
        {
            ShowBarGraph(DataPoints, YMax, YStep, BarSpacing, AxisColor, AxisStepColor, Font);
            IsAnimating = false;
        }
        else if(IsAnimating)
        {
            float r = AnimationDelay / AnimationTime;
            float curValue = MaxValue * r;
            float curHeight = MaxBarHeight * r;

            for(int i = 0; i < DataPoints.Count; i++)
            {
                float barX = (i + 1) * XStep;
                float barValue, barHeight;
                if(DataPoints[i].Value < curValue)
                {
                    barValue = DataPoints[i].Value;
                    barHeight = (barValue / YMax) * (GraphHeight - YMarginTop);
                }
                else
                {
                    barValue = curValue;
                    barHeight = curHeight;
                }
                Vector2 pos = new Vector2(barX, barHeight / 2);
                Vector2 size = new Vector2(BarWidth, barHeight);
                RectTransform rect = Bars[i].GetComponent<RectTransform>();
                rect.anchoredPosition = pos;
                rect.sizeDelta = size;

                float barLabelY = barHeight + FontSize;
                BarLabels[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(barX, barLabelY);
                BarLabels[i].text = barValue.ToString("0.0") + "%";
            }
            AnimationDelay += Time.deltaTime;
        }
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
        ShowBarGraph(testList, maxValue, step, spacing * 0.1f, Color.white, Color.grey, Font);
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

    private GameObject DrawRectangle(Vector2 centerPos, Vector2 dimensions, Color color)
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

    private Text DrawText(string text, Vector2 centerPos, Vector2 dimensions, Color c, Font font, int size)
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

    private GameObject CreateBar(float x, float width, float height, Color c)
    {
        Vector2 position = new Vector2(x, height / 2);
        Vector2 dimensions = new Vector2(width, height);
        return DrawRectangle(position, dimensions, c);
    }

    public void ClearGraph()
    {
        foreach (Transform t in GraphContainer) Destroy(t.gameObject);
        Bars.Clear();
        BarLabels.Clear();
        IsAnimating = false;
    }
    public void ShowBarGraph(List<GraphDataPoint> dataPoints, float yMax, float yStep, float barSpacing, Color axisColor, Color axisStepColor, Font font, bool animation = false)
    {
        ClearGraph();
        DataPoints = dataPoints;

        Font = font;
        XStep = GraphWidth / (dataPoints.Count + 1);
        BarSpacing = barSpacing;
        BarWidth = XStep * (1 - barSpacing);
        YMax = yMax;
        YStep = yStep;
        AxisWidth = Mathf.Min(GraphWidth, GraphHeight) * 0.01f;
        FontSize = (int)(GraphHeight * 0.07f);
        YMarginTop = GraphHeight * 0.05f;
        AxisColor = axisColor;
        AxisStepColor = axisStepColor;

        DrawAxis();

        // Bars and bar labels
        for (int i = 0; i < dataPoints.Count; i++)
        {
            float xPos = (i + 1) * XStep;
            float height = (dataPoints[i].Value / yMax) * (GraphHeight - YMarginTop);
            if (animation) height = 0;
            Bars.Add(CreateBar(xPos, BarWidth, height, dataPoints[i].Color)); // Bars
            BarLabels.Add(DrawText(dataPoints[i].Value.ToString("0.0") + "%", new Vector2(xPos, height + FontSize), new Vector2(BarWidth, FontSize), dataPoints[i].Color, font, FontSize)); // Bar value labels
        }
    }

    public void ShowAnimatedBarGraph(List<GraphDataPoint> dataPoints, float yMax, float yStep, float barSpacing, Color axisColor, Color axisStepColor, Font font, float animationTime)
    {
        ShowBarGraph(dataPoints, yMax, yStep, barSpacing, axisColor, axisStepColor, font, animation: true);
        MaxValue = dataPoints.Max(x => x.Value);
        MaxBarHeight = (MaxValue / yMax) * (GraphHeight - YMarginTop);
        AnimationTime = animationTime;
        AnimationDelay = 0f;
        IsAnimating = true;
    }

    private void DrawAxis()
    {
        // Axis origin
        DrawRectangle(new Vector2(-AxisWidth / 2, -AxisWidth / 2), new Vector2(AxisWidth, AxisWidth), AxisColor);

        // X-axis
        DrawRectangle(new Vector2(GraphWidth / 2, -AxisWidth / 2), new Vector2(GraphWidth, AxisWidth), AxisColor);
        for (int i = 0; i < DataPoints.Count; i++)
        {
            float xPos = (i + 1) * XStep + 1;
            DrawText(DataPoints[i].Label, new Vector2(xPos, -FontSize), new Vector2(BarWidth, FontSize), DataPoints[i].Color, Font, FontSize); // X-axis labels
        }

        // Y-axis
        DrawRectangle(new Vector2(-AxisWidth / 2, GraphHeight / 2), new Vector2(AxisWidth, GraphHeight), AxisColor);
        DrawRectangle(new Vector2(GraphWidth / 2, GraphHeight - YMarginTop), new Vector2(GraphWidth, AxisWidth), AxisStepColor);
        DrawText(YMax.ToString(), new Vector2(-FontSize, GraphHeight - YMarginTop), new Vector2(4 * FontSize, FontSize), AxisColor, Font, FontSize);
        int yAxisSteps = (int)(YMax / YStep);
        if (YMax % YStep == 0) yAxisSteps--;
        for (int i = 0; i < yAxisSteps; i++)
        {
            float yStepValue = (i + 1) * YStep;
            float y = (yStepValue / YMax) * (GraphHeight - YMarginTop);
            DrawRectangle(new Vector2(GraphWidth / 2, y), new Vector2(GraphWidth, AxisWidth), AxisStepColor);
            string label = yStepValue.ToString();
            DrawText(label, new Vector2(-FontSize, y), new Vector2(4 * FontSize, FontSize), AxisColor, Font, FontSize);
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
