using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_ElectionTactics : MonoBehaviour
    {
        public ElectionTacticsGame Game;

        public Color HeaderColor;
        public Color PanelColor;

        public TabButton ParliamentTabButton;
        public TabButton PolicyTabButton;
        public TabButton DistrictTabButton;

        public Tab ActiveTab;
        public Dictionary<Tab, GameObject> TabPanels = new Dictionary<Tab, GameObject>(); // Which tab is connected to which object
        public Dictionary<Tab, TabButton> TabButtons = new Dictionary<Tab, TabButton>(); // Which tab is part of which button

        public UI_Parliament Parliament;
        public UI_DistrictList DistrictList;
        public UI_DistrictInfo DistrictInfo;
        public UI_PolicySelection PolicySelection;

        public District SelectedDistrict;
        public Color SelectedDistrictColor = Color.red;

        void Start()
        {
            TabPanels.Add(Tab.Policies, PolicySelection.gameObject);
            TabButtons.Add(Tab.Policies, PolicyTabButton);
            PolicyTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Policies));

            TabPanels.Add(Tab.DistrictList, DistrictList.gameObject);
            TabButtons.Add(Tab.DistrictList, DistrictTabButton);
            DistrictTabButton.Button.onClick.AddListener(() => SelectTab(Tab.DistrictList));

            TabPanels.Add(Tab.DistrictInfo, DistrictInfo.gameObject);
            TabButtons.Add(Tab.DistrictInfo, DistrictTabButton);

            TabPanels.Add(Tab.Parliament, Parliament.gameObject);
            TabButtons.Add(Tab.Parliament, ParliamentTabButton);
            ParliamentTabButton.Button.onClick.AddListener(() => SelectTab(Tab.Parliament));
        }

        public void SelectTab(Tab tab)
        {
            TabButtons[ActiveTab].Background.color = HeaderColor;
            TabPanels[ActiveTab].SetActive(false);

            ActiveTab = tab;

            TabButtons[ActiveTab].Background.color = PanelColor;
            TabPanels[ActiveTab].SetActive(true);

            switch(tab)
            {
                case Tab.Policies:
                    UnselectDistrict();
                    PolicySelection.Init(Game.PlayerParty);
                    break;

                case Tab.DistrictList:
                    UnselectDistrict();
                    DistrictList.Init(this, Game.Districts.Values.OrderByDescending(x => x.Population).ToList());
                    break;

                case Tab.Parliament:
                    UnselectDistrict();
                    Parliament.Init(Game.Parties);
                    break;
            }
        }

        public void SelectDistrict(District d)
        {
            if (SelectedDistrict == d && SelectedDistrict != null) // Clicking the already selected district or clicking away from district unselects it
            {
                SelectTab(Tab.DistrictList);
            }
            else if (d != null) // Clicking on a district
            {
                if (SelectedDistrict != null) SelectedDistrict.Region.Unhighlight();
                SelectedDistrict = d;
                SelectedDistrict.Region.Highlight(SelectedDistrictColor);
                SelectTab(Tab.DistrictInfo);
                DistrictInfo.Init(d);
            }
            else if(d == null && SelectedDistrict != null) // Clicking away when a dsitrict has been selected before
            {
                SelectTab(Tab.DistrictList);
            }
            else // Click away when nothing has been selected
            {
                // nothing happens
            }
        }

        private void UnselectDistrict()
        {
            if(SelectedDistrict != null)
            {
                SelectedDistrict.Region.Unhighlight();
                SelectedDistrict = null;
            }
        }
    }

    public enum Tab
    {
        Parliament,
        Policies,
        DistrictList,
        DistrictInfo
    }
}
