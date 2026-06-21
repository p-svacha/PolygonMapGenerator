using UnityEngine;
using UnityEngine.EventSystems;

namespace ElectionTactics
{
    public class UI_DistrictLabelClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private District district;

        public void Init(District d)
        {
            district = d;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ElectionTacticsGame.Instance.State != GameState.PreparationPhase) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                district.Game.UI.SelectDistrict(district);
            }
        }
    }
}