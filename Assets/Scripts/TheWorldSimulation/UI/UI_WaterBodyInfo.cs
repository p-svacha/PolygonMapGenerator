using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WaterBodyInfo : MonoBehaviour
{
    public Text WaterBodyName;
    public Text WaterBodyRegions;
    public Text WaterBodyArea;

    public void SetWaterBodyInfo(WaterBody waterBody)
    {
        if (waterBody == null)
        {
            WaterBodyName.text = "";
            WaterBodyRegions.text = "";
            WaterBodyArea.text = "";
        }
        else
        {
            WaterBodyName.text = waterBody.Name;
            WaterBodyRegions.text = waterBody.BorderingLandRegions.Count.ToString();
            WaterBodyArea.text = waterBody.Area.ToString("0.00") + " km²";
        }
    }
}
