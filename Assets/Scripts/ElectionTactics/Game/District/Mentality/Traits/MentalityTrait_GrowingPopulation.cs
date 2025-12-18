using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_GrowingPopulation : MentalityTrait
    {
        private const int MIN_INCREASE = 80; // thousand
        private const int MAX_INCREASE = 120; // thousand

        private int PopulationIncrease;

        protected override void OnInit()
        {
            PopulationIncrease = Random.Range(MIN_INCREASE, MAX_INCREASE + 1) * 1000;
        }

        public override void OnPostElection()
        {
            District.Population += PopulationIncrease;
        }

        public override string Description => $"The population in this district grows by {PopulationIncrease} every cycle.";
    }
}
