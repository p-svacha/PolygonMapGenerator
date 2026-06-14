using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ElectionTactics : MonoBehaviour
    {
        public static UI_ElectionTactics Instance;

        public ElectionTacticsGame Game;
        public Font Font;

        [Header("General")]
        public Button MuteAudioButton;
        public Image MuteAudioButtonIcon;

        [Header("Screens")]
        public UI_LoadingScreen LoadingScreen;
        public UI_PostGameScreen PostGameScreen;

        [Header("Newspaper")]
        public UI_Newspaper Newspaper;
        
        [Header ("Map Controls")]
        public MapControls MapControls;
        public UI_DistrictLabel DistrictLabelPrefab;
        public GameObject OverlayContainer;

        [Header("Side Panel Elements")]
        public UI_SidePanelHeader SidePanelHeader;
        public UI_SidePanelFooter SidePanelFooter;
        public UI_ElectionControls ElectionControls;

        [Header("Tab Buttons")]
        public TabButton DistrictTabButton;
        public TabButton ParliamentTabButton;
        public TabButton ConstitutionTabButton;
        public TabButton EventsTabButton;
        public TabButton PoliciesTabButton;
        public TabButton CampaignsTabButton;
        public TabButton VotingTabButton;
        public TabButton SettingsTabButton;

        public Tab ActiveTab;
        public Dictionary<Tab, GameObject> TabPanels = new Dictionary<Tab, GameObject>(); // Which tab is connected to which object
        public Dictionary<Tab, TabButton> TabButtons = new Dictionary<Tab, TabButton>(); // Which tab is part of which button

        [Header("Side Panel Tabs")]
        public Image SidePanel;
        public GameObject SidePanelContentContainer;
        public UI_DistrictList DistrictList;
        public UI_DistrictInfo DistrictInfo;
        public UI_Parliament Parliament;
        public UI_Constitution Constitution;
        public UI_Events Events;
        public UI_PolicySelection PolicySelection;
        public UI_Campaigns Campaigns;
        public UI_Voting Voting;
        public UI_Settings Settings;

        [Header("Standings Panel")]
        public UI_PartyList StandingsPanel;

        public bool IsDistrictSelected;
        public District SelectedDistrict;
        public bool IsShowingNewspaper => Newspaper.gameObject.activeSelf;

        private void Awake()
        {
            Instance = this;

            MuteAudioButton.onClick.AddListener(ToggleAudio);
        }

        public void Init(ElectionTacticsGame game)
        {
            Game = game;

            // Listeners
            TabPanels.Add(Tab.DistrictList, DistrictList.gameObject);
            TabButtons.Add(Tab.DistrictList, DistrictTabButton);
            DistrictTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.DistrictList));

            TabPanels.Add(Tab.DistrictInfo, DistrictInfo.gameObject);
            TabButtons.Add(Tab.DistrictInfo, DistrictTabButton);

            TabPanels.Add(Tab.Parliament, Parliament.gameObject);
            TabButtons.Add(Tab.Parliament, ParliamentTabButton);
            ParliamentTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Parliament));

            TabPanels.Add(Tab.Constitution, Constitution.gameObject);
            TabButtons.Add(Tab.Constitution, ConstitutionTabButton);
            ConstitutionTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Constitution));

            TabPanels.Add(Tab.Events, Events.gameObject);
            TabButtons.Add(Tab.Events, EventsTabButton);
            EventsTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Events));

            TabPanels.Add(Tab.Policies, PolicySelection.gameObject);
            TabButtons.Add(Tab.Policies, PoliciesTabButton);
            PoliciesTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Policies));

            TabPanels.Add(Tab.Campaigns, Campaigns.gameObject);
            TabButtons.Add(Tab.Campaigns, CampaignsTabButton);
            CampaignsTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Campaigns));

            TabPanels.Add(Tab.Voting, Voting.gameObject);
            TabButtons.Add(Tab.Voting, VotingTabButton);
            VotingTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Voting));

            TabPanels.Add(Tab.Settings, Settings.gameObject);
            TabButtons.Add(Tab.Settings, SettingsTabButton);
            SettingsTabButton.Button.onClick.AddListener(() => ClickSelectTab(Tab.Settings));

            // Element initialization
            ElectionControls.Init(Game);

            // Newspaper
            Newspaper.Init();
            Newspaper.Hide();

            HideAllTabs();
        }

        private void ClickSelectTab(Tab tab)
        {
            if (IsShowingNewspaper) return;
            SelectTab(tab, playClickSound: true);
        }

        private void HideAllTabs()
        {
            foreach (Transform t in SidePanelContentContainer.transform) t.gameObject.SetActive(false);
        }

        #region Update

        private void Update()
        {

        }

        #endregion

        private void ToggleAudio()
        {
            int newLevel = AudioManager.CycleVolumeLevel();

            switch (newLevel)
            {
                case 0:
                    MuteAudioButtonIcon.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/AudioOff");
                    break;
                case 1:
                    MuteAudioButtonIcon.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/AudioOn_1");
                    break;
                case 2:
                    MuteAudioButtonIcon.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/AudioOn_2");
                    break;
                case 3:
                    MuteAudioButtonIcon.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/AudioOn_3");
                    break;
            }

            // Play a click on any non-mute level so there's audible feedback
            if (newLevel != 0) AudioManager.PlayStandardClickSound();
        }

        public void DestroyCurrentGame()
        {
            IsDistrictSelected = false;
            SelectedDistrict = null;
            HelperFunctions.DestroyAllChildredImmediately(OverlayContainer);
        }

        public void SelectTab(Tab tab, bool playClickSound = false)
        {
            if (playClickSound) AudioManager.PlayStandardClickSound();

            TabButtons[ActiveTab].SetSelected(false);
            TabPanels[ActiveTab].SetActive(false);

            ActiveTab = tab;

            TabButtons[ActiveTab].SetSelected(true);
            TabPanels[ActiveTab].SetActive(true);

            if(tab != Tab.DistrictInfo) UnselectDistrict();

            switch (tab)
            {
                case Tab.Policies:
                    PolicySelection.Init(Game.LocalPlayerParty);
                    break;

                case Tab.DistrictList:
                    DistrictList.Init(this, Game.VisibleDistricts.Values.OrderByDescending(x => x.Population).ToList());
                    break;

                case Tab.Parliament:
                    Parliament.Init(Game, Game.Parties);
                    break;
            }
        }

        public void SelectDistrict(District d)
        {
            if (SelectedDistrict == d && SelectedDistrict != null) // Clicking the already selected district or clicking away from district unselects it
            {
                AudioManager.PlayStandardClickSound();

                SelectTab(Tab.DistrictList);
                IsDistrictSelected = false;
            }
            else if (d != null) // Clicking on a district
            {
                AudioManager.PlayStandardClickSound();

                UnselectDistrict();
                SelectedDistrict = d;
                SelectedDistrict.Region.SetAnimatedHighlight(true);
                SelectTab(Tab.DistrictInfo);
                DistrictInfo.Init(d);
                IsDistrictSelected = true;
            }
            else if(d == null && SelectedDistrict != null) // Clicking away when a district has been selected before
            {
                AudioManager.PlayStandardClickSound();

                SelectTab(Tab.DistrictList);
                IsDistrictSelected = false;
            }
            else // Click away when nothing has been selected
            {
                // nothing happens
            }

            // Debug
            /*
            if (SelectedDistrict != null)
            {
                Debug.Log("Coast Ratio: " + SelectedDistrict.Region.CoastRatio);
                Debug.Log("Ocean Coast Ratio: " + SelectedDistrict.Region.OceanCoastRatio);
                Debug.Log("Lake Coast Ratio: " + SelectedDistrict.Region.LakeCoastRatio);
            }
            */
        }

        private void UnselectDistrict()
        {
            if(IsDistrictSelected)
            {
                SelectedDistrict.Region.SetAnimatedHighlight(false);
                SelectedDistrict = null;
                IsDistrictSelected = false;
            }
        }


        public void SlideOutHeader(float slideTime)
        {
            SidePanelHeader.Slide(new Vector2(0, 60), slideTime);
        }
        public void SlideInHeader(float slideTime)
        {
            SidePanelHeader.Slide(new Vector2(0, 0), slideTime);
        }

        public void SlideOutFooter(float slideTime)
        {
            SidePanelFooter.Slide(new Vector2(0, -60), slideTime);
        }
        public void SlideInFooter(float slideTime)
        {
            SidePanelFooter.Slide(new Vector2(0, 0), slideTime);
        }

        // Only move button, not legend
        public void SlideOutMapControls(float slideTime)
        {
            MapControls.OverlayDropdown.GetComponent<UIElement>().Slide(new Vector2(-70, 0), slideTime);
        }   
        public void SlideInMapControls(float slideTime)
        {
            MapControls.OverlayDropdown.GetComponent<UIElement>().Slide(new Vector2(0, 0), slideTime);
        }

        public void SlideOutStandings(float slideTime)
        {
            StandingsPanel.Slide(new Vector2(0, -60), slideTime);
        }
        public void SlideInStandings(float slideTime)
        {
            StandingsPanel.Slide(new Vector2(0, 0), slideTime);
        }


        public void ShowAndHighlightPolicy(Policy policy)
        {
            // Switch to policy screen
            if (ActiveTab != Tab.Policies)
            {
                SelectTab(Tab.Policies, playClickSound: false);
            }

            Tooltip.Instance.Hide();
            PolicySelection.ShowAndHighlight(policy);
        }
    }

    public enum Tab
    {
        DistrictList,
        DistrictInfo,
        Parliament,
        Constitution,
        Events,
        Policies,
        Campaigns,
        Voting,
        Settings
    }
}
