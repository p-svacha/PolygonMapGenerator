using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject LeftPanel;
    public GameObject RightPanel;

    public UI_LandmassInfo LandmassInfo;
    public UI_RegionInfo RegionInfo;
    public UI_RiverInfo RiverInfo;

    public float DynamicYStart = -200;
    public float DynamicY;
    public List<UI_RiverInfo> RiverInfos;

    public void SetRiverInfo(List<River> rivers)
    {
        foreach (UI_RiverInfo info in RiverInfos)
            Destroy(info.gameObject);

        RiverInfos.Clear();
        DynamicY = DynamicYStart;

        foreach(River r in rivers)
        {
            UI_RiverInfo info = GameObject.Instantiate(RiverInfo, RightPanel.transform);
            info.SetRiverInfo(r);

            info.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, DynamicY);
            DynamicY -= info.GetComponent<RectTransform>().sizeDelta.y;
            RiverInfos.Add(info);
        }
    }
    
}
