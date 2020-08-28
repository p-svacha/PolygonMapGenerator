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

    void Start()
    {
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
        PMG.GenerateMap(size, size);

        LoadingScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
