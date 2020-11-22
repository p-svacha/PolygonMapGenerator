using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class District
    {
        public string Name;
        public Region Region;

        public List<GeographyTrait> Geography = new List<GeographyTrait>();
        public Language Language;
        public Religion Religion;
        public Density Density;
        public AgeGroup AgeGroup;
        public EconomyTrait Economy1;
        public EconomyTrait Economy2;
        public EconomyTrait Economy3;
        public List<CultureTrait> CultureTraits = new List<CultureTrait>();

        public District(Region r)
        {
            Region = r;
            Name = MarkovChainWordGenerator.GetRandomName(10);
            SetGeographyTraits();
            Language = ElectionTacticsGame.GetRandomLanguage();
            Religion = ElectionTacticsGame.GetRandomReligion();
            Density = ElectionTacticsGame.GetRandomDensity();
            AgeGroup = ElectionTacticsGame.GetRandomAgeGroup();
            Economy1 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy2 == Economy1) Economy2 = ElectionTacticsGame.GetRandomEconomyTrait();
            Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(Economy3 == Economy2 || Economy3 == Economy1) Economy3 = ElectionTacticsGame.GetRandomEconomyTrait();
            while(CultureTraits.Count < 3)
            {
                CultureTrait c = ElectionTacticsGame.GetRandomCulture();
                if (!CultureTraits.Contains(c)) CultureTraits.Add(c);
            }
        }

        private void SetGeographyTraits()
        {
            if (Region.IsNextToWater) Geography.Add(GeographyTrait.Coastal);
            else Geography.Add(GeographyTrait.Landlocked);
            if (Region.Area < 0.18f) Geography.Add(GeographyTrait.Tiny);
            if (Region.Area > 1f) Geography.Add(GeographyTrait.Vast);
        }
    }
}
