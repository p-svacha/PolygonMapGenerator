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
        public GameState State;
        public District SelectedDistrict;

        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();
        public List<Party> Parties = new List<Party>();
        public Party PlayerParty;

        public List<GeographyTrait> ActiveGeographyPolicies = new List<GeographyTrait>();
        public List<EconomyTrait> ActiveEconomyPolicies = new List<EconomyTrait>();
        public List<Density> ActiveDensityPolicies = new List<Density>();
        public List<AgeGroup> ActiveAgeGroupPolicies = new List<AgeGroup>();
        public List<Language> ActiveLanguagePolicies = new List<Language>();
        public List<Religion> ActiveReligionPolicies = new List<Religion>();

        #region Initialization
        // Start is called before the first frame update
        void Start()
        {
            StartGame();
            State = GameState.Loading;
        }

        private void StartGame()
        {
            PMG.GenerateMap(10, 10, 0.08f, 1.5f, island: true, drawRegionBorders: true, callback: OnMapGenerated);
        }

        private void OnMapGenerated()
        {
            Map = PMG.Map;

            PlayerParty = new Party();
            Parties.Add(PlayerParty);

            Region startRegion = Map.LandRegions[UnityEngine.Random.Range(0, Map.LandRegions.Count)];
            AddDistrict(startRegion);

            CameraHandler.FocusDistricts(Districts.Values.ToList());

            UI.SelectTab(Tab.Policies);
            
            State = GameState.Running;
        }

        #endregion

        #region Game Flow

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
            if (SelectedDistrict != null)
            {
                SelectedDistrict.Region.SetHighlighted(true);
                UI.SelectDistrict(SelectedDistrict);
            }
        }

        #endregion

        #region Update Values

        private void UpdateDistrictColors()
        {
            foreach(Region r in Map.LandRegions)
            {
                if (Districts.ContainsKey(r)) r.SetGreyedOut(false);
                else r.SetGreyedOut(true);
            }
        }

        private void UpdateActivePolicies()
        {
            foreach(District d in Districts.Values)
            {
                foreach(GeographyTrait t in d.Geography)
                {
                    if(!ActiveGeographyPolicies.Contains(t))
                    {
                        ActiveGeographyPolicies.Add(t);
                        foreach (Party p in Parties) p.AddGeographyPolicy(t);
                    }
                }

                if (!ActiveEconomyPolicies.Contains(d.Economy1))
                {
                    ActiveEconomyPolicies.Add(d.Economy1);
                    foreach (Party p in Parties) p.AddEconomyPolicy(d.Economy1);
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy2))
                {
                    ActiveEconomyPolicies.Add(d.Economy2);
                    foreach (Party p in Parties) p.AddEconomyPolicy(d.Economy2);
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy3))
                {
                    ActiveEconomyPolicies.Add(d.Economy3);
                    foreach (Party p in Parties) p.AddEconomyPolicy(d.Economy3);
                }

                if (!ActiveDensityPolicies.Contains(d.Density))
                {
                    ActiveDensityPolicies.Add(d.Density);
                    foreach (Party p in Parties) p.AddDensityPolicy(d.Density);
                }
                if (!ActiveLanguagePolicies.Contains(d.Language))
                {
                    ActiveLanguagePolicies.Add(d.Language);
                    foreach (Party p in Parties) p.AddLanguagePolicy(d.Language);
                }
                if (!ActiveReligionPolicies.Contains(d.Religion))
                {
                    ActiveReligionPolicies.Add(d.Religion);
                    foreach (Party p in Parties) p.AddReligionPolicy(d.Religion);
                }
                if (!ActiveAgeGroupPolicies.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupPolicies.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.AddAgeGroupPolicy(d.AgeGroup);
                }
            }
        }

        #endregion

        #region Game Commands

        private void AddDistrict(Region r)
        {
            Districts.Add(r, new District(r));
            UpdateDistrictColors();
            UpdateActivePolicies();
        }

        #endregion

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
        public static AgeGroup GetRandomAgeGroup()
        {
            Array values = Enum.GetValues(typeof(AgeGroup));
            return (AgeGroup)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        #endregion
    }
}
