using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_DecliningPopulation : MentalityTrait
    {
        private const int MIN_DECREASE = 80; // thousand
        private const int MAX_DECREASE = 120; // thousand

        private int PopulationDecrease;

        protected override void OnInit()
        {
            PopulationDecrease = Random.Range(MIN_DECREASE, MAX_DECREASE + 1) * 1000;
        }

        public override void OnPostElection()
        {
            District.Population -= PopulationDecrease;
            if (District.Population < 1000) District.Population = 1000;
        }

        public override string Description => $"The population in this district shrinks by {PopulationDecrease} every cycle.";
    }
}
