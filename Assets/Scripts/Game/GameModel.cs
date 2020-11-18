using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    public Map Map;
    public GameUI GameUI;
    public FlagGenerator FlagGenerator;

    public Vector3 DefaultCameraPosition;

    public int Year = 1800;

    public List<Nation> Nations = new List<Nation>();

    public GameEventHandler EventHandler;
    public CameraHandler CameraHandler;

    public void Init(Map map)
    {
        EventHandler = new GameEventHandler(this);
        FlagGenerator = GameObject.Find("FlagGenerator").GetComponent<FlagGenerator>();
        FlagGenerator.GenerateFlag();
        Map = map;
        Map.SetDisplayMode(MapDisplayMode.Topographic);
        GameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        MarkovChainWordGenerator.TargetNumWords = Map.Regions.Count * 2;
        GameObject.Find("LoadingScreen").SetActive(false);

        // Camera
        CameraHandler = Camera.main.GetComponent<CameraHandler>();
        CameraHandler.SetMap(map);
        CameraHandler.JumpToFocusMap(map);

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

        // Active game event
        if (EventHandler.ActiveEvent != null) EventHandler.Update();

        // Handle inputs
        if (Input.GetKeyDown(KeyCode.Space) && EventHandler.ActiveEvent == null)
        {
            EventHandler.ExecuteRandomEvent();
            Year++;
        }
        Map.HandleInputs();
    }

    public void AddLog(string text)
    {
        GameUI.AddLog("Year " + Year + ": " + text);
    }

    #region Game Events

    public void CaptureRegion(Nation nation, Region region)
    {
        if(region.Name == null) region.Name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
        nation.AddProvince(region);
    }

    public Nation CreateNation(Region region)
    {
        region.Name = MarkovChainWordGenerator.GetRandomName(maxLength: 16);
        Nation newNation = new Nation();
        newNation.Name = MarkovChainWordGenerator.GetRandomName(maxLength: 10);
        newNation.Capital = region;
        newNation.Flag = FlagGenerator.GenerateFlag();
        newNation.PrimaryColor = ColorManager.GetRandomColor(Nations.Select(x => x.PrimaryColor).ToList());
        newNation.AddProvince(region);
        Nations.Add(newNation);
        return newNation;
    }

    #endregion
}
