using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    private Dictionary<string, int> MapSizes = new Dictionary<string, int>()
    {
        {"Tiny", 10 },
        {"Small", 15 },
        {"Standard", 20 },
        {"Big", 25 },
        {"Huge", 30 },
    };

    public GameObject ContentPanel;
    public GameObject LoadingScreen;
    public Button MenuButton;
    public PolygonMapGenerator PMG;
    public CanvasGroup CanvasGroup;

    void Start()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        Show();
        float y = 300;
        foreach(KeyValuePair<string, int> kvp in MapSizes)
        {
            Button newButton = Instantiate(MenuButton, ContentPanel.transform);
            newButton.name = kvp.Key;
            newButton.GetComponentInChildren<Text>().text = kvp.Key + " (" + kvp.Value + "km x " + kvp.Value + "km)";
            newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);
            y -= 100;
            newButton.onClick.AddListener(() => StartMapGeneration(kvp.Value));
        }
    }

    private void StartMapGeneration(int size)
    {
        MapGenerationSettings settings = new MapGenerationSettings(MapGenerationSettings.RandomSeed(), size, size, 0.08f, 1.5f, 5, 30, MapType.Island);
        PMG.GenerateMap(settings, OnMapGenerationDone);

        LoadingScreen.gameObject.SetActive(true);
        Hide();
    }

    private void OnMapGenerationDone(Map map)
    {
        GameObject game = new GameObject("GameModel");
        WorldSimulation model = game.AddComponent<WorldSimulation>();
        model.Init(map);
    }

    public void Hide()
    {
        CanvasGroup.alpha = 0f;
        CanvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        CanvasGroup.alpha = 1f;
        CanvasGroup.blocksRaycasts = true;
    }
}
