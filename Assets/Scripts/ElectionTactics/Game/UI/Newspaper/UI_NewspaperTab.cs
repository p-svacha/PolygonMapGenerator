using UnityEngine;

namespace ElectionTactics
{
    public class UI_NewspaperTab : MonoBehaviour
    {
        private const int NEWSPAPERS_PER_ROW = 4;

        [Header("Elements")]
        public GameObject Container;

        [Header("Prefabs")]
        public GameObject RowPrefab;
        public UI_NewspaperButton NewspaperButtonPrefab;

        public void Show()
        {
            HelperFunctions.DestroyAllChildredImmediately(Container);

            int counter = 0;
            GameObject currentRow = null;
            foreach (Newspaper newspaper in ElectionTacticsGame.Instance.Newspapers)
            {
                if (counter % NEWSPAPERS_PER_ROW == 0)
                {
                    currentRow = GameObject.Instantiate(RowPrefab, Container.transform);
                    HelperFunctions.DestroyAllChildredImmediately(currentRow);
                }

                UI_NewspaperButton elem = GameObject.Instantiate(NewspaperButtonPrefab, currentRow.transform);
                elem.Init(newspaper);

                counter++;
            }
        }
    }
}
