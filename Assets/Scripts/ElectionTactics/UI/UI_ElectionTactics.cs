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

        public PostGameScreen PostGameScreen;
        
        // Map Controls
        public MapControls MapControls;
        public UI_DistrictLabel DistrictLabelPrefab;

        // Standings
        public UI_Standings Standings;

        [Header ("Header")]
        public Image Header;
        public Image ElectionPanel;
        public Button ElectionButton;
        public Text PPText;
        public TabButton ParliamentTabButton;
        public TabButton PolicyTabButton;
        public TabButton DistrictTabButton;
        public Tab ActiveTab;
        public Dictionary<Tab, GameObject> TabPanels = new Dictionary<Tab, GameObject>(); // Which tab is connected to which object
        public Dictionary<Tab, TabButton> TabButtons = new Dictionary<Tab, TabButton>(); // Which tab is part of which button

        [Header("Side Panel")]
        public Image SidePanel;
        public UI_Parliament Parliament;
        public UI_DistrictList DistrictList;
        public UI_DistrictInfo DistrictInfo;
        public UI_PolicySelection PolicySelection;
        public District SelectedDistrict;

        // Header Animation
        private bool IsHeaderMoving;
        private Vector2 HeaderSourcePos;
        private Vector2 HeaderTargetPos;
        private float HeaderSlideTime;
        private float HeaderSlideDelay;

        void Start()
        {
            // Colors
            Header.color = ColorManager.Colors.UiHeaderColor;
            ElectionPanel.color = ColorManager.Colors.UiSpecialColor;
            SidePanel.color = ColorManager.Colors.UiMainColor;

            // Listeners
            ElectionButton.onClick.AddListener(() => Game.RunGeneralElection());

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

        #region Update

        private void Update()
        {
            if(IsHeaderMoving)
            {
                if(HeaderSlideDelay >= HeaderSlideTime)
                {
                    IsHeaderMoving = false;
                    Header.GetComponent<RectTransform>().anchoredPosition = HeaderTargetPos;
                }
                else
                {
                    float r = HeaderSlideDelay / HeaderSlideTime;
                    Header.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(HeaderSourcePos, HeaderTargetPos, r);
                    HeaderSlideDelay += Time.deltaTime;
                }
            }
        }

        #endregion

        public void SelectTab(Tab tab)
        {
            TabButtons[ActiveTab].SetSelected(false);
            TabPanels[ActiveTab].SetActive(false);

            ActiveTab = tab;

            TabButtons[ActiveTab].SetSelected(true);
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
                SelectedDistrict.Region.Highlight(ColorManager.Colors.SelectedDistrictColor);
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

        public void SlideOutHeader(float time)
        {
            HeaderSlideTime = time;
            HeaderSlideDelay = 0f;
            IsHeaderMoving = true;
            HeaderSourcePos = Header.GetComponent<RectTransform>().anchoredPosition;
            HeaderTargetPos = new Vector2(0, 60);
        }
        public void SlideInHeader(float time)
        {
            HeaderSlideTime = time;
            HeaderSlideDelay = 0f;
            IsHeaderMoving = true;
            HeaderSourcePos = Header.GetComponent<RectTransform>().anchoredPosition;
            HeaderTargetPos = new Vector2(0, 0);
        }

        public void UpdatePolicyPointDisplay()
        {
            PPText.text = Game.PlayerParty.PolicyPoints.ToString();
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
