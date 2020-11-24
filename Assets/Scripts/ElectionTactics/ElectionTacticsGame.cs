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

        public Dictionary<Region, District> Districts = new Dictionary<Region, District>();
        public List<Party> Parties = new List<Party>();
        public Party PlayerParty;

        public List<GeographyTrait> ActiveGeographyPolicies = new List<GeographyTrait>();
        public List<EconomyTrait> ActiveEconomyPolicies = new List<EconomyTrait>();
        public List<Density> ActiveDensityPolicies = new List<Density>();
        public List<AgeGroup> ActiveAgeGroupPolicies = new List<AgeGroup>();
        public List<Language> ActiveLanguagePolicies = new List<Language>();
        public List<Religion> ActiveReligionPolicies = new List<Religion>();

        // Game values
        private const int PolicyPointsPerCycle = 4;
        private const int NumOpponents = 4;
        public int ElectionCycle;

        // General Election
        private float HeaderSlideTime = 1f;
        private float DistrictPanTime = 2f;
        private float PostDistrictPanTime = 1f;
        private float GraphAnimationTime = 6f;
        private float PostGraphPauseTime = 2.5f;
        private float ListAnimationTime = 1f;
        private float PostListAnimationTime = 1f;

        private List<District> ElectionOrder;
        private int CurElectionDistrictIndex;
        private District CurElectionDistrict;


        #region Initialization

        // Start is called before the first frame update
        void Start()
        {
            StartGame();
            State = GameState.Loading;
        }

        private void StartGame()
        {
            PMG.GenerateMap(10, 10, 0.08f, 1.5f, island: true, ColorManager.Colors.InactiveDistrictColor, ColorManager.Colors.WaterColor, drawRegionBorders: true, callback: OnMapGenerated);
        }

        private void OnMapGenerated()
        {
            Map = PMG.Map;

            CreateParties();

            UI.SelectTab(Tab.Parliament);
            StartElectionCycle();
            CameraHandler.FocusDistricts(Districts.Values.ToList());

            State = GameState.Running;
        }

        private void CreateParties()
        {
            PlayerParty = new Party(this, "Player Party", Color.red);
            Parties.Add(PlayerParty);
            List<Color> takenColors = new List<Color>();
            for(int i = 0; i < NumOpponents; i++)
            {
                string name = PartyNameGenerator.GetRandomPartyName(maxLength: 35);
                Color color = PartyNameGenerator.GetPartyColor(name, takenColors);
                takenColors.Add(color);
                Party p = new Party(this, name, color);
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
                            if (Districts.ContainsKey(mouseRegion)) UI.SelectDistrict(Districts[mouseRegion]);
                            else UI.SelectDistrict(null);
                        }
                    }
                }
                break;
            }


            /*
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(UI.SelectedDistrict != null)
                {
                    UI.SelectedDistrict.RunElection(PlayerParty, Parties);
                    UI.DistrictInfo.Init(UI.SelectedDistrict);
                }
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                AddRandomDistrict();
            }
            if(Input.GetKeyDown(KeyCode.S))
            {
                Parties[UnityEngine.Random.Range(0, Parties.Count)].Seats += UnityEngine.Random.Range(1, 11);
                UI.Parliament.PartyList.MovePositions(Parties, 1f);
            }
            */
        }

        private void StartElectionCycle()
        {
            ElectionCycle++;
            AddRandomDistrict();
            foreach (Party p in Parties) AddPolicyPoints(p, PolicyPointsPerCycle);
        }

        #endregion

        #region Update Values

        private void UpdateDistrictColors()
        {
            foreach(Region r in Map.LandRegions)
            {
                if (Districts.ContainsKey(r)) r.SetColor(ColorManager.Colors.ActiveDistrictColor);
                else r.SetColor(ColorManager.Colors.InactiveDistrictColor);
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
                        foreach (Party p in Parties) p.AddPolicy(new GeographyPolicy(p, t, 0));
                    }
                }

                if (!ActiveEconomyPolicies.Contains(d.Economy1))
                {
                    ActiveEconomyPolicies.Add(d.Economy1);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy1, 0));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy2))
                {
                    ActiveEconomyPolicies.Add(d.Economy2);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy2, 0));
                }
                if (!ActiveEconomyPolicies.Contains(d.Economy3))
                {
                    ActiveEconomyPolicies.Add(d.Economy3);
                    foreach (Party p in Parties) p.AddPolicy(new EconomyPolicy(p, d.Economy3, 0));
                }

                if (!ActiveDensityPolicies.Contains(d.Density))
                {
                    ActiveDensityPolicies.Add(d.Density);
                    foreach (Party p in Parties) p.AddPolicy(new DensityPolicy(p, d.Density, 0));
                }
                if (!ActiveAgeGroupPolicies.Contains(d.AgeGroup))
                {
                    ActiveAgeGroupPolicies.Add(d.AgeGroup);
                    foreach (Party p in Parties) p.AddPolicy(new AgeGroupPolicy(p, d.AgeGroup, 0));
                }
                if (!ActiveLanguagePolicies.Contains(d.Language))
                {
                    ActiveLanguagePolicies.Add(d.Language);
                    foreach (Party p in Parties) p.AddPolicy(new LanguagePolicy(p, d.Language, 0));
                }
                if (!ActiveReligionPolicies.Contains(d.Religion))
                {
                    ActiveReligionPolicies.Add(d.Religion);
                    foreach (Party p in Parties) p.AddPolicy(new ReligionPolicy(p, d.Religion, 0));
                }
            }
        }

        private void AddDistrict(Region r)
        {
            Districts.Add(r, new District(r));
            UpdateDistrictColors();
            UpdateActivePolicies();
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

        public void AddPolicyPoints(Party p, int amount)
        {
            p.PolicyPoints += amount;
            UI.UpdatePolicyPointDisplay();
        }

        public void IncreasePolicy(Policy p)
        {
            if (PlayerParty.PolicyPoints == 0) return;
            PlayerParty.PolicyPoints--;
            p.IncreaseValue();
            UI.UpdatePolicyPointDisplay();
        }
        public void DecreasePolicy(Policy p)
        {
            if (p.Value == 0) return;
            PlayerParty.PolicyPoints++;
            p.DecreaseValue();
            UI.UpdatePolicyPointDisplay();
        }

        #endregion

        #region Election
        public void RunGeneralElection()
        {
            State = GameState.GeneralElection;
            UI.SelectTab(Tab.Parliament);

            // Reset seats
            foreach(Party p in Parties)
            {
                p.Seats = 0;
                UI.Parliament.PartyList.Init(Parties);
            }

            ElectionOrder = Districts.Values.OrderBy(x => x.Population).ToList();
            CurElectionDistrictIndex = 0;

            UI.SlideOutHeader(HeaderSlideTime);
            Invoke(nameof(MoveToNextElectionDistrict), HeaderSlideTime);
        }

        private void MoveToNextElectionDistrict()
        {
            UI.Parliament.CurrentElectionGraph.ClearGraph();

            if (CurElectionDistrict != null) CurElectionDistrict.Region.Unhighlight();

            if (CurElectionDistrictIndex < ElectionOrder.Count)
            {
                CurElectionDistrict = ElectionOrder[CurElectionDistrictIndex];
                CurElectionDistrict.Region.Highlight(ColorManager.Colors.SelectedDistrictColor);

                UI.Parliament.CurrentElectionTitle.text = CurElectionDistrict.Name;
                UI.Parliament.CurrentElectionSeatsText.text = CurElectionDistrict.Seats.ToString();
                UI.Parliament.CurrentElectionSeatsIcon.gameObject.SetActive(true);
                CameraHandler.MoveToFocusDistricts(new List<District>() { CurElectionDistrict }, DistrictPanTime);
                Invoke(nameof(RunCurrentDistrictElection), DistrictPanTime + PostDistrictPanTime);
            }
            else
            {
                UI.Parliament.CurrentElectionTitle.text = "";
                UI.Parliament.CurrentElectionSeatsText.text = "";
                UI.Parliament.CurrentElectionSeatsIcon.gameObject.SetActive(false);
                Invoke(nameof(EndElection), PostDistrictPanTime);
            }
        }

        private void RunCurrentDistrictElection()
        {
            CurElectionDistrict.RunElection(PlayerParty, Parties);
            CurElectionDistrict.CurrentWinnerParty.Seats += CurElectionDistrict.Seats;

            List<GraphDataPoint> dataPoints = new List<GraphDataPoint>();
            foreach (KeyValuePair<Party, float> kvp in CurElectionDistrict.LastElectionResult.VoteShare)
                dataPoints.Add(new GraphDataPoint(kvp.Key.Acronym, kvp.Value, kvp.Key.Color));
            int yMax = (((int)CurElectionDistrict.LastElectionResult.VoteShare.Values.Max(x => x)) / 9 + 1) * 10;

            UI.Parliament.CurrentElectionGraph.ShowAnimatedBarGraph(dataPoints, yMax, 10, 0.1f, Color.white, Color.grey, UI.Font, GraphAnimationTime);
            CurElectionDistrictIndex++;
            Invoke(nameof(UpdatePartyList), GraphAnimationTime + PostGraphPauseTime);
            Invoke(nameof(UnhighlightPartyList), GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + 0.1f);
            Invoke(nameof(MoveToNextElectionDistrict), GraphAnimationTime + PostGraphPauseTime + ListAnimationTime + PostListAnimationTime);
        }

        private void UpdatePartyList()
        {
            UI.Parliament.PartyList.HighlightParty(CurElectionDistrict.CurrentWinnerParty);
            UI.Parliament.PartyList.MovePositions(Parties, ListAnimationTime);
        }

        private void UnhighlightPartyList()
        {
            UI.Parliament.PartyList.UnhighlightParty(CurElectionDistrict.CurrentWinnerParty);
        }

        private void EndElection()
        {
            UI.Parliament.CurrentElectionContainer.SetActive(false);
            StartElectionCycle();
            CameraHandler.MoveToFocusDistricts(Districts.Values.ToList(), DistrictPanTime);
            UI.SlideInHeader(DistrictPanTime);

            State = GameState.Running;
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
