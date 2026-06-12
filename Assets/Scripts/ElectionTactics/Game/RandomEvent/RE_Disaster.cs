using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_Disaster : RandomEvent
    {
        private const float MIN_POPULATION = 100000;
        private const float MIN_CASUALTIES = 0.05f; // in % of district population
        private const float MAX_CASUALTIES = 0.15f; // in % of district population

        protected override void ExecuteEffect()
        {
            var game = ElectionTacticsGame.Instance;
            District d = game.ActiveDistricts.Where(d => d.Population > MIN_POPULATION).ToList().RandomElement();
            int casualties = (int)(d.Population * Random.Range(MIN_CASUALTIES, MAX_CASUALTIES));
            casualties = (casualties / 1000) * 1000; // round to 1000s
            d.Population = Mathf.Max(1000, d.Population - casualties);

            ArticleHeadline = $"Catastrophe strikes {d.Name}";
            ArticleBody = $"A natural disaster has hit {d.Name} right after the election. " +
                            $"Reports suggest around {casualties:N0} fatalities.";
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Any(d => d.Population > MIN_POPULATION);
        }
    }
}
