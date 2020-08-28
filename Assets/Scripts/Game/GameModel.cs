using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    public Map Map;
    public GameUI GameUI;
    public FlagGenerator FlagGenerator;

    public int Year = 1800;

    public List<Nation> Nations = new List<Nation>();

    public GameEventHandler EventHandler;

    public void Init(Map map)
    {
        EventHandler = new GameEventHandler(this);
        FlagGenerator = GameObject.Find("FlagGenerator").GetComponent<FlagGenerator>();
        FlagGenerator.GenerateFlag();
        Map = map;
        Camera.main.transform.position = new Vector3(Map.Width / 2f, Map.Height, Map.Height / 2f);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Map.SetDisplayMode(MapDisplayMode.Political);
        GameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        MarkovChainWordGenerator.TargetNumWords = Map.Regions.Count * 2;
        GameObject.Find("LoadingScreen").SetActive(false);

        GameUI.AddLog("Welcome to this new empty world.");
    }

    // Update is called once per frame
    void Update()
    {
        // Get region mouse is pointing at
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Region mouseRegion = hit.transform.gameObject.GetComponent<Region>();
            if (mouseRegion != null)
            {
                GameUI.UpdateRightPanel(mouseRegion);
            }
        }

        if(Input.GetKeyDown(KeyCode.Space) && !EventHandler.IsExecuting)
        {
            EventHandler.ExecuteRandomEvent();
            Year++;
        }

        // Handle inputs
        Map.HandleInputs();
    }

    public void AddLog(string text)
    {
        GameUI.AddLog("Year " + Year + ": " + text);
    }

    #region Game Events

    public Nation CreateNation(Region region)
    {
        region.Name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
        Nation newNation = new Nation();
        newNation.Name = MarkovChainWordGenerator.GetRandomName(maxLength: 10);
        newNation.Capital = region;
        newNation.Flag = FlagGenerator.GenerateFlag();
        newNation.PrimaryColor = ColorManager.GetRandomColor(Nations.Select(x => x.PrimaryColor).ToList());
        newNation.AddRegion(region);
        Nations.Add(newNation);
        return newNation;
    }

    #endregion
}
