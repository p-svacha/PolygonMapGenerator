using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictListElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Text NameText;
        public Text SeatsText;
        public Text MarginText;
        public Image PartyIcon;

        public District District;


        public void Init(UI_ElectionTactics UI, District d)
        {
            District = d;
            NameText.text = d.Name;
            SeatsText.text = d.Seats.ToString();
            if(d.CurrentWinnerParty != null)
            {
                PartyIcon.gameObject.SetActive(true);
                PartyIcon.color = d.CurrentWinnerParty.Color;
                float margin = d.GetLatestElectionResult().GetMargin(UI.Game.PlayerParty);
                MarginText.text = (margin > 0 ? "+" : "") + margin.ToString("0.0") + " %";
            }
            else
            {
                PartyIcon.gameObject.SetActive(false);
                MarginText.text = "";
            }

            GetComponent<Button>().onClick.AddListener(() => { d.Region.SetAnimatedHighlight(false); UI.SelectDistrict(d); });
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            District.Region.SetAnimatedHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            District.Region.SetAnimatedHighlight(false);
        }
    }
}
