using ElectionTactics;
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
    private GraphAnimationType AnimationType;
    private float AnimationTime;
    private float AnimationDelay;
    private float AnimationSpeedModifier = 1f;
    private System.Action AnimationCallback;

    private List<GraphDataPoint> SourceDataPoints;
    private List<GraphDataPoint> TargetDataPoints;
    private float SourceYMax;
    private float TargetYMax;

    void Start()
    {
    }

    void Update()
    {
        if(AnimationType != GraphAnimationType.None)
        {
            if (AnimationDelay >= AnimationTime) // Animation is done
            {
                if(AnimationType == GraphAnimationType.Update)
                {
                    YMax = TargetYMax;
                    DataPoints = TargetDataPoints;
                }

                ShowBarGraph(DataPoints, YMax, YStep, BarSpacing, AxisColor, AxisStepColor, Font);
                AnimationType = GraphAnimationType.None;
                if (AnimationCallback != null) AnimationCallback();
            }
            else // Animation is ongoing
            {
                float r = AnimationDelay / AnimationTime;

                switch (AnimationType)
                {
                    case GraphAnimationType.Init:
                        float curValue = MaxValue * r;
                        float curHeight = MaxBarHeight * r;

                        for (int i = 0; i < DataPoints.Count; i++)
                        {
                            float barX = (i + 1) * XStep;
                            float barValue, barHeight;
                            if (DataPoints[i].Value < curValue)
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
                        break;

                    case GraphAnimationType.Update:
                        List<GraphDataPoint> tmpDataPoints = new List<GraphDataPoint>();
                        float tmpYMax = 0;
                        for(int i = 0; i < TargetDataPoints.Count; i++)
                        {
                            // Check if the same data point exists in both source and target
                            GraphDataPoint matchingSourceDataPoint = SourceDataPoints.FirstOrDefault(x => x.Label == TargetDataPoints[i].Label);
 
                            // Lerp value
                            float value;
                            if (matchingSourceDataPoint == null) value = TargetDataPoints[i].Value; // Show final value immediately if data point wasn't present before update
                            else value = matchingSourceDataPoint.Value + (TargetDataPoints[i].Value - matchingSourceDataPoint.Value) * r;
                            GraphDataPoint tmpDataPoint = new GraphDataPoint(TargetDataPoints[i].Label, value, TargetDataPoints[i].Color, TargetDataPoints[i].Icons, TargetDataPoints[i].IconTooltipTitles, TargetDataPoints[i].IconTooltipTexts);
                            tmpDataPoints.Add(tmpDataPoint);

                            tmpYMax = SourceYMax + (TargetYMax - SourceYMax) * r;
                        }
                        ShowBarGraph(tmpDataPoints, tmpYMax, YStep, BarSpacing, AxisColor, AxisStepColor, Font, stopAnimation: false);
                        break;
                }

                AnimationDelay += Time.deltaTime * AnimationSpeedModifier;
            }
        }
    }

    #region Public methods

    /// <summary>
    /// Instantly destroys the whole graph
    /// </summary>
    public void ClearGraph(bool stopAnimation = true)
    {
        foreach (Transform t in GraphContainer) Destroy(t.gameObject);
        Bars.Clear();
        BarLabels.Clear();
        if(stopAnimation) AnimationType = GraphAnimationType.None;
    }

    /// <summary>
    /// Instantly displays a bar graph with the given attributes
    /// </summary>
    public void ShowBarGraph(List<GraphDataPoint> dataPoints, float yMax, float yStep, float barSpacing, Color axisColor, Color axisStepColor, Font font, bool stopAnimation = true, bool zeroed = false)
    {
        GraphWidth = GraphContainer.rect.width;
        GraphHeight = GraphContainer.rect.height;

        ClearGraph(stopAnimation);
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
            if (zeroed) height = 0;
            Bars.Add(CreateBar(xPos, BarWidth, height, dataPoints[i].Color)); // Bars
            BarLabels.Add(DrawText(zeroed ? "" : dataPoints[i].Value.ToString("0.0") + "%", new Vector2(xPos, height + FontSize), new Vector2(BarWidth, FontSize), dataPoints[i].Color, font, FontSize)); // Bar value labels
        }
    }
    
    /// <summary>
    /// Displays an empty bar graph and initializes an animation that can either be instantly started with startAnimation = true or by calling StartInitAnimation()
    /// </summary>
    public void InitAnimatedBarGraph(List<GraphDataPoint> dataPoints, float yMax, float yStep, float barSpacing, Color axisColor, Color axisStepColor, Font font, float animationTime, bool startAnimation)
    {
        ShowBarGraph(dataPoints, yMax, yStep, barSpacing, axisColor, axisStepColor, font, zeroed: true);
        MaxValue = dataPoints.Max(x => x.Value);
        MaxBarHeight = (MaxValue / yMax) * (GraphHeight - YMarginTop);
        AnimationTime = animationTime;
        AnimationDelay = 0f;
        if (startAnimation) StartAnimation();
    }

    /// <summary>
    /// Starts the animation that has been previously initialized with InitAnimatedGraph(). Callback gets executed when the animation is done.
    /// </summary>
    public void StartAnimation(System.Action callback = null)
    {
        AnimationType = GraphAnimationType.Init;
        AnimationCallback = callback;
    }

    /// <summary>
    /// Updates the values of an already initialized graph with an animation
    /// </summary>
    public void UpdateAnimatedBarGraph(List<GraphDataPoint> dataPoints, float yMax, float animationTime)
    {
        SourceDataPoints = DataPoints;
        TargetDataPoints = dataPoints;
        SourceYMax = YMax;
        TargetYMax = yMax;
        AnimationTime = animationTime;
        AnimationDelay = 0f;
        AnimationType = GraphAnimationType.Update;
    }

    /// <summary>
    /// Modifies the speed of the animation.
    /// </summary>
    public void SetAnimationSpeedModifier(float speed)
    {
        AnimationSpeedModifier = speed;
    }

    #endregion

    #region GraphElements

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

    private Image DrawImage(Sprite sprite, Vector2 centerPos, Vector2 dimensions, string tooltipTitle = "", string tooltipText = "")
    {
        GameObject obj = new GameObject("ModifierIcon", typeof(Image));
        obj.transform.SetParent(GraphContainer, false);
        Image imgObj = obj.GetComponent<Image>();
        imgObj.sprite = sprite;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = centerPos;
        rect.sizeDelta = dimensions;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);

        // Tooltip
        if(tooltipTitle != "")
        {
            TooltipTarget tooltipTarget = obj.AddComponent<TooltipTarget>();
            tooltipTarget.Title = tooltipTitle;
            tooltipTarget.Text = tooltipText;
        }

        return imgObj;
    }

    private GameObject CreateBar(float x, float width, float height, Color c)
    {
        Vector2 position = new Vector2(x, height / 2);
        Vector2 dimensions = new Vector2(width, height);
        return DrawRectangle(position, dimensions, c);
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
            for(int j = 0; j < DataPoints[i].Icons.Count; j++) // X-axis label icons
            {
                Vector2 dimensions = new Vector2(20, 20);
                float iconXStep = 25;
                float iconXStart = xPos - (DataPoints[i].Icons.Count - 1) * iconXStep * 0.5f;
                float iconX = iconXStart + j * iconXStep;
                float iconY = -FontSize * 2;
                DrawImage(DataPoints[i].Icons[j], new Vector2(iconX, iconY), dimensions, DataPoints[i].IconTooltipTitles[j], DataPoints[i].IconTooltipTexts[j]);
            }
        }

        // Y-axis
        DrawRectangle(new Vector2(-AxisWidth / 2, GraphHeight / 2), new Vector2(AxisWidth, GraphHeight), AxisColor);
        DrawRectangle(new Vector2(GraphWidth / 2, GraphHeight - YMarginTop), new Vector2(GraphWidth, AxisWidth), AxisStepColor);
        DrawText(((int)YMax).ToString(), new Vector2(-FontSize, GraphHeight - YMarginTop), new Vector2(4 * FontSize, FontSize), AxisColor, Font, FontSize);
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

    #endregion

    #region Test

    private void ShowRandomGraph()
    {
        int n = Random.Range(3, 9);
        int maxValue = Random.Range(4, 13) * 10;
        float spacing = Random.Range(1, 4);
        int step = Random.Range(1, 3) * 10;
        List<GraphDataPoint> testList = new List<GraphDataPoint>();
        for (int i = 0; i < n; i++)
        {
            testList.Add(new GraphDataPoint("P" + i, Random.Range(0, maxValue), new Color(Random.value, Random.value, Random.value)));
        }
        ShowBarGraph(testList, maxValue, step, spacing * 0.1f, Color.white, Color.grey, Font);
    }

    #endregion
}
