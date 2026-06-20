using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_DistrictSeatChange : NewsEvent
    {
        public District District { get; private set; }
        public int SeatsBefore { get; private set; }
        public int SeatsAfter { get; private set; }
        public bool IsGain => SeatsAfter > SeatsBefore;

        public NewsEvent_DistrictSeatChange(District district, int seatsBefore, int seatsAfter) : base()
        {
            District = district;
            SeatsBefore = seatsBefore;
            SeatsAfter = seatsAfter;
        }

        public override string GetArticleIconName() => "Star";
        public override int GetArticlePriority() => 50 + SeatsAfter;

        public override string GetArticleHeadline()
        {
            List<string> gain = new List<string>()
            {
                $"{District.Name} Gains a Seat",
                $"Growing {District.Name} Earns a Seat",
                $"More Clout for {District.Name}",
                $"{District.Name} Expands Its Voice",
            };
            List<string> loss = new List<string>()
            {
                $"{District.Name} Loses a Seat",
                $"Shrinking {District.Name} Sheds a Seat",
                $"{District.Name} Loses Ground",
                $"A Quieter Voice for {District.Name}",
            };
            return IsGain ? gain.RandomElement() : loss.RandomElement();
        }

        public override string GetArticleBody()
        {
            // The fact
            List<string> gainFacts = new List<string>()
            {
                $"{District.Name} will send {SeatsAfter} representatives to parliament next cycle, one more than before.",
                $"Population growth has earned {District.Name} a new seat, bringing its total to {SeatsAfter}.",
                $"Come next election, {District.Name} will hold {SeatsAfter} seats.",
            };

            List<string> lossFacts = new List<string>()
            {
                $"{District.Name} will send {SeatsAfter} representatives to parliament next cycle, one fewer than before.",
                $"A shrinking population costs {District.Name} a seat, leaving it with {SeatsAfter}.",
                $"Come next election, {District.Name} will hold just {SeatsAfter} seats.",
            };

            List<string> facts = IsGain ? gainFacts : lossFacts;

            // Colour fragments: each stands alone, shuffled and sampled
            var pool = new List<string>();
            if (IsGain)
            {
                pool.Add("Local officials are celebrating the newfound influence.");
                pool.Add("More residents means more votes, and more votes mean more power in the capital.");
                pool.Add($"The {District.Economy1.Label.ToLower()} sector has drawn families to the area for years.");
                pool.Add("Rival parties are already eyeing the district's growing electorate.");
            }
            else
            {
                pool.Add("Local officials have voiced concern over the district's decline.");
                pool.Add("Fewer residents means a smaller say in national affairs.");
                pool.Add($"A downturn in the {District.Economy1.Label.ToLower()} sector may be driving people away.");
                pool.Add("Parties will think twice before spending resources here next cycle.");
            }
            // Shared, context-light fragments for extra variety
            pool.Add($"It is a notable shift for the {District.Density.Label.ToLower()}-density region.");

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElement())}";
        }
    }
}
