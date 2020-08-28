using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject LeftPanel;
    public GameObject RightPanel;

    public UI_NationInfo NationInfo;

    public UI_LandmassInfo LandmassInfo;
    public UI_WaterBodyInfo WaterBodyInfo;
    public UI_RegionInfo RegionInfo;
    public UI_RiverInfo RiverInfo;

    public UI_LogPanel LogPanel;

    public float DynamicYStart = -250;
    public float DynamicY;
    public List<UI_RiverInfo> RiverInfos;

    public void UpdateRightPanel(Region region)
    {
        if(region.IsWater)
        {
            WaterBodyInfo.gameObject.SetActive(true);
            NationInfo.gameObject.SetActive(false);
            LandmassInfo.gameObject.SetActive(false);
            RegionInfo.gameObject.SetActive(false);

            WaterBodyInfo.SetWaterBodyInfo(region.WaterBody);
            SetRiverInfo(region.Rivers);
        }
        else
        {
            WaterBodyInfo.gameObject.SetActive(false);
            LandmassInfo.gameObject.SetActive(true);
            RegionInfo.gameObject.SetActive(true);

            if (region.Nation == null) NationInfo.gameObject.SetActive(false);
            else
            {
                NationInfo.gameObject.SetActive(true);
                NationInfo.SetNationInfo(region.Nation);
            }
            
            LandmassInfo.SetLandmassInfo(region.Landmass);
            RegionInfo.SetRegionInfo(region);
            SetRiverInfo(region.Rivers);
        }
    }

    public void AddLog(string text)
    {
        LogPanel.SetText(text);
    }

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
