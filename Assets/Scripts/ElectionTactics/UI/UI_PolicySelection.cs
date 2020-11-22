using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_PolicySelection : MonoBehaviour
    {
        public PolicyControl PolicyControlPrefab;

        public GameObject GeographyContainer;
        public GameObject EconomyContainer;
        public GameObject DensityContainer;
        public GameObject AgeGroupContainer;
        public GameObject LanguageContainer;
        public GameObject ReligionContainer;
        

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
            for (int i = 1; i < GeographyContainer.transform.childCount; i++) Destroy(GeographyContainer.transform.GetChild(i).gameObject);
            for (int i = 1; i < EconomyContainer.transform.childCount; i++) Destroy(EconomyContainer.transform.GetChild(i).gameObject);
            for (int i = 1; i < DensityContainer.transform.childCount; i++) Destroy(DensityContainer.transform.GetChild(i).gameObject);
            for (int i = 1; i < AgeGroupContainer.transform.childCount; i++) Destroy(AgeGroupContainer.transform.GetChild(i).gameObject);
            for (int i = 1; i < LanguageContainer.transform.childCount; i++) Destroy(LanguageContainer.transform.GetChild(i).gameObject);
            for (int i = 1; i < ReligionContainer.transform.childCount; i++) Destroy(ReligionContainer.transform.GetChild(i).gameObject);

            foreach(KeyValuePair<GeographyTrait, int> kvp in p.GeographyPolicies)
                AddPolicyControl(GeographyContainer.transform, kvp.Key.ToString(), kvp.Value);
            foreach (KeyValuePair<EconomyTrait, int> kvp in p.EconomyPolicies)
                AddPolicyControl(EconomyContainer.transform, kvp.Key.ToString(), kvp.Value);
            foreach (KeyValuePair<Density, int> kvp in p.DensityPolicies)
                AddPolicyControl(DensityContainer.transform, kvp.Key.ToString(), kvp.Value);
            foreach (KeyValuePair<AgeGroup, int> kvp in p.AgeGroupPolicies)
                AddPolicyControl(AgeGroupContainer.transform, kvp.Key.ToString(), kvp.Value);
            foreach (KeyValuePair<Language, int> kvp in p.LanguagePolicies)
                AddPolicyControl(LanguageContainer.transform, kvp.Key.ToString(), kvp.Value);
            foreach (KeyValuePair<Religion, int> kvp in p.ReligionPolicies)
                AddPolicyControl(ReligionContainer.transform, kvp.Key.ToString(), kvp.Value);
        }

        private void AddPolicyControl(Transform parent, string label, int value)
        {
            PolicyControl pc = Instantiate(PolicyControlPrefab, parent);
            pc.GetComponent<RectTransform>().sizeDelta = new Vector2(pc.GetComponent<RectTransform>().sizeDelta.x, 40);
            pc.Init(label, value);
        }
    }
}
