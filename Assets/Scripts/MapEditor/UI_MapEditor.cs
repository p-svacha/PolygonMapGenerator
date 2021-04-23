using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MapEditor : MonoBehaviour
{
    public PolygonMapGenerator PMG;
    public BaseMapCameraHandler CameraControls;

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
    public Text MinContinentSizeText;
    public Text MaxContinentSizeText;

    [Header("Map Type")]
    public Dropdown MapTypeDropdown;
    private MapType CurrentMapType;
    public GameObject ContinentsOptions;
    public Slider ContinentSizeSlider;

    [Header("Display")]
    public Toggle RegionBorderToggle;
    public Toggle ShorelineBorderToggle;
    public Toggle ConnectionOverlayToggle;
    public Dropdown DrawModeDropdown;
    private MapDrawMode CurrentDrawMode;

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
    public Text Region_BiomeText;
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

    [Header("Buttons")]
    public Button TurnToLandButton;
    public Button TurnToWaterButton;
    public Button SplitRegionButton;
    public Button MergeRegionsButton;
    public Button CreateNationButton;

    // Interaction
    public Region LastHoveredRegion;
    public Region HoveredRegion;

    public List<Region> SelectedRegions = new List<Region>();
    public List<Region> SelectedRegionAdjacent = new List<Region>();
    public List<Region> SelectedRegionLandNeighbours = new List<Region>();
    public List<Region> SelectedRegionWaterNeighbours = new List<Region>();

    // Nations
    public List<Nation> Nations = new List<Nation>();
    public Dictionary<Region, Nation> NationMap;


    void Start()
    {
        GenerateButton.onClick.AddListener(GenerateButton_OnClick);

        foreach (MapType mapType in Enum.GetValues(typeof(MapType))) MapTypeDropdown.options.Add(new Dropdown.OptionData(mapType.ToString()));
        MapTypeDropdown.onValueChanged.AddListener(MapTypeDropdown_OnValueChanged);
        MapTypeDropdown.value = 1; MapTypeDropdown.value = 0;
        ContinentSizeSlider.onValueChanged.AddListener(ContinentSizeSlider_OnValueChanged);

        RegionBorderToggle.onValueChanged.AddListener(RegionBorderToggle_OnValueChanged);
        ShorelineBorderToggle.onValueChanged.AddListener(ShorelineBorderToggle_OnValueChanged);
        foreach (MapDrawMode drawMode in Enum.GetValues(typeof(MapDrawMode))) DrawModeDropdown.options.Add(new Dropdown.OptionData(drawMode.ToString()));
        DrawModeDropdown.onValueChanged.AddListener(DrawModeDropdown_OnValueChanged);
        DrawModeDropdown.value = 1; DrawModeDropdown.value = 0;

        TurnToLandButton.onClick.AddListener(TurnSelectedToRegionsToLand);
        TurnToWaterButton.onClick.AddListener(TurnSelectedRegionsToWater);
        SplitRegionButton.onClick.AddListener(SplitSelectedRegion);
        MergeRegionsButton.onClick.AddListener(MergeSelectedRegions);
        CreateNationButton.onClick.AddListener(CreateNation);

        MapInfoPanel.SetActive(false);
        RegionInfoPanel.SetActive(false);
        MultRegions_Panel.SetActive(false);
        ClearRegionSelection();
    }

    void Update()
    {
        // Mouse controls
        if(!CameraControls.IsHoveringUi())
        {
            // Currently hovered region
            LastHoveredRegion = HoveredRegion;
            HoveredRegion = CameraControls.GetHoveredRegion();

            // Region selection
            if (Input.GetMouseButtonDown(0))
            {
                SelectRegion(HoveredRegion);
            }

            // Connection overlay
            if(ConnectionOverlayToggle.isOn && HoveredRegion != LastHoveredRegion)
            {
                if (LastHoveredRegion != null) LastHoveredRegion.ShowConnectionOverlays(false);
                if (HoveredRegion != null && !HoveredRegion.IsWater) HoveredRegion.ShowConnectionOverlays(true);
            }

        }
    }

    #region General

    private void Reset()
    {
        ClearRegionSelection();
        NationMap = new Dictionary<Region, Nation>();
        foreach (Nation n in Nations) n.DestroyAllObjects();
        Nations.Clear();
    }

    private void GenerateButton_OnClick()
    {
        if (int.Parse(WidthText.text) < 3 || int.Parse(WidthText.text) > 25 || int.Parse(HeightText.text) < 3 || int.Parse(HeightText.text) > 25 || float.Parse(MinAreaText.text) > 0.5) return;

        Reset();

        // Generate new map
        int width = int.Parse(WidthText.text);
        int height = int.Parse(HeightText.text);
        float minRegionArea = float.Parse(MinAreaText.text);
        float maxRegionArea = float.Parse(MaxAreaText.text);
        int minContinentSize = int.Parse(MinContinentSizeText.text);
        int maxContinentSize = int.Parse(MaxContinentSizeText.text);
        float continentSizeFactor = ContinentSizeSlider.value;
        MapGenerationSettings settings = new MapGenerationSettings(width, height, minRegionArea, maxRegionArea, minContinentSize, maxContinentSize, CurrentMapType, continentSizeFactor);
        PMG.GenerateMap(settings, callback: OnMapGenerationDone);
    }

    private void OnMapGenerationDone(Map map)
    {
        if (CurrentMap != null) CurrentMap.DestroyAllGameObjects();
        CurrentMap = map;
        CurrentMap.InitializeMap(RegionBorderToggle.isOn, ShorelineBorderToggle.isOn, CurrentDrawMode);
        CameraControls.Init(map);

        SetMapInformation(CurrentMap);
        
        foreach (Region r in CurrentMap.Regions) NationMap.Add(r, null);
    }

    #endregion

    #region Map Type

    private void MapTypeDropdown_OnValueChanged(int value)
    {
        CurrentMapType = (MapType)Enum.Parse(typeof(MapType), MapTypeDropdown.options[value].text);
        switch(CurrentMapType)
        {
            case MapType.Regional:
                ContinentsOptions.SetActive(false);
                break;

            case MapType.Island:
                ContinentsOptions.SetActive(false);
                break;

            case MapType.Continents:
                ContinentsOptions.SetActive(true);
                break;
        }
    }

    private void ContinentSizeSlider_OnValueChanged(float value)
    {

    }

    #endregion

    #region Display

    private void RegionBorderToggle_OnValueChanged(bool enabled)
    {
        if (CurrentMap != null) CurrentMap.ShowRegionBorders(enabled);
    }

    private void ShorelineBorderToggle_OnValueChanged(bool enabled)
    {
        if (CurrentMap != null) CurrentMap.ShowShorelineBorders(enabled);
    }

    private void DrawModeDropdown_OnValueChanged(int value)
    {
        CurrentDrawMode = (MapDrawMode) Enum.Parse(typeof(MapDrawMode), DrawModeDropdown.options[value].text);
        if(CurrentMap != null) CurrentMap.UpdateDrawMode(CurrentDrawMode);
    }

    #endregion



    private void SetMapInformation(Map map)
    {
        if (map == null) MapInfoPanel.SetActive(false);
        else
        {
            MapInfoPanel.SetActive(true);
            MapAreaText.text = map.Attributes.Area.ToString() + " km²";
            NumRegionsText.text = map.NumLandRegions.ToString() + " (" + map.Regions.Count + ")";
            LandAreaText.text = map.LandArea.ToString("0.00") + " km²";
            WaterAreaText.text = map.WaterArea.ToString("0.00") + " km²";
            NumLandmassesText.text = map.NumLandmasses.ToString();
            NumWaterBodiesText.text = map.NumWaterBodies.ToString();
        }
    }

    private void SelectRegion(Region region)
    {
        bool multiSelection = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift);

        // Clicking outside a region or on the only selected region clears the selection
        if(region == null || (SelectedRegions.Count == 1 && region == SelectedRegions[0]))
        {
            ClearRegionSelection();
            return;
        }

        // Clicking on a region other than the selected region without ctrl selects the new region and unselects the old region
        if(!multiSelection)
        {
            ClearRegionSelection();
            SelectedRegions.Add(region);
        }
        // Clicking on a region while holding control adds/removes the region to/from the current selection
        if(multiSelection)
        {
            if (SelectedRegions.Contains(region))
            {
                SelectedRegions.Remove(region);
                region.Unhighlight();
            }
            else SelectedRegions.Add(region);
        }

        UpdateSelectionProperties();

        // If one region is selected, display the region info
        if (SelectedRegions.Count == 1)
        {
            Region selectedRegion = SelectedRegions[0];
            selectedRegion.Highlight(SelectedColor);
            RegionInfoPanel.SetActive(true);
            MultRegions_Panel.SetActive(false);

            RegionAreaText.text = selectedRegion.Area.ToString("0.00") + " km²";
            RegionIdText.text = selectedRegion.Polygon.Id.ToString();
            Region_BiomeText.text = selectedRegion.Biome.ToString();
            TotalBorderText.text = selectedRegion.TotalBorderLength.ToString("0.00" + " km");
            LandBorderText.text = selectedRegion.InlandBorderLength.ToString("0.00" + " km");
            CoastlineText.text = selectedRegion.CoastLength.ToString("0.00" + " km");
            AdjacentRegionsText.text = selectedRegion.AdjacentRegions.Count.ToString();
            LandNeighboursText.text = selectedRegion.LandNeighbours.Count.ToString();
            WaterNeighboursText.text = selectedRegion.WaterNeighbours.Count.ToString();
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
            MultRegions_AdjacentText.text = SelectedRegionAdjacent.Count.ToString();
            MultRegions_LandNeighboursText.text = SelectedRegionLandNeighbours.Count.ToString();
            MultRegions_WaterNeighboursText.text = SelectedRegionWaterNeighbours.Count.ToString();
        }
    }

    private void UpdateSelectionProperties()
    {
        SelectedRegionAdjacent.Clear();
        SelectedRegionLandNeighbours.Clear();
        SelectedRegionWaterNeighbours.Clear();

        foreach(Region r in SelectedRegions)
        {
            foreach(Region landNeighbour in r.LandNeighbours)
            {
                if (SelectedRegionWaterNeighbours.Contains(landNeighbour)) SelectedRegionWaterNeighbours.Remove(landNeighbour);
                if (!SelectedRegionLandNeighbours.Contains(landNeighbour) && !SelectedRegions.Contains(landNeighbour)) SelectedRegionLandNeighbours.Add(landNeighbour);
            }
            foreach(Region waterNeighbour in r.WaterNeighbours)
            {
                if (!SelectedRegionLandNeighbours.Contains(waterNeighbour) && !SelectedRegionWaterNeighbours.Contains(waterNeighbour) && !SelectedRegions.Contains(waterNeighbour)) SelectedRegionWaterNeighbours.Add(waterNeighbour);
            }
            foreach(Region adjacent in r.AdjacentRegions)
            {
                if (!SelectedRegionAdjacent.Contains(adjacent) && !SelectedRegions.Contains(adjacent)) SelectedRegionAdjacent.Add(adjacent);
            }
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        TurnToLandButton.interactable = CurrentMap != null && SelectedRegions.Count > 0 && SelectedRegions.All(x => x.IsWater);
        TurnToWaterButton.interactable = CurrentMap != null && SelectedRegions.Count > 0 && SelectedRegions.All(x => !x.IsWater);
        SplitRegionButton.interactable = CurrentMap != null && SelectedRegions.Count == 1 && PMG.CanSplitPolygon(SelectedRegions[0].Polygon);
        MergeRegionsButton.interactable = CurrentMap != null && SelectedRegions.Count == 2 && PMG.CanMergePolygons(SelectedRegions[0].Polygon, SelectedRegions[1].Polygon);
        CreateNationButton.interactable = CurrentMap != null && SelectedRegions.Count > 0 && SelectedRegions.All(x => !x.IsWater);
    }

    private void ClearRegionSelection()
    {
        foreach (Region r in SelectedRegions) r.Unhighlight();
        SelectedRegions.Clear();
        RegionInfoPanel.SetActive(false);
        MultRegions_Panel.SetActive(false);
        UpdateSelectionProperties();
    }

    #region Button Actions

    private void TurnSelectedToRegionsToLand()
    {
        foreach (Region r in SelectedRegions) r.Polygon.IsWater = false;
        Reset();
        PMG.Redraw();
    }

    private void TurnSelectedRegionsToWater()
    {
        foreach (Region r in SelectedRegions) r.Polygon.IsWater = true;
        Reset();
        PMG.Redraw();
    }

    private void SplitSelectedRegion()
    {
        PMG.SplitPolygon(SelectedRegions[0].Polygon);
        Reset();
        PMG.Redraw();
    }

    private void MergeSelectedRegions()
    {
        PMG.MergePolygons(SelectedRegions[0].Polygon, SelectedRegions[1].Polygon);
        ClearRegionSelection();
        PMG.Redraw();
    }

    private void CreateNation()
    {
        Nation newNation = new Nation();
        newNation.Name = MarkovChainWordGenerator.GetRandomName(10);
        newNation.PrimaryColor = ColorManager.GetRandomColor();
        newNation.SecondaryColor = ColorManager.GetRandomColor(new List<Color>() { newNation.PrimaryColor });
        foreach (Region r in SelectedRegions)
        {
            if (NationMap[r] != null) NationMap[r].RemoveRegion(r);
            newNation.AddRegion(r, false);
            NationMap[r] = newNation;
        }
        ClearRegionSelection();
        newNation.UpdateProperties();
        Nations.Add(newNation);
    }

    #endregion

    public void HighlightNeighbours()
    {
        foreach (Region r in SelectedRegionLandNeighbours) r.Highlight(LandNeighbourColor);
        foreach (Region r in SelectedRegionWaterNeighbours) r.Highlight(WaterNeighbourColor);
    }

    public void UnhighlightNeighbours()
    {
        foreach (Region r in SelectedRegionLandNeighbours) r.Unhighlight();
        foreach (Region r in SelectedRegionWaterNeighbours) r.Unhighlight();
    }
}
