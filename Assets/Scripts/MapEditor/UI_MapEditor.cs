using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MapEditor : MonoBehaviour
{
    public PolygonMapGenerator PMG;
    public Button GenerateButton;

    public Text WidthText;
    public Text HeightText;

    public Text MinAreaText;
    public Text MaxAreaText;

    public Toggle IslandToggle;

    public Toggle RegionBorderToggle;

    public GameObject MapInfoPanel;
    public Text MapAreaText;
    public Text NumRegionsText;
    public Text LandAreaText;
    public Text NumLandmassesText;
    public Text WaterAreaText;
    public Text NumWaterBodiesText;

    public GameObject RegionInfoPanel;
    public Text RegionAreaText;
    public Text RegionIdText;
    public Button SplitRegionButton;

    // Interaction
    public Region SelectedRegion;

    void Start()
    {
        GenerateButton.onClick.AddListener(Generate);
        SplitRegionButton.onClick.AddListener(SplitSelectedRegion);
        RegionBorderToggle.onValueChanged.AddListener(ToggleRegionBorders);
        MapInfoPanel.SetActive(false);
        RegionInfoPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Click
        {
            bool uiClick = EventSystem.current.IsPointerOverGameObject();

            if (!uiClick)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    Region mouseRegion = hit.transform.gameObject.GetComponent<Region>();
                    SelectRegion(mouseRegion);
                }
            }
        }
    }

    private void Generate()
    {
        SelectRegion(null);
        float minRegionArea = float.Parse(MinAreaText.text);
        float maxRegionArea = float.Parse(MaxAreaText.text);
        PMG.GenerateMap(int.Parse(WidthText.text), int.Parse(HeightText.text), minRegionArea, maxRegionArea, IslandToggle.isOn, RegionBorderToggle.isOn, callback: OnMapGenerationDone);
    }

    private void OnMapGenerationDone()
    {
        SetMapInformation(PMG.Map);
    }

    private void ToggleRegionBorders(bool enabled)
    {
        if (PMG.Map != null) PMG.Map.ShowRegionBorders(enabled);
    }

    private void SetMapInformation(Map map)
    {
        if (map == null) MapInfoPanel.SetActive(false);
        else
        {
            MapInfoPanel.SetActive(true);
            MapAreaText.text = map.Area.ToString() + " km²";
            NumRegionsText.text = map.NumLandRegions.ToString();
            LandAreaText.text = map.LandArea.ToString("0.00") + " km²";
            WaterAreaText.text = map.WaterArea.ToString("0.00") + " km²";
            NumLandmassesText.text = map.NumLandmasses.ToString();
            NumWaterBodiesText.text = map.NumWaterBodies.ToString();
        }
    }

    private void SelectRegion(Region region)
    {
        if (SelectedRegion == region && SelectedRegion != null) // Clicking on a selected region unselects it
        {
            SelectedRegion.SetHighlighted(false);
            SelectedRegion = null;
        }
        else
        {
            if (SelectedRegion != null) SelectedRegion.SetHighlighted(false);
            SelectedRegion = region;
        }

        RegionInfoPanel.SetActive(SelectedRegion != null);
        if (SelectedRegion != null)
        {
            SelectedRegion.SetHighlighted(true);
            RegionAreaText.text = SelectedRegion.Area.ToString("0.00") + " km²";
            RegionIdText.text = SelectedRegion.Polygon.Id.ToString();
        }
    }
    private void SplitSelectedRegion()
    {
        PMG.SplitPolygon(SelectedRegion.Polygon);
        PMG.DrawMap(RegionBorderToggle.isOn);
    }
}
