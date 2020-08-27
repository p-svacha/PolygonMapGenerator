using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LandmassInfo : MonoBehaviour
{
    public Text LandmassName;
    public Text LandmassSize;
    public Text LandmassArea;

    public void SetLandmassInfo(Landmass landmass)
    {
        if(landmass == null)
        {
            LandmassName.text = "";
            LandmassSize.text = "";
            LandmassArea.text = "";
        }
        else
        {
            LandmassName.text = landmass.Name;
            LandmassSize.text = landmass.Size.ToString() + (landmass.Size == 1 ? " Region" : " Regions");
            LandmassArea.text = landmass.Area.ToString("0.00") + " km²";
        }
    }
}
