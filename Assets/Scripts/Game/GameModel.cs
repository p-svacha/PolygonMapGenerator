using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    public Map Map;
    public GameUI GameUI;

    public void Init(Map map)
    {
        Debug.Log("Initializing game");
        Map = map;
        Camera.main.transform.position = new Vector3(Map.Width / 2f, Map.Height * 0.9f, Map.Height / 2f);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Map.SetDisplayMode(MapDisplayMode.Political);
        GameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
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
                GameUI.LandmassInfo.SetLandmassInfo(mouseRegion.Landmass);
                GameUI.RegionInfo.SetRegionInfo(mouseRegion);
                GameUI.SetRiverInfo(mouseRegion.Rivers);
            }
        }

        // Handle inputs
        Map.HandleInputs();
    }
}
