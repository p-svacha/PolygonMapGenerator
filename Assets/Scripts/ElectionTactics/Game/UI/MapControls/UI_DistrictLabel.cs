using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictLabel : MonoBehaviour
    {
        public District District;
        public Image Background;
        public Image BackgroundTop;
        public Image BackgroundBot;
        public UI_SeatNumber Seats;
        public TextMeshProUGUI NameText;
        public Image ReligionIcon;
        public Image LanguageIcon;
        public Image DensityIcon;
        public TextMeshProUGUI PopularityText;
        public GameObject MarginContainer;
        public TextMeshProUGUI MarginText;

        // State
        public DistrictLabelMode Mode { get; private set; }
        public int PopularityImpact { get; private set; } // If not 0, this will show next to the popularity as +/- the value.

        public void Update()
        {
            transform.position = Camera.main.WorldToScreenPoint(new Vector3(District.Region.CenterPoi.x, 0.01f, District.Region.CenterPoi.y));
            float scale = 1f / Camera.main.transform.position.y;
            if (scale < 0.5f) scale = 0.5f;
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void Init(District district)
        {
            District = district;
            GetComponent<UI_DistrictLabelClickHandler>().Init(district);
            Refresh(DistrictLabelMode.Default);
        }

        public void Refresh() => Refresh(Mode);
        public void Refresh(DistrictLabelMode mode)
        {
            Mode = mode;

            NameText.text = District.Name;
            LanguageIcon.sprite = IconManager.Singleton.GetLanguageIcon(District.Language);
            DensityIcon.sprite = IconManager.Singleton.GetDensityIcon(District.Density);
            ReligionIcon.gameObject.SetActive(District.Religion != ReligionDefOf.None);
            if (District.Religion != ReligionDefOf.None) ReligionIcon.sprite = IconManager.Singleton.GetReligionIcon(District.Religion);

            // Seats
            // Debug.Log($"Refreshing seats ({District.Name}): Current number: {District.Seats}");
            int numSeats = District.GetSeats();

            if (ElectionTacticsGame.Instance.State == GameState.Election) // Show seats of previous cycle when in election
            {
                DistrictElectionResult latestResult = ElectionTacticsGame.Instance.GetLatestElectionResult().GetDistrictResult(District);
                if (latestResult != null)
                {
                    // Debug.Log($"Number from prev election ({District.Name}): {latestResult.Seats}");
                    numSeats = latestResult.Seats;
                }
            }
            Seats.InitDistrictSeats(numSeats, District.GetSeatAllocationMethod(), darkMode: true);


            // Dynamic tooltips
            ReligionIcon.GetComponent<TooltipTarget>().Init(District.Religion.Label, "The religion of this district.");
            LanguageIcon.GetComponent<TooltipTarget>().Init(District.Language.Label, "The language spoken in this districts.");

            switch(mode)
            {
                case DistrictLabelMode.Default:

                    string popularityLabel = District.GetPartyPopularity(District.Game.LocalPlayerParty).ToString();
                    if (PopularityImpact > 0) popularityLabel += "+" + PopularityImpact.ToString();
                    else if (PopularityImpact < 0) popularityLabel += PopularityImpact.ToString();
                    PopularityText.text = popularityLabel;


                    if (District.CurrentWinnerParty != null)
                    {
                        MarginContainer.gameObject.SetActive(true);
                        SetBackgroundColor(District.CurrentWinnerParty.Color);
                        MarginText.text = District.GetLatestElectionResult().GetMargin(District.Game.LocalPlayerParty);
                    }
                    else
                    {
                        MarginContainer.gameObject.SetActive(false);
                        SetBackgroundColor(Color.white);
                        MarginText.text = "";
                    }
                    break;

                case DistrictLabelMode.InElection:
                    DistrictElectionResult currentElectionResult = District.GetLatestElectionResult();
                    if (currentElectionResult != null ) PopularityText.text = currentElectionResult.PartyPopularities[District.Game.LocalPlayerParty].ToString();

                    MarginContainer.gameObject.SetActive(false);
                    SetBackgroundColor(Color.white);
                    MarginText.text = "";
                    break;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundTop.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundBot.GetComponent<RectTransform>());
        }

        public void SetBackgroundColor(Color c)
        {
            c = ColorManager.Instance.Lighter(c);
            Background.color = c;
            BackgroundTop.color = c;
            BackgroundBot.color = c;
        }

        public void SetMargin(string marginText)
        {
            MarginContainer.gameObject.SetActive(true);
            MarginText.text = marginText;
            LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundBot.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Calling this will show the popularity impact for the local player of a single point of the given policy in the popularity box with a +/- sign. For example instead of "20" it will show "20 + 5".
        /// </summary>
        public void ShowPolicyImpact(Policy p)
        {
            PopularityImpact = p.GetSinglePointPopularityDelta(District);
            Refresh();
        }
        public void HidePolicyImpact()
        {
            PopularityImpact = 0;
            Refresh();
        }
    }
}
