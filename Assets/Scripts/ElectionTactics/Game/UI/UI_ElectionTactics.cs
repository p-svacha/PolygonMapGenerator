using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_ElectionTactics : MonoBehaviour
    {
        public ElectionTacticsGame Game;
        public Font Font;

        [Header("Screens")]
        public UI_LoadingScreen LoadingScreen;
        public UI_PostGameScreen PostGameScreen;
        
        [Header ("Map Controls")]
        public MapControls MapControls;
        public UI_DistrictLabel DistrictLabelPrefab;

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
        public UI_DistrictList DistrictList;
        public UI_DistrictInfo DistrictInfo;
        public UI_Parliament Parliament;
        public UI_Constitution Constitution;
        public UI_Events Events;
        public UI_PolicySelection PolicySelection;
        public UI_Campaigns Campaigns;
        public UI_Voting Voting;
        public UI_Settings Settings;

        public bool IsDistrictSelected;
        public District SelectedDistrict;


        public void Init(ElectionTacticsGame game)
        {
            Game = game;

            // Listeners
            TabPanels.Add(Tab.DistrictList, DistrictList.gameObject);
            TabButtons.Add(Tab.DistrictList, DistrictTabButton);
            DistrictTabButton.Button.onClick.AddListener(() => SelectTab(Tab.DistrictList));

            TabPanels.Add(Tab.DistrictInfo, DistrictInfo.gameObject);
            TabButtons.Add(Tab.DistrictInfo, DistrictTabButton);

            TabPanels.Add(Tab.Parliament, Parliament.gameObject);
            TabButtons.Add(Tab.Parliament, ParliamentTabButton);
            ParliamentTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Parliament));

            TabPanels.Add(Tab.Constitution, Constitution.gameObject);
            TabButtons.Add(Tab.Constitution, ConstitutionTabButton);
            ConstitutionTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Constitution));

            TabPanels.Add(Tab.Events, Events.gameObject);
            TabButtons.Add(Tab.Events, EventsTabButton);
            EventsTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Events));

            TabPanels.Add(Tab.Policies, PolicySelection.gameObject);
            TabButtons.Add(Tab.Policies, PoliciesTabButton);
            PoliciesTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Policies));

            TabPanels.Add(Tab.Campaigns, Campaigns.gameObject);
            TabButtons.Add(Tab.Campaigns, CampaignsTabButton);
            CampaignsTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Campaigns));

            TabPanels.Add(Tab.Voting, Voting.gameObject);
            TabButtons.Add(Tab.Voting, VotingTabButton);
            VotingTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Voting));

            TabPanels.Add(Tab.Settings, Settings.gameObject);
            TabButtons.Add(Tab.Settings, SettingsTabButton);
            SettingsTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Settings));

            // Element initialization
            ElectionControls.Init(Game);
        }

        #region Update

        private void Update()
        {

        }

        #endregion

        public void SelectTab(Tab tab)
        {
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
                SelectTab(Tab.DistrictList);
                IsDistrictSelected = false;
            }
            else if (d != null) // Clicking on a district
            {
                UnselectDistrict();
                SelectedDistrict = d;
                SelectedDistrict.Region.SetAnimatedHighlight(true);
                SelectTab(Tab.DistrictInfo);
                DistrictInfo.Init(d);
                IsDistrictSelected = true;
            }
            else if(d == null && SelectedDistrict != null) // Clicking away when a district has been selected before
            {
                SelectTab(Tab.DistrictList);
                IsDistrictSelected = false;
            }
            else // Click away when nothing has been selected
            {
                // nothing happens
            }
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
