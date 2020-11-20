using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class ElectionTacticsGame : MonoBehaviour
    {
        public PolygonMapGenerator PMG;
        public Map Map;
        public CameraHandler CameraHandler;
        public UI_ElectionTactics UI;

        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();

        public GameState State;

        public District SelectedDistrict;

        // Start is called before the first frame update
        void Start()
        {
            StartGame();
            State = GameState.Loading;
        }

        // Update is called once per frame
        void Update()
        {
            switch(State)
            {
                case GameState.Running:
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
                            if (Districts.ContainsKey(mouseRegion)) SelectDistrict(Districts[mouseRegion]);
                            else SelectDistrict(null);
                        }
                    }
                }
                break;
            }
        }

        private void SelectDistrict(District d)
        {
            if (SelectedDistrict == d && SelectedDistrict != null) // Clicking on a selected district unselects it
            {
                SelectedDistrict.Region.SetHighlighted(false);
                SelectedDistrict = null;
            }
            else
            {
                if (SelectedDistrict != null) SelectedDistrict.Region.SetHighlighted(false);
                SelectedDistrict = d;
            }

            UI.DistrictInfo.gameObject.SetActive(SelectedDistrict != null);
            if (SelectedDistrict != null)
            {
                SelectedDistrict.Region.SetHighlighted(true);
                UI.DistrictInfo.SetDistrict(SelectedDistrict);
            }
        }

        private void StartGame()
        {
            PMG.GenerateMap(10, 10, 0.08f, 1.5f, island: true, drawRegionBorders: true, callback: OnMapGenerated);
        }

        private void OnMapGenerated()
        {
            Map = PMG.Map;
            Region startRegion = Map.LandRegions[UnityEngine.Random.Range(0, Map.LandRegions.Count)];
            Districts.Add(startRegion, new District(startRegion));
            CameraHandler.FocusDistricts(Districts.Values.ToList());
            UpdateDistrictColors();
            State = GameState.Running;
        }

        private void UpdateDistrictColors()
        {
            foreach(Region r in Map.LandRegions)
            {
                if (Districts.ContainsKey(r)) r.SetGreyedOut(false);
                else r.SetGreyedOut(true);
            }
        }

        #region Random Values

        public static CultureTrait GetRandomCulture()
        {
            Array values = Enum.GetValues(typeof(CultureTrait));
            return (CultureTrait)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static Density GetRandomDensity()
        {
            Array values = Enum.GetValues(typeof(Density));
            return (Density)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static EconomyTrait GetRandomEconomyTrait()
        {
            Array values = Enum.GetValues(typeof(EconomyTrait));
            return (EconomyTrait)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static Language GetRandomLanguage()
        {
            Array values = Enum.GetValues(typeof(Language));
            return (Language)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        public static Religion GetRandomReligion()
        {
            Array values = Enum.GetValues(typeof(Religion));
            return (Religion)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        #endregion
    }
}
