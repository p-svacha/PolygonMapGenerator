using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class RE_Disaster : RandomEvent
    {
        private const float MIN_POPULATION = 100000;
        private const float MIN_CASUALTIES = 0.20f; // in % of district population
        private const float MAX_CASUALTIES = 0.50f; // in % of district population

        public District District { get; private set; }
        public int Casualties { get; private set; }

        protected override void ExecuteEffect()
        {
            var game = ElectionTacticsGame.Instance;
            District = game.ActiveDistricts.Where(d => d.Population > MIN_POPULATION).ToList().RandomElement();
            int casualties = (int)(District.Population * Random.Range(MIN_CASUALTIES, MAX_CASUALTIES));
            Casualties = (casualties / 1000) * 1000; // round to 1000s
            District.Population = Mathf.Max(1000, District.Population - casualties);
        }

        public override bool CanExecute()
        {
            return Game.ActiveDistricts.Any(d => d.Population > MIN_POPULATION);
        }


        public override string GetArticleIconName() => "Fire";
        public override int GetArticlePriority() => 100;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"Catastrophe Strikes {District.Name}",
                $"Disaster Hits {District.Name}",
                $"Tragedy in {District.Name}",
                $"{District.Name} Reels from Calamity",
                $"Dark Day for {District.Name}",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"A natural disaster has struck {District.Name} in the wake of the election, with early reports citing around {Casualties:N0} fatalities.",
                $"Calamity has befallen {District.Name}: officials estimate roughly {Casualties:N0} lives lost.",
                $"In the aftermath of the vote, {District.Name} was hit by catastrophe, leaving an estimated {Casualties:N0} dead.",
            };

            var pool = new List<string>
            {
                "Rescue crews from neighbouring districts have rushed to assist.",
                "The full economic toll will not be known for some time.",
                "Officials have declared a state of emergency across the region.",
                "Residents are picking through the wreckage in search of survivors.",
                $"The local {District.Economy1.Label.ToLower()} sector has been hit especially hard.",
                "Relief funds are being organized in the capital.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }


    }
}
