using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_ElectionTactics : MonoBehaviour
    {
        public ElectionTacticsGame Game;

        public Color HeaderColor;
        public Color PanelColor;

        public TabButton PolicyTabButton;
        public TabButton DistrictTabButton;

        public Tab ActiveTab;
        public Dictionary<Tab, GameObject> TabPanels = new Dictionary<Tab, GameObject>();
        public Dictionary<Tab, TabButton> TabButtons = new Dictionary<Tab, TabButton>();

        public UI_DistrictInfo DistrictInfo;
        public UI_PolicySelection PolicySelection;

        void Start()
        {
            TabPanels.Add(Tab.Policies, PolicySelection.gameObject);
            TabButtons.Add(Tab.Policies, PolicyTabButton);

            TabPanels.Add(Tab.Districts, DistrictInfo.gameObject);
            TabButtons.Add(Tab.Districts, DistrictTabButton);

            foreach (KeyValuePair<Tab, TabButton> kvp in TabButtons) kvp.Value.Button.onClick.AddListener(() => SelectTab(kvp.Key));
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
                    PolicySelection.Init(Game.PlayerParty);
                    break;
            }
        }

        public void SelectDistrict(District d)
        {
            SelectTab(Tab.Districts);
            DistrictInfo.SetDistrict(d);
        }
    }

    public enum Tab
    {
        Policies,
        Districts,
    }
}
