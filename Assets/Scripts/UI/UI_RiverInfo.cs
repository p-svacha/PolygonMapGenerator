using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RiverInfo : MonoBehaviour
{
    public Text RiverName;
    public Text RiverRegions;
    public Text RiverLength;

    public void SetRiverInfo(River river)
    {
        if (river == null)
        {
            RiverName.text = "";
            RiverRegions.text = "";
            RiverLength.text = "";
        }
        else
        {
            RiverName.text = river.Name;
            RiverRegions.text = river.Regions.Count + " Regions";
            RiverLength.text = river.Length.ToString("0.00") + " km";
        }
    }
}
