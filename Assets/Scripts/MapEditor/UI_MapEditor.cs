using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MapEditor : MonoBehaviour
{
    public PolygonMapGenerator PMG;

    public Map CurrentMap;

    [Header("Colors")]
    public Color SelectedColor = Color.red;
    public Color LandNeighbourColor = new Color(0.75f, 0.50f, 0.16f);
    public Color WaterNeighbourColor = Color.blue;

    [Header("General Settings")]
    public Button GenerateButton;
    public Text WidthText;
    public Text HeightText;
    public Text MinAreaText;
    public Text MaxAreaText;
    public Toggle IslandToggle;
    public Toggle RegionBorderToggle;

    [Header("Map Information")]
    public GameObject MapInfoPanel;
    public Text MapAreaText;
    public Text NumRegionsText;
    public Text LandAreaText;
    public Text NumLandmassesText;
    public Text WaterAreaText;
    public Text NumWaterBodiesText;

    [Header("Region Information")]
    public GameObject RegionInfoPanel;
    public Text RegionAreaText;
    public Text RegionIdText;
    public Button SplitRegionButton;
    public Text TotalBorderText;
    public Text LandBorderText;
    public Text CoastlineText;
    public Text AdjacentRegionsText;
    public Text LandNeighboursText;
    public Text WaterNeighboursText;

    [Header("Multiple Region Selection")]
    public GameObject MultRegions_Panel;
    public Text MultRegions_TitleText;
    public Text MultRegions_AreaText;
    public Text MultRegions_TotalBorderText;
    public Text MultRegions_LandBorderText;
    public Text MultRegions_CoastlineText;
    public Text MultRegions_AdjacentText;
    public Text MultRegions_LandNeighboursText;
    public Text MultRegions_WaterNeighboursText;
    public Button MultRegions_MergeButton;
    public Button MultRegions_CreateNationButton;


    // Interaction
    public List<Region> SelectedRegions = new List<Region>();

    void Start()
    {
        GenerateButton.onClick.AddListener(Generate);
        SplitRegionButton.onClick.AddListener(SplitSelectedRegion);
        RegionBorderToggle.onValueChanged.AddListener(ToggleRegionBorders);
        MultRegions_MergeButton.onClick.AddListener(MergeSelectedRegions);
        MultRegions_CreateNationButton.onClick.AddListener(CreateNation);

        MapInfoPanel.SetActive(false);
        RegionInfoPanel.SetActive(false);
        MultRegions_Panel.SetActive(false);
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
        ClearRegionSelection();
        float minRegionArea = float.Parse(MinAreaText.text);
        float maxRegionArea = float.Parse(MaxAreaText.text);
        PMG.GenerateMap(int.Parse(WidthText.text), int.Parse(HeightText.text), minRegionArea, maxRegionArea, IslandToggle.isOn, PolygonMapGenerator.DefaultLandColor, PolygonMapGenerator.DefaultWaterColor, RegionBorderToggle.isOn, callback: OnMapGenerationDone, destroyOldMap:false);
    }

    private void OnMapGenerationDone()
    {
        SetMapInformation(PMG.Map);
        if (CurrentMap != null) CurrentMap.DestroyAllGameObjects();
        CurrentMap = PMG.Map;
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
        bool holdingCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // Clicking outside a region or on the only selected region clears the selection
        if(region == null || (SelectedRegions.Count == 1 && region == SelectedRegions[0]))
        {
            ClearRegionSelection();
            return;
        }

        // Clicking on a region other than the selected region without ctrl selects the new region and unselects the old region
        if(!holdingCtrl)
        {
            ClearRegionSelection();
            SelectedRegions.Add(region);
        }
        // Clicking on a region while holding control adds/removes the region to/from the current selection
        if(holdingCtrl)
        {
            if (SelectedRegions.Contains(region))
            {
                SelectedRegions.Remove(region);
                region.Unhighlight();
            }
            else SelectedRegions.Add(region);
        }

        // If one region is selected, display the region info
        if(SelectedRegions.Count == 1)
        {
            Region selectedRegion = SelectedRegions[0];
            selectedRegion.Highlight(SelectedColor);
            RegionInfoPanel.SetActive(true);
            MultRegions_Panel.SetActive(false);

            RegionAreaText.text = selectedRegion.Area.ToString("0.00") + " km²";
            RegionIdText.text = selectedRegion.Polygon.Id.ToString();
            TotalBorderText.text = selectedRegion.TotalBorderLength.ToString("0.00" + " km");
            LandBorderText.text = selectedRegion.InlandBorderLength.ToString("0.00" + " km");
            CoastlineText.text = selectedRegion.CoastLength.ToString("0.00" + " km");
            AdjacentRegionsText.text = selectedRegion.AdjacentRegions.Count.ToString();
            LandNeighboursText.text = selectedRegion.LandNeighbours.Count.ToString();
            WaterNeighboursText.text = selectedRegion.WaterNeighbours.Count.ToString();
            SplitRegionButton.interactable = PMG.CanSplitPolygon(selectedRegion.Polygon);
        }

        else if(SelectedRegions.Count > 1)
        {
            RegionInfoPanel.SetActive(false);
            MultRegions_Panel.SetActive(true);
            foreach (Region r in SelectedRegions) r.Highlight(SelectedColor);
            
            MultRegions_TitleText.text = SelectedRegions.Count + " Regions Selected";
            MultRegions_AreaText.text = SelectedRegions.Sum(x => x.Area).ToString("0.00") + " km²";
            MultRegions_TotalBorderText.text = "N/A";
            MultRegions_LandBorderText.text = "N/A";
            MultRegions_CoastlineText.text = "N/A";
            MultRegions_AdjacentText.text = "N/A";
            MultRegions_LandNeighboursText.text = "N/A";
            MultRegions_WaterNeighboursText.text = "N/A";

            MultRegions_MergeButton.interactable = (SelectedRegions.Count == 2 && PMG.CanMergePolygons(SelectedRegions[0].Polygon, SelectedRegions[1].Polygon) && SelectedRegions.All(x => !x.IsWater));
            MultRegions_CreateNationButton.interactable = (SelectedRegions.All(x => !x.IsWater));
        }
    }

    private void ClearRegionSelection()
    {
        foreach (Region r in SelectedRegions) r.Unhighlight();
        SelectedRegions.Clear();
        RegionInfoPanel.SetActive(false);
        MultRegions_Panel.SetActive(false);
    }

    private void SplitSelectedRegion()
    {
        PMG.SplitPolygon(SelectedRegions[0].Polygon);
        ClearRegionSelection();
        PMG.FindWaterNeighbours();
        PMG.DrawMap(RegionBorderToggle.isOn);
    }

    private void MergeSelectedRegions()
    {
        PMG.MergePolygons(SelectedRegions[0].Polygon, SelectedRegions[1].Polygon);
        ClearRegionSelection();
        PMG.FindWaterNeighbours();
        PMG.DrawMap(RegionBorderToggle.isOn);
    }

    private void CreateNation()
    {
        Nation newNation = new Nation();
        newNation.Name = "Test";
        newNation.PrimaryColor = ColorManager.GetRandomColor();
        newNation.SecondaryColor = ColorManager.GetRandomColor(new List<Color>() { newNation.PrimaryColor });
        foreach (Region r in SelectedRegions) newNation.AddRegion(r);
        newNation.CreateNationPolygons();
    }

    public void HighlightNeighbours()
    {
        foreach (Region r in SelectedRegions[0].LandNeighbours) r.Highlight(LandNeighbourColor);
        foreach (Region r in SelectedRegions[0].WaterNeighbours) r.Highlight(WaterNeighbourColor);
    }

    public void UnhighlightNeighbours()
    {
        foreach (Region r in SelectedRegions[0].LandNeighbours) r.Unhighlight();
        foreach (Region r in SelectedRegions[0].WaterNeighbours) r.Unhighlight();
    }
}
