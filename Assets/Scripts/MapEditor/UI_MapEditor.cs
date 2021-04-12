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

    // Controls
    private static float ZOOM_SPEED = 0.3f;
    private static float DRAG_SPEED = 0.015f;
    private bool IsMouseWheelDown;
    private float MinCameraHeight;
    private float MaxCameraHeight;

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
    public Button SplitRegionButton;
    public Button MergeRegionsButton;
    public Button CreateNationButton;

    // Interaction
    public List<Region> SelectedRegions = new List<Region>();
    public List<Region> SelectedRegionAdjacent = new List<Region>();
    public List<Region> SelectedRegionLandNeighbours = new List<Region>();
    public List<Region> SelectedRegionWaterNeighbours = new List<Region>();

    // Nations
    public Dictionary<Region, Nation> NationMap;


    void Start()
    {
        GenerateButton.onClick.AddListener(Generate);
        SplitRegionButton.onClick.AddListener(SplitSelectedRegion);
        RegionBorderToggle.onValueChanged.AddListener(ToggleRegionBorders);
        MergeRegionsButton.onClick.AddListener(MergeSelectedRegions);
        CreateNationButton.onClick.AddListener(CreateNation);

        MapInfoPanel.SetActive(false);
        RegionInfoPanel.SetActive(false);
        MultRegions_Panel.SetActive(false);
        ClearRegionSelection();
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

        if (PMG.Map != null)
        {
            if (Input.mouseScrollDelta.y != 0) // Scroll
            {
                Camera.main.transform.position += new Vector3(0f, -Input.mouseScrollDelta.y * ZOOM_SPEED, 0f);
                if (Camera.main.transform.position.y < MinCameraHeight) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, MinCameraHeight, Camera.main.transform.position.z);
                if (Camera.main.transform.position.y > MaxCameraHeight) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, MaxCameraHeight, Camera.main.transform.position.z);
            }

            // Dragging with middle mouse button
            if (Input.GetKeyDown(KeyCode.Mouse2)) IsMouseWheelDown = true;
            if (Input.GetKeyUp(KeyCode.Mouse2)) IsMouseWheelDown = false;
            if (IsMouseWheelDown)
            {
                Debug.Log("Drag");
                float speed = Camera.main.transform.position.y * DRAG_SPEED;
                Camera.main.transform.position += new Vector3(-Input.GetAxis("Mouse X") * speed, 0f, -Input.GetAxis("Mouse Y") * speed);
            }
        }
    }

    private void Generate()
    {
        if (int.Parse(WidthText.text) < 3 || int.Parse(WidthText.text) > 25 || int.Parse(HeightText.text) < 3 || int.Parse(HeightText.text) > 25 || float.Parse(MinAreaText.text) > 0.5) return;

        // Reset
        ClearRegionSelection();
        NationMap = new Dictionary<Region, Nation>();

        // Generate new map
        float minRegionArea = float.Parse(MinAreaText.text);
        float maxRegionArea = float.Parse(MaxAreaText.text);
        PMG.GenerateMap(int.Parse(WidthText.text), int.Parse(HeightText.text), minRegionArea, maxRegionArea, IslandToggle.isOn, PolygonMapGenerator.DefaultLandColor, PolygonMapGenerator.DefaultWaterColor, RegionBorderToggle.isOn, callback: OnMapGenerationDone, destroyOldMap:false);
    }

    private void OnMapGenerationDone()
    {
        SetMapInformation(PMG.Map);
        if (CurrentMap != null) CurrentMap.DestroyAllGameObjects();
        CurrentMap = PMG.Map;
        MinCameraHeight = 0.4f;
        MaxCameraHeight = Mathf.Max(CurrentMap.Width, CurrentMap.Height) * 1.2f;

        foreach (Region r in CurrentMap.Regions) NationMap.Add(r, null);
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
        foreach (Region r in SelectedRegions)
        {
            if (NationMap[r] != null) NationMap[r].RemoveRegion(r);
            newNation.AddRegion(r);
            NationMap[r] = newNation;
        }
        ClearRegionSelection();
    }

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
