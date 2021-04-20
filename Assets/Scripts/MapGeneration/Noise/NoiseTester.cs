using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTester : MonoBehaviour
{
    public GameObject ContinentPlane;
    public GameObject TemperaturePlane;
    public GameObject PrecipitationPlane;
    public GameObject LandTopologyPlane;
    public GameObject WaterTopologyPlane;
    public GameObject WindPlane;

    // Start is called before the first frame update
    public void DisplayNoise(Noise noise, GameObject plane, MapGenerationSettings settings, float blackValue = 0f, float whiteValue = 1f)
    {
        int texRes = 32;
        plane.transform.localScale = new Vector3(settings.Width / 10, 1, settings.Height / 10);
        plane.transform.position = new Vector3(plane.transform.localScale.x * 10 / 2, -1f, plane.transform.localScale.z * 10 / 2);

        Texture2D tex = new Texture2D((int)plane.transform.localScale.x * texRes, (int)plane.transform.localScale.z * texRes); 
        for(int y = 0; y < tex.height; y++)
        {
            for(int x = 0; x < tex.width; x++)
            {
                float noiseX = (((float)x / (float)tex.width) * (plane.transform.localScale.x * 10));
                float noiseY = (((float)y / (float)tex.height) * (plane.transform.localScale.z * 10));
                float value = (noise.GetValue(noiseX, noiseY, settings));
                //Debug.Log("Value at " + noiseX + "/" + noiseY + "/" + 1f + ": " + rmfn.GetValue(noiseX, noiseY, 1f));
                float colorValue = (value - blackValue) / (whiteValue - blackValue);
                tex.SetPixel(tex.width - x - 1, tex.height - y - 1,  new Color(colorValue, colorValue, colorValue));
            }
        }
        tex.Apply();

        plane.GetComponent<MeshRenderer>().material.mainTexture = tex;
    }
}
