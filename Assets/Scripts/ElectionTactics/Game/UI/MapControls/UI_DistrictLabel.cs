using System.Collections;
using System.Collections.Generic;
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
        public Text SeatsText;
        public Text NameText;
        public Image ReligionIcon;
        public Image LanguageIcon;
        public Image DensityIcon;
        public Text PopularityText;
        public Text MarginText;

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
            Refresh(DistrictLabelMode.Default);
        }

        public void Refresh(DistrictLabelMode mode)
        {
            SeatsText.text = District.Seats.ToString();
            NameText.text = District.Name;
            LanguageIcon.sprite = IconManager.Singleton.GetLanguageIcon(District.Language);
            DensityIcon.sprite = IconManager.Singleton.GetDensityIcon(District.Density);
            ReligionIcon.gameObject.SetActive(District.Religion != Religion.None);
            if (District.Religion != Religion.None) ReligionIcon.sprite = IconManager.Singleton.GetReligionIcon(District.Religion);

            switch(mode)
            {
                case DistrictLabelMode.Default:
                    PopularityText.text = District.GetPartyPopularity(District.Game.LocalPlayerParty).ToString();
                    if (District.CurrentWinnerParty != null)
                    {
                        SetBackgroundColor(District.CurrentWinnerParty.Color);
                        MarginText.text = District.GetLatestElectionResult().GetMargin(District.Game.LocalPlayerParty);
                    }
                    else
                    {
                        SetBackgroundColor(Color.white);
                        MarginText.text = "";
                    }
                    break;

                case DistrictLabelMode.InElection:
                    DistrictElectionResult currentElectionResult = District.GetLatestElectionResult();
                    if(currentElectionResult != null ) PopularityText.text = currentElectionResult.PartyPopularities[District.Game.LocalPlayerParty].ToString();

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
            MarginText.text = marginText;
            LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundBot.GetComponent<RectTransform>());
        }
    }
}
