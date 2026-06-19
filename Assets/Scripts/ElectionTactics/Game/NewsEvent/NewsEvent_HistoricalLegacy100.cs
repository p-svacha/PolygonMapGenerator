using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_HistoricalLegacy100 : NewsEvent
    {
        public District District { get; private set; }
        public Party Party { get; private set; }

        public NewsEvent_HistoricalLegacy100(District district, Party party) : base()
        {
            District = district;
            Party = party;
        }

        public override string GetArticleIconName() => "Smiley";
        public override int GetArticlePriority() => 50;

        public override string GetArticleHeadline()
        {
            List<string> templates = new List<string>()
            {
                $"{Party.Acronym}'s Dynasty in {District.Name}",
                $"A Century of {Party.Acronym}",
                $"{District.Name}: {Party.Acronym} Stronghold",
                $"Unshakeable: {Party.Acronym} in {District.Name}",
                $"{Party.Acronym} Owns {District.Name}",
            };

            return templates.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> milestone = new List<string>()
            {
                $"Through years of unbroken victories in {District.Name}, {Party.Name} has built a historical legacy now valued at 100.",
                $"{Party.Name}'s grip on {District.Name} has reached a milestone: a legacy bonus of 100, the fruit of relentless winning.",
                $"In {District.Name}, no party comes close to {Party.Name}, whose historical legacy has climbed to a remarkable 100.",
            };

            var pool = new List<string>
        {
            $"Opponents have all but given up campaigning in the district.",
            $"For voters here, supporting {Party.Acronym} has become something close to tradition.",
            $"Analysts call it one of the safest seats in the country.",
            $"Whole generations have known no other party in power locally.",
            $"The district's {District.Economy1.Label.ToLower()} workers form the backbone of this loyalty.",
            $"Critics warn that such dominance leaves little room for fresh ideas.",
        };

            // Two fragments here, shuffled, since this is a 'big' event worth a longer article
            return $"{milestone.RandomElement()} {string.Join(" ", pool.RandomElements(2))}";
        }
    }
}
