using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    public Map Map;

    public void Init(Map map)
    {
        Debug.Log("Initializing game");
        Map = map;
        Camera.main.transform.position = new Vector3(Map.Width / 2, Map.Height * 0.9f, Map.Height / 2);
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Map.SetDisplayMode(MapDisplayMode.Political);
    }

    // Update is called once per frame
    void Update()
    {
        Map.HandleInputs();
    }
}
