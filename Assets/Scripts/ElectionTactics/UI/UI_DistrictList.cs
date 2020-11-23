using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_DistrictList : MonoBehaviour
    {
        public UI_DistrictListElement ListElementPrefab;

        public GameObject ListContainer;

        public void Init(UI_ElectionTactics UI, List<District> districts)
        {
            for (int i = 0; i < ListContainer.transform.childCount; i++) Destroy(ListContainer.transform.GetChild(i).gameObject);

            foreach (District d in districts)
            {
                UI_DistrictListElement elem = Instantiate(ListElementPrefab, ListContainer.transform);
                elem.Init(UI, d);
            }
        }


    }
}
