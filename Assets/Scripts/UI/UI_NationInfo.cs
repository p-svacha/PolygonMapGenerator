using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_NationInfo : MonoBehaviour
{
    public Text NationName;
    public Text NationCapital;
    public Text NationRegions;
    public Text NationArea;
    public Image NationFlag;

    public void SetNationInfo(Nation nation)
    {
        if (nation == null)
        {
            NationName.text = "";
            NationCapital.text = "";
            NationRegions.text = "";
            NationArea.text = "";
            NationFlag.sprite = null;
        }
        else
        {
            NationName.text = nation.Name;
            NationCapital.text = nation.Capital.Name;
            NationRegions.text = nation.Regions.Count.ToString() + (nation.Regions.Count == 1 ? " Region" : " Regions");
            NationArea.text = nation.Area.ToString("0.00") + " km²";
            NationFlag.sprite = nation.Flag;
        }
    }
}
