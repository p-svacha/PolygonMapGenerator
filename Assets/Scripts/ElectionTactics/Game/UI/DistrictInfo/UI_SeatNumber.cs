using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_SeatNumber : MonoBehaviour
    {
        [Header("Elements")]
        public Image Frame;
        public TextMeshProUGUI Text;
        public TooltipTarget TooltipTarget;

        public void InitDistrictSeats(int seats, SeatAllocationMethodDef seatAllocationmethod, bool darkMode = false)
        {
            SetValue(seats);
            Frame.sprite = GetSprite(seatAllocationmethod, darkMode);
            Frame.color = Color.white;
            Text.color = darkMode ? Color.white : Color.black;

            string tooltipDesc = "How many seats this district is worth.\n\n" + seatAllocationmethod.Description;
            TooltipTarget.Init(Tooltip.TooltipType.TitleAndText, "Seats", tooltipDesc);
        }

        public void InitPartySeats(Party p, string value)
        {
            SetValue(value);
            // SeatNumber.SetColor(p.Color, p.ContrastColor);
            SetColor(new Color(0.45f, 0.45f, 0.45f), Color.white);

            string tooltipDesc = "How many seats the party holds in the parliament.";
            TooltipTarget.Init(Tooltip.TooltipType.TitleAndText, "Seats", tooltipDesc);
        }

        public void SetValue(int value) => SetValue(value.ToString());
        public void SetValue(string value) => Text.text = value;

        public void SetFrame(Sprite sprite)
        {
            Frame.sprite = sprite;
        }

        public void SetColor(Color frameColor, Color textColor)
        {
            Frame.sprite = ResourceManager.LoadSprite("ElectionTactics/Icons/Circle_White");
            Frame.color = frameColor;
            Text.color = textColor;
        }
       

        private Sprite GetSprite(SeatAllocationMethodDef method, bool darkMode)
        {
            string baseName = darkMode ? "Circle" : "Circle_White";
            if (method == SeatAllocationMethodDefOf.WinnerTakesAll) return ResourceManager.LoadSprite($"ElectionTactics/Icons/{baseName}");
            if (method == SeatAllocationMethodDefOf.DHondtPR) return ResourceManager.LoadSprite($"ElectionTactics/Icons/{baseName}_Striped_1");
            if (method == SeatAllocationMethodDefOf.HamiltonPR) return ResourceManager.LoadSprite($"ElectionTactics/Icons/{baseName}_Striped_2");
            throw new System.Exception("method not handledl");
        }
    }
}
