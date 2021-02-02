using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RegionInfo : MonoBehaviour
{
    public Text RegionName;
    public Text RegionArea;

    public void SetRegionInfo(Region region)
    {
        if (region == null || region.IsWater)
        {
            RegionName.text = "";
            RegionArea.text = "";
        }
        else
        {
            RegionName.text = region.Name;
            RegionArea.text = region.Area.ToString("0.00") + " km²";
        }
    }
}
