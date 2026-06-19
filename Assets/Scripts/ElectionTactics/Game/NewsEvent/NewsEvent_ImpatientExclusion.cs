using ElectionTactics;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_ImpatientExclusion : NewsEvent
    {
        public District District { get; private set; }
        public Party Party { get; private set; }

        public NewsEvent_ImpatientExclusion(District district, Party party) : base()
        {
            District = district;
            Party = party;
        }


        public override string GetArticleIconName() => "Exclusion";
        public override int GetArticlePriority() => 60;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{Party.Acronym} Barred from {District.Name}",
                $"{District.Name} Shuts Out {Party.Acronym}",
                $"Enough: {District.Name} Turns on {Party.Acronym}",
                $"{Party.Acronym} Loses Its Place in {District.Name}",
                $"{District.Name} Has Had Its Fill of {Party.Acronym}",
            };
            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"After failing to win a single seat in {District.Name} for {CT_Impatient.THRESHOLD} consecutive cycles, {Party.Name} has been permanently barred from standing there.",
                $"Voters in {District.Name} have lost all patience with {Party.Name}: after {CT_Impatient.THRESHOLD} fruitless elections, the party is excluded from future ballots.",
                $"{Party.Name} has been shut out of {District.Name} for good, having failed to secure a seat there across {CT_Impatient.THRESHOLD} cycles running.",
            };

            var pool = new List<string>
            {
                $"The district's Impatient character has long made clear that repeated failure carries consequences.",
                $"Party officials have not yet commented on what they plan to do next.",
                $"Rivals are expected to move quickly to absorb {Party.Acronym}'s former supporters.",
                $"It is a humbling end to what was once a more competitive presence in the district.",
                $"Whether {Party.Acronym} can recover elsewhere remains to be seen.",
                $"Local observers say the writing had been on the wall for some time.",
            };

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
