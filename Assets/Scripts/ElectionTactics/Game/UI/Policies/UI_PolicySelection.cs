using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_PolicySelection : MonoBehaviour
    {
        public PolicyControl PolicyControlPrefab;
        public VerticalLayoutGroup VLG;

        public GameObject GeographyContainer;
        public GameObject EconomyContainer;
        public GameObject DensityContainer;
        public GameObject AgeGroupContainer;
        public GameObject LanguageContainer;
        public GameObject ReligionContainer;
        public GameObject DistrictContainer;

        public Dictionary<PolicyType, GameObject> PolicyContainers = new Dictionary<PolicyType, GameObject>();

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Init(Party p)
        {
            if(PolicyContainers.Count == 0)
            {
                PolicyContainers.Add(PolicyType.Geography, GeographyContainer);
                PolicyContainers.Add(PolicyType.Economy, EconomyContainer);
                PolicyContainers.Add(PolicyType.Density, DensityContainer);
                PolicyContainers.Add(PolicyType.AgeGroup, AgeGroupContainer);
                PolicyContainers.Add(PolicyType.Language, LanguageContainer);
                PolicyContainers.Add(PolicyType.Religion, ReligionContainer);
                PolicyContainers.Add(PolicyType.District, DistrictContainer);
            }

            foreach(GameObject container in PolicyContainers.Values)
            {
                for (int i = 1; i < container.transform.childCount; i++) Destroy(container.transform.GetChild(i).gameObject);
            }

            foreach(Policy policy in p.ActivePolicies.OrderBy(x => x.SortingOrder))
            {
                AddPolicyControl(policy);
            }

            Canvas.ForceUpdateCanvases();
            VLG.SetLayoutVertical();
        }

        private void AddPolicyControl(Policy p)
        {
            PolicyControl pc = Instantiate(PolicyControlPrefab, PolicyContainers[p.Type].transform);
            pc.GetComponent<RectTransform>().sizeDelta = new Vector2(pc.GetComponent<RectTransform>().sizeDelta.x, 40);
            pc.Init(p);
        }
    }
}
