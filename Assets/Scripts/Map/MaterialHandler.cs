using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialHandler : MonoBehaviour
{
    public Material TopographicLandMaterial;
    public Material TopographicWaterMaterial;

    public Material PoliticalLandMaterial;
    public Material PoliticalWaterMaterial;

    public Material SatelliteLandMaterial;
    public Material SatelliteWaterMaterial;

    public Color LandColor;
    public Color WaterColor;

    public Color HighlightColor;
    public Color GreyedOutColor;

    public static MaterialHandler Materials
    {
        get
        {
            return GameObject.Find("MaterialHandler").GetComponent<MaterialHandler>();
        }
    }
}