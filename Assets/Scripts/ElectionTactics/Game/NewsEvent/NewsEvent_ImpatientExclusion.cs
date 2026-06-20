using ElectionTactics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_ImpatientExclusion : NewsEvent
    {
        public District District { get; private set; }
        public List<Party> Parties { get; private set; }
        private bool IsMultiple => Parties.Count > 1;

        public NewsEvent_ImpatientExclusion(District district, List<Party> parties) : base()
        {
            District = district;
            Parties = parties;
        }

        public override string GetArticleIconName() => "Exclusion";
        public override int GetArticlePriority() => 60;

        public override string GetArticleHeadline()
        {
            if (IsMultiple)
            {
                string acronyms = JoinAcronyms();
                return new List<string>()
                {
                    $"{District.Name} Clears House",
                    $"Multiple Parties Barred from {District.Name}",
                    $"{District.Name} Has Had Enough",
                    $"Mass Exclusion in {District.Name}",
                    $"{acronyms} Shut Out of {District.Name}",
                }.RandomElement();
            }
            else
            {
                Party party = Parties[0];
                return new List<string>()
                {
                    $"{party.Acronym} Barred from {District.Name}",
                    $"{District.Name} Shuts Out {party.Acronym}",
                    $"Enough: {District.Name} Turns on {party.Acronym}",
                    $"{party.Acronym} Loses Its Place in {District.Name}",
                    $"{District.Name} Has Had Its Fill of {party.Acronym}",
                }.RandomElement();
            }
        }

        public override string GetArticleBody()
        {
            string facts;
            List<string> pool;

            if (IsMultiple)
            {
                string nameList = JoinNames();
                string acronymList = JoinAcronyms();

                facts = new List<string>()
                {
                    $"{nameList} have all been permanently barred from {District.Name} after each failing to win a seat there across {CT_Impatient.THRESHOLD} consecutive cycles.",
                    $"In a sweeping verdict, {District.Name} has excluded {nameList} from future ballots. None managed to win a seat there in {CT_Impatient.THRESHOLD} cycles.",
                    $"Voters in {District.Name} have run out of patience with {nameList}. All {Parties.Count} parties are now permanently shut out.",
                }.RandomElement();

                pool = new List<string>
                {
                    $"The district's Impatient character has long made clear that repeated failure carries consequences.",
                    $"The simultaneous exclusion of {Parties.Count} parties is unprecedented in recent memory.",
                    $"Rivals still standing in {District.Name} are expected to scramble for the newly unclaimed support.",
                    $"Officials from the affected parties have remained largely silent on what comes next.",
                    $"Local observers say the writing had been on the wall for all of them for some time.",
                    $"With so many parties gone at once, the political landscape of {District.Name} is fundamentally changed.",
                };
            }
            else
            {
                Party party = Parties[0];

                facts = new List<string>()
                {
                    $"After failing to win a single seat in {District.Name} for {CT_Impatient.THRESHOLD} consecutive cycles, {party.Name} has been permanently barred from standing there.",
                    $"Voters in {District.Name} have lost all patience with {party.Name}: after {CT_Impatient.THRESHOLD} fruitless elections, the party is excluded from future ballots.",
                    $"{party.Name} has been shut out of {District.Name} for good, having failed to secure a seat there across {CT_Impatient.THRESHOLD} cycles running.",
                }.RandomElement();

                pool = new List<string>
                {
                    $"The district's Impatient character has long made clear that repeated failure carries consequences.",
                    $"Party officials have not yet commented on what they plan to do next.",
                    $"Rivals are expected to move quickly to absorb {party.Acronym}'s former supporters.",
                    $"It is a humbling end to what was once a more competitive presence in the district.",
                    $"Whether {party.Acronym} can recover elsewhere remains to be seen.",
                    $"Local observers say the writing had been on the wall for some time.",
                };
            }

            return $"{facts} {string.Join(" ", pool.RandomElements(2))}";
        }

        private string JoinNames() => JoinList(Parties.Select(p => p.Name));
        private string JoinAcronyms() => JoinList(Parties.Select(p => p.Acronym));

        private static string JoinList(IEnumerable<string> items)
        {
            var list = items.ToList();
            if (list.Count == 1) return list[0];
            return string.Join(", ", list.Take(list.Count - 1)) + " and " + list.Last();
        }
    }
}
