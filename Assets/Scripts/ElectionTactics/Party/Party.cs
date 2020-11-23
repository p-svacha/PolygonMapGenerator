using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics {
    public class Party
    {
        public string Name;
        public string Acronym;
        public Color Color;

        public List<Policy> Policies = new List<Policy>();

        public int Seats;

        public Party(string name, Color c)
        {
            Name = name;
            Acronym = "";
            string[] words = name.Split(' ');
            foreach (string w in words) Acronym += (w[0] + "").ToUpper(); 
            Color = c;
        }

        public void AddPolicy(Policy p)
        {
            Policies.Add(p);
        }

        public int GetPolicyValueFor(GeographyTrait t)
        {
            return Policies.OfType<GeographyPolicy>().First(x => x.Trait == t).Value;
        }
        public int GetPolicyValueFor(EconomyTrait t)
        {
            return Policies.OfType<EconomyPolicy>().First(x => x.Trait == t).Value;
        }
        public int GetPolicyValueFor(Density d)
        {
            return Policies.OfType<DensityPolicy>().First(x => x.Density == d).Value;
        }
        public int GetPolicyValueFor(AgeGroup a)
        {
            return Policies.OfType<AgeGroupPolicy>().First(x => x.AgeGroup == a).Value;
        }
        public int GetPolicyValueFor(Language l)
        {
            return Policies.OfType<LanguagePolicy>().First(x => x.Language == l).Value;
        }
        public int GetPolicyValueFor(Religion r)
        {
            return Policies.OfType<ReligionPolicy>().First(x => x.Religion == r).Value;
        }
    }
}
