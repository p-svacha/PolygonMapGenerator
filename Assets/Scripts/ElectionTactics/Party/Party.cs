using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics {
    public class Party : MonoBehaviour
    {
        public string Name;
        public Color Color;

        public Dictionary<GeographyTrait, int> GeographyPolicies = new Dictionary<GeographyTrait, int>();
        public Dictionary<Density, int> DensityPolicies = new Dictionary<Density, int>();
        public Dictionary<Language, int> LanguagePolicies = new Dictionary<Language, int>();
        public Dictionary<Religion, int> ReligionPolicies = new Dictionary<Religion, int>();
        public Dictionary<AgeGroup, int> AgeGroupPolicies = new Dictionary<AgeGroup, int>();
        public Dictionary<EconomyTrait, int> EconomyPolicies = new Dictionary<EconomyTrait, int>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Add Policies

        public void AddGeographyPolicy(GeographyTrait t)
        {
            GeographyPolicies.Add(t, 0);
        }
        
        public void AddDensityPolicy(Density d)
        {
            DensityPolicies.Add(d, 0);
        }

        public void AddLanguagePolicy(Language l)
        {
            LanguagePolicies.Add(l, 0);
        }

        public void AddReligionPolicy(Religion r)
        {
            ReligionPolicies.Add(r, 0);
        }

        public void AddAgeGroupPolicy(AgeGroup a)
        {
            AgeGroupPolicies.Add(a, 0);
        }

        public void AddEconomyPolicy(EconomyTrait e)
        {
            EconomyPolicies.Add(e, 0);
        }

        #endregion
    }
}
