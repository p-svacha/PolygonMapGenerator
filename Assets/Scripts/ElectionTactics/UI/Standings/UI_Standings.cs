using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics {
    public class UI_Standings : MonoBehaviour
    {
        public GameObject Container;
        public UI_StandingsElement ElementPrefab;

        public void Init(List<Party> parties)
        {
            for (int i = 0; i < Container.transform.childCount; i++) Destroy(Container.transform.GetChild(i).gameObject);

            foreach(Party p in parties.OrderByDescending(x => x.GamePoints))
            {
                UI_StandingsElement elem = Instantiate(ElementPrefab, Container.transform);
                elem.Init(p);
            }
        }

    }
}
