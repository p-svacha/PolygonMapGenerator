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
        public Color SelectedDistrictColor = Color.red;

        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();
        public List<Party> Parties = new List<Party>();
        public Party PlayerParty;

        public List<GeographyTrait> ActiveGeographyPolicies = new List<GeographyTrait>();
        public List<EconomyTrait> ActiveEconomyPolicies = new List<EconomyTrait>();
        public List<Density> ActiveDensityPolicies = new List<Density>();
        public List<AgeGroup> ActiveAgeGroupPolicies = new List<AgeGroup>();
        public List<Language> ActiveLanguagePolicies = new List<Language>();
        public List<Religion> ActiveReligionPolicies = new List<Religion>();

        private const int NumOpponents = 3;

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

            CreateParties();
            AddRandomDistrict();

            UI.SelectTab(Tab.Policies);
            
            State = GameState.Running;
        }

        private void CreateParties()
        {
            PlayerParty = new Party("Player Party", Color.red);
            Parties.Add(PlayerParty);
            List<Color> takenColors = new List<Color>();
            for(int i = 0; i < NumOpponents; i++)
            {
                string name = PartyNameGenerator.GetRandomPartyName();
                Color color = PartyNameGenerator.GetPartyColor(name, takenColors);
                takenColors.Add(color);
                Party p = new Party(name, color);
                Parties.Add(p);
            }
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


            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(SelectedDistrict != null)
                {
                    SelectedDistrict.RunElection(Parties);
                    UI.SelectDistrict(SelectedDistrict);
                }
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                AddRandomDistrict();
            }
        }

        private void SelectDistrict(District d)
        {
            if (SelectedDistrict == d && SelectedDistrict != null) // Clicking on a selected district unselects it
            {
                SelectedDistrict.Region.Unhighlight();
                SelectedDistrict = null;
            }
            else
            {
                if (SelectedDistrict != null) SelectedDistrict.Region.Unhighlight();
                SelectedDistrict = d;
            }
            if (SelectedDistrict != null)
            {
                SelectedDistrict.Region.Highlight(SelectedDistrictColor);
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
                        foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(t, 0));
                    }
                }

                if (!ActiveEconomyPolicies.Contains(d.Economy1))
                {
                    ActiveEconomyPolicies.Add(d.Economy1);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(d.Economy1, 0));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy2))
                {
                    ActiveEconomyPolicies.Add(d.Economy2);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(d.Economy2, 0));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy3))
                {
                    ActiveEconomyPolicies.Add(d.Economy3);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(d.Economy3, 0));
                }

                if (!ActiveDensityPolicies.Contains(d.Density))
                {
                    ActiveDensityPolicies.Add(d.Density);
                    foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(d.Density, 0));
                }
                if (!ActiveAgeGroupPolicies.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupPolicies.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(d.AgeGroup, 0));
                }
                if (!ActiveLanguagePolicies.Contains(d.Language))
                {
                    ActiveLanguagePolicies.Add(d.Language);
                    foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(d.Language, 0));
                }
                if (!ActiveReligionPolicies.Contains(d.Religion))
                {
                    ActiveReligionPolicies.Add(d.Religion);
                    foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(d.Religion, 0));
                }
            }
        }

        private void AddDistrict(Region r)
        {
            Districts.Add(r, new District(r));
            UpdateDistrictColors();
            UpdateActivePolicies();
            CameraHandler.FocusDistricts(Districts.Values.ToList());
        }

        #endregion

        #region Game Commands

        public void AddRandomDistrict()
        {
            if (Districts.Count == 0)
            {
                Region randomRegion = Map.LandRegions[UnityEngine.Random.Range(0, Map.LandRegions.Count)];
                AddDistrict(randomRegion);
                return;
            }
            else
            {
                // 1. Find regions which have the highest ratio of (neighbouring active districts : neighbouring inactive districts)
                List<Region> candidates = new List<Region>();
                float highestRatio = 0;
                foreach(Region r in Map.LandRegions.Where(x => !Districts.Keys.Contains(x)))
                {
                    int activeNeighbours = r.Neighbours.Where(x => Districts.Keys.Contains(x)).Count();
                    int totalNeighbours = r.Neighbours.Count;
                    float ratio = 1f * activeNeighbours / totalNeighbours;
                    if (ratio == highestRatio) candidates.Add(r);
                    else if(ratio > highestRatio)
                    {
                        candidates.Clear();
                        candidates.Add(r);
                        highestRatio = ratio;
                    }
                }
                Region chosenRegion = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                AddDistrict(chosenRegion);
            }
        }

        #endregion

        #region Random Values

        public static Mentality GetRandomCulture()
        {
            Array values = Enum.GetValues(typeof(Mentality));
            return (Mentality)values.GetValue(UnityEngine.Random.Range(0, values.Length));
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
