using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_NewspaperButton : MonoBehaviour
    {
        private Newspaper Newspaper;

        [Header("Elements")]
        public Button Button;
        public TextMeshProUGUI YearText;

        public void Init(Newspaper newspaper)
        {
            Newspaper = newspaper;
            YearText.text = Newspaper.Year.ToString();
            Button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            UI_ElectionTactics.Instance.Newspaper.ShowNewspaper(Newspaper);
        }
    }
}
