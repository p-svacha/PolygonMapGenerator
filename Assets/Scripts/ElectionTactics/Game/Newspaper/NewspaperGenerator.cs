using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public static class NewspaperGenerator
    {
        private static ElectionTacticsGame Game => ElectionTacticsGame.Instance;
        private static GeneralElectionResult ElectionResult;
        private static Newspaper Newspaper;

        public static Newspaper GenerateYearNewspaper()
        {
            ElectionResult = Game.GetLatestElectionResult();

            Newspaper = new Newspaper(ElectionResult);
            Newspaper.SetMainArticle(GenerateMainArticle());
            Newspaper.SetMinorArticles(GenerateMinorArticles());
            return Newspaper;
        }

        /// <summary>
        /// Generates the full article of the year's newspaper based on the general election result.
        /// </summary>
        private static NewspaperMainArticle GenerateMainArticle()
        {
            NewspaperMainArticle mainArticle = new NewspaperMainArticle(Newspaper);

            // Chapter 1
            GenerateHeadlineAndChapter1(mainArticle);

            // Chapter 2
            string chapter2 = GenerateChapter2();
            mainArticle.SetChapter2(chapter2);

            return mainArticle;
        }

        /// <summary>
        /// Chapter 1 is generic flavour describing the election outcome. Fixed cases so the player learns to read them.
        /// </summary>
        private static void GenerateHeadlineAndChapter1(NewspaperMainArticle mainArticle)
        {
            var seats = ElectionResult.SeatsWon;
            int totalSeats = seats.Values.Sum();
            Party topParty = seats.OrderByDescending(x => x.Value).First().Key;
            int topSeats = seats[topParty];
            Party secondParty = seats.OrderByDescending(x => x.Value).Skip(1).First().Key;
            int secondSeats = seats[secondParty];
            Party party3 = seats.Count() > 2 ? seats.OrderByDescending(x => x.Value).Skip(2).First().Key : null;
            int seats3 = party3 != null ? seats[party3] : -1;
            Party party4 = seats.Count() > 3 ? seats.OrderByDescending(x => x.Value).Skip(3).First().Key : null;
            int seats4 = party4 != null ? seats[party4] : -1;

            List<Party> winners = ElectionResult.WinnerParties;
            int winnerMargin = topSeats - secondSeats;

            var candidates = new Dictionary<(string Headline, string Chapter1), int>();

            // Case: general single winner (baseline, always available)
            if (winners.Count == 1)
            {
                var headlines = new List<string>
                {
                    "Election Results Are In!",
                    $"{topParty.Acronym} Wins!",
                    "The People Have Spoken",
                    $"A Year for {topParty.Acronym}",
                };
                var chapters = new List<string>
                {
                    $"{topParty.NameOrAcr} has secured victory with {topSeats} of {totalSeats} seats. {secondParty.NameOrAcr} followed with {secondSeats}. It is the {Ordinal(topParty.TotalElectionsWon)} general election {topParty.Acronym} has won.",
                    $"With {topSeats} of {totalSeats} seats, {topParty.NameOrAcr} comes out on top this year, ahead of {secondParty.NameOrAcr} on {secondSeats}. Attention now turns to the cycle ahead.",
                    $"The votes are counted: {topParty.NameOrAcr} takes {topSeats} seats to {secondParty.NameOrAcr}'s {secondSeats}, claiming the year's election.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 10);
            }

            // Case: one party takes every seat
            if (topSeats == ElectionResult.GetTotalSeats())
            {
                var headlines = new List<string>
                {
                    "A Clean Sweep!",
                    $"{topParty.Acronym} Takes Everything",
                    "Total Domination",
                    "Not a Seat to Spare",
                };
                var chapters = new List<string>
                {
                    $"In a staggering result, {topParty.NameOrAcr} has claimed every single one of the {totalSeats} seats on offer. No rival managed to win even one. Such complete dominance is rarely seen.",
                    $"{topParty.NameOrAcr} has swept the board, winning all {totalSeats} seats. The opposition is left with nothing to show for the cycle.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: exact two-way tie at the top
            if (topSeats == secondSeats && topSeats != seats3)
            {
                var headlines = new List<string>
                {
                    "A Divided Parliament",
                    "Deadlock!",
                    "No Clear Winner",
                    "Two-Way Stalemate",
                };
                var chapters = new List<string>
                {
                    $"The nation wakes to a split decision: {topParty.NameOrAcr} and {secondParty.NameOrAcr} have tied on {topSeats} seats apiece. With no single victor, uneasy negotiations lie ahead.",
                    $"It ends level. {topParty.NameOrAcr} and {secondParty.NameOrAcr} each took {topSeats} seats, leaving the parliament evenly split and the path forward unclear.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: three-way tie at the top
            if (party3 != null && topSeats == secondSeats && topSeats == seats3)
            {
                var headlines = new List<string>
                {
                    "A Three-Way Split!",
                    "Parliament in Pieces",
                    "Three Share the Crown",
                };
                var chapters = new List<string>
                {
                    $"In an extraordinary outcome, three parties — {topParty.Acronym}, {secondParty.Acronym} and {party3.Acronym} — have finished level on {topSeats} seats each. Commentators are at a loss for precedent.",
                    $"{topParty.Acronym}, {secondParty.Acronym} and {party3.Acronym} each claimed {topSeats} seats, fracturing the parliament three ways and guaranteeing a turbulent cycle.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: won by exactly one seat
            if (winners.Count == 1 && topSeats == secondSeats + 1)
            {
                var headlines = new List<string>
                {
                    "Won by a Single Seat!",
                    "The Narrowest of Margins",
                    "One Seat Decides It",
                    "Down to the Wire",
                };
                var chapters = new List<string>
                {
                    $"It could hardly have been closer. {topParty.NameOrAcr} edged out {secondParty.NameOrAcr} by a single seat, {topSeats} to {secondSeats}. A handful of votes elsewhere could have flipped the result.",
                    $"{topParty.NameOrAcr} clings to victory by one seat — {topSeats} against {secondParty.NameOrAcr}'s {secondSeats}. The thinnest of margins hands them the year.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: close finish (< 5% of seats), but not the exact one-seat case
            if (winners.Count == 1 && winnerMargin > 1 && winnerMargin < totalSeats * 0.05f)
            {
                var headlines = new List<string>
                {
                    "A Tight Race",
                    "Closer Than Expected",
                    "Neck and Neck to the End",
                };
                var chapters = new List<string>
                {
                    $"{topParty.NameOrAcr} held on through a tense count, finishing {winnerMargin} seats clear of {secondParty.NameOrAcr}, {topSeats} to {secondSeats}. The result was in doubt until the final districts reported.",
                    $"A narrow win for {topParty.NameOrAcr}, just {winnerMargin} seats ahead of {secondParty.NameOrAcr}. Few would have called it with confidence beforehand.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 50);
            }

            // Case: absolute majority (> 50%)
            if (winners.Count == 1 && topSeats > totalSeats * 0.5f)
            {
                var headlines = new List<string>
                {
                    "A Commanding Majority",
                    $"{topParty.Acronym} Takes Control",
                    "An Absolute Majority",
                };
                var chapters = new List<string>
                {
                    $"{topParty.NameOrAcr} has won an outright majority, claiming {topSeats} of {totalSeats} seats. With more than half the parliament, they can govern without compromise.",
                    $"More than half the seats now belong to {topParty.NameOrAcr} — {topSeats} of {totalSeats}. It is a mandate few parties ever achieve.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 50);
            }

            // Case: supermajority (> 75%)
            if (winners.Count == 1 && topSeats > totalSeats * 0.75f && topSeats < totalSeats)
            {
                var headlines = new List<string>
                {
                    "A Crushing Mandate",
                    "An Overwhelming Victory",
                    $"{topParty.Acronym} Sweeps the Nation",
                };
                var chapters = new List<string>
                {
                    $"This was no ordinary win. {topParty.NameOrAcr} took {topSeats} of {totalSeats} seats — more than three quarters of the parliament — leaving the opposition in disarray.",
                    $"{topParty.NameOrAcr} has all but obliterated its rivals, securing {topSeats} of {totalSeats} seats. Such a margin reshapes the political landscape entirely.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 100);
            }

            // Case: top 3 within 10% of seats
            if (party3 != null && winners.Count == 1 && (topSeats - seats3) < totalSeats * 0.10f)
            {
                var headlines = new List<string>
                {
                    "A Three-Horse Race",
                    "Three Parties in Contention",
                    "Tightly Packed at the Top",
                };
                var chapters = new List<string>
                {
                    $"Three parties finished within touching distance: {topParty.Acronym} on {topSeats}, {secondParty.Acronym} on {secondSeats}, and {party3.Acronym} on {seats3}. {topParty.NameOrAcr} comes out ahead, but only just.",
                    $"The top three — {topParty.Acronym}, {secondParty.Acronym} and {party3.Acronym} — are separated by a mere handful of seats. {topParty.NameOrAcr} leads a crowded field.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: top 4 within 15% of seats
            if (party4 != null && winners.Count == 1 && (topSeats - seats4) < totalSeats * 0.15f)
            {
                var headlines = new List<string>
                {
                    "A Crowded Field",
                    "Four-Way Scramble",
                    "Anyone's Election",
                };
                var chapters = new List<string>
                {
                    $"Four parties remain firmly in the hunt this cycle. {topParty.NameOrAcr} edges ahead on {topSeats}, but {secondParty.Acronym}, {party3.Acronym} and {party4.Acronym} are all within striking distance.",
                    $"Rarely has the top of the table been so congested: {topParty.Acronym}, {secondParty.Acronym}, {party3.Acronym} and {party4.Acronym} are all within {topSeats - seats4} seats of one another.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            var chosen = candidates.GetWeightedRandomElement();
            mainArticle.SetHeadline(chosen.Headline);
            mainArticle.SetChapter1(chosen.Chapter1);
        }

        /// <summary>
        /// Small ordinal helper ("1st", "2nd", "3rd"...).
        /// </summary>
        private static string Ordinal(int n)
        {
            if (n <= 0) return n.ToString();
            if (n % 100 is >= 11 and <= 13) return $"{n}th";
            return (n % 10) switch { 1 => $"{n}st", 2 => $"{n}nd", 3 => $"{n}rd", _ => $"{n}th" };
        }

        /// <summary>
        /// Chapter 2 is all about giving the player a hint about information that is otherwise not acquirable.
        /// </summary>
        /// <summary>
        /// Chapter 2 gives a hint the player can't otherwise obtain. Each option self-selects only when it has data.
        /// </summary>
        private static string GenerateChapter2()
        {
            var candidates = new List<string>();
            var standings = Game.GetCurrentStandings().Keys.Where(p => p != Game.LocalPlayerParty && !p.IsEliminated).ToList();

            // Pick an opponent weighted toward better standing (earlier in the standings list = stronger).
            Party PickWeightedOpponent()
            {
                if (standings.Count == 0) return null;
                var weights = new Dictionary<Party, int>();
                for (int i = 0; i < standings.Count; i++) weights[standings[i]] = standings.Count - i; // top of standings = highest weight
                return weights.GetWeightedRandomElement();
            }

            // Option 1: an opponent's most-invested policies + whether they'll keep investing.
            {
                Party opp = PickWeightedOpponent();
                if (opp != null)
                {
                    var top = opp.ActivePolicies.Where(p => p.Value > 0).OrderByDescending(p => p.Value).Take(2).ToList();
                    if (top.Count > 0)
                    {
                        string list = JoinPolicies(top.Select(p => $"{p.Name} ({p.Value})"));
                        // WillKeepInvesting: assumes a helper on PartyAI returning true if the policy is high-weighted.
                        bool keepGoing = top.Any(p => opp.AI != null && (opp.AI.GetPolicyWeight(p) > 0.5f && !p.IsMaxed));
                        string tail = keepGoing
                            ? $" Insiders expect {opp.Acronym} to keep pouring resources into these areas."
                            : $" Whether {opp.Acronym} continues down this path is uncertain.";
                        candidates.Add($"Word from within {opp.NameOrAcr} suggests their efforts have centred on {list}.{tail}");
                    }
                }
            }

            // Option 2: an opponent's highest-weighted policies + current points spent.
            {
                Party opp = PickWeightedOpponent();
                if (opp != null && opp.AI != null)
                {
                    var favoured = opp.ActivePolicies
                        .OrderByDescending(p => opp.AI.GetPolicyWeight(p))
                        .Take(2).ToList();
                    if (favoured.Count > 0)
                    {
                        string list = JoinPolicies(favoured.Select(p => $"{p.Name} ({p.Value} so far)"));
                        candidates.Add($"Strategists close to {opp.NameOrAcr} reveal a strong leaning toward {list}. Expect those priorities to shape their campaign.");
                    }
                }
            }

            // Option 3: an opponent's lowest-weighted policies (areas they neglect).
            {
                Party opp = PickWeightedOpponent();
                if (opp != null && opp.AI != null)
                {
                    var neglected = opp.ActivePolicies
                        .OrderBy(p => opp.AI.GetPolicyWeight(p))
                        .Take(2).ToList();
                    if (neglected.Count > 0)
                    {
                        string list = JoinPolicies(neglected.Select(p => p.Name));
                        candidates.Add($"One weakness for {opp.NameOrAcr}: they show little interest in {list} districts. A rival willing to contest those areas may find an opening.");
                    }
                }
            }

            // Option 4: a single attribute the very next district will have (and how many of the next few share it).
            {
                var upcoming = GetUpcomingDistricts(3);
                if (upcoming.Count > 0)
                {
                    var (label, count) = GetSharedUpcomingAttribute(upcoming, requireAllThree: false);
                    if (label != null)
                    {
                        string howMany = count == 1
                            ? "The next district to join"
                            : $"The next {count} districts to join";
                        candidates.Add($"{howMany} the nation {(count == 1 ? "will be" : "will all be")} {label}. Plan your policies accordingly.");
                    }
                }
            }

            // Option 5: an attribute shared by all of the next few districts.
            {
                var upcoming = GetUpcomingDistricts(3);
                if (upcoming.Count >= 2)
                {
                    var (label, _) = GetSharedUpcomingAttribute(upcoming, requireAllThree: true);
                    if (label != null)
                        candidates.Add($"Surveyors report that the coming wave of new districts will share a common thread: all are {label}.");
                }
            }

            // Option 6: a random policy and how much the top opponents have invested in it.
            {
                var allActive = Game.LocalPlayerParty.ActivePolicies;
                if (allActive.Count > 0 && standings.Count > 0)
                {
                    Policy probe = allActive.RandomElement();
                    var topOpponents = standings.Take(3).ToList();
                    var spends = topOpponents
                        .Select(o => (o, o.GetPolicyByLocalId(probe.LocalId).Value))
                        .Where(x => x.Item2 > 0)
                        .ToList();
                    if (spends.Count > 0)
                    {
                        string list = JoinPolicies(spends.Select(x => $"{x.o.Acronym} ({x.Item2})"));
                        candidates.Add($"On the matter of {probe.Name} policy, the leading parties have committed: {list}. The rest stay quiet on the issue.");
                    }
                    else
                    {
                        candidates.Add($"Notably, none of the leading parties have invested in {probe.Name} policy — a gap that remains wide open.");
                    }
                }
            }

            // Option 7: pure flavour, no hint. Always available as a floor.
            {
                var flavour = new List<string>
                {
                    "Elsewhere, commentators debate what the coming cycle holds, though few agree on anything of substance.",
                    "Pundits caution that a year is a long time in politics, and today's certainties may not survive the next.",
                    "Beyond the numbers, the public mood remains hard to read, with voters keeping their counsel for now.",
                    "Analysts remind readers that momentum can shift quickly, and no lead is ever truly safe.",
                };
                candidates.Add(flavour.RandomElement());
            }

            return candidates.RandomElement();
        }

        private static string JoinPolicies(IEnumerable<string> items)
        {
            var list = items.ToList();
            if (list.Count == 0) return "";
            if (list.Count == 1) return list[0];
            return string.Join(", ", list.Take(list.Count - 1)) + " and " + list.Last();
        }

        /// <summary>
        /// Returns the next N districts due to be activated, in spawn order.
        /// </summary>
        private static List<District> GetUpcomingDistricts(int n)
        {
            return Game.AllDistricts
                .Where(d => !d.IsActive)
                .OrderBy(d => d.Index)
                .Take(n)
                .ToList();
        }

        /// <summary>
        /// Finds a shared attribute among the upcoming districts. If requireAllThree, only returns
        /// an attribute all of them share; otherwise returns the first district's attribute and counts
        /// how many consecutive upcoming districts share it.
        /// </summary>
        private static (string Label, int Count) GetSharedUpcomingAttribute(List<District> upcoming, bool requireAllThree)
        {
            if (upcoming.Count == 0) return (null, 0);
            District first = upcoming[0];

            // Candidate attribute descriptions, each with a predicate matching another district.
            var attributes = new List<(string Label, System.Func<District, bool> Match)>
            {
                ($"{first.Language.Label.ToLower()}-speaking", d => d.Language == first.Language),
                first.HasReligion ? ($"of the {first.Religion.Label} faith", (System.Func<District, bool>)(d => d.Religion == first.Religion)) : (null, null),
                ($"{first.Density.Label.ToLower()}-density", d => d.Density == first.Density),
                ($"{first.AgeGroup.Label.ToLower()}-dominated", d => d.AgeGroup == first.AgeGroup),
                ($"centred on {first.Economy1.Label.ToLower()}", d => d.Economy1 == first.Economy1),
            };
            attributes = attributes.Where(a => a.Label != null).ToList();

            if (requireAllThree)
            {
                var shared = attributes.Where(a => upcoming.All(d => a.Match(d))).ToList();
                if (shared.Count == 0) return (null, 0);
                var pick = shared.RandomElement();
                return (pick.Label, upcoming.Count);
            }
            else
            {
                var pick = attributes.RandomElement();
                int count = 0;
                foreach (var d in upcoming) { if (pick.Match(d)) count++; else break; }
                return (pick.Label, count);
            }
        }


        /// <summary>
        /// Looks at the comparison between the current game state and the one before the election and creates articles about events (i.e. change of seats). Also looks at the event list and generates articles about events of the year. Can also in some cases include a procedurally generated gameplay-unrelevant fun article.
        /// <br/>List is always sorted by importance. And most important article will always get column 1, so the first shared columns are for unimportant articles.
        /// </summary>
        private static List<NewspaperMinorArticle> GenerateMinorArticles()
        {
            List<NewspaperMinorArticle> articles = new List<NewspaperMinorArticle>();

            // Identify number of articles
            List<RandomEvent> randomEvents = Game.RandomEvents.Where(e => e.Year == Game.Year - 1).ToList();
            List<NewsEvent> newsEvents = Game.NewsEvents.Where(e => e.Year == Game.Year - 1).ToList();
            bool hasFunArticle = Random.value < 0.3f;

            int numArticles = randomEvents.Count + newsEvents.Count + (hasFunArticle ? 1 : 0);
            int currentArticleIndex = 1;

            // Generate articles about random district events
            foreach (RandomEvent randomEvent in randomEvents)
            {
                articles.Add(GenerateRandomEventArticle(randomEvent, IsSmallArticle(currentArticleIndex, numArticles)));
                currentArticleIndex++;
            }

            // Generate articles about news events
            foreach (NewsEvent newsEvent in newsEvents)
            {
                articles.Add(GenerateNewsEventArticle(newsEvent, IsSmallArticle(currentArticleIndex, numArticles)));
                currentArticleIndex++;
            }

            // Generate fun article
            if (hasFunArticle)
            {
                articles.Add(GenerateFunArticle(IsSmallArticle(currentArticleIndex, numArticles)));
                currentArticleIndex++;
            }

            articles = articles.OrderByDescending(a => a.Priority).Take(6).ToList();
            return articles;
        }

        private static bool IsSmallArticle(int currentIndex, int numArticles)
        {
            if (currentIndex > numArticles) throw new System.Exception($"currentIndex is {currentIndex}, numArticles is {numArticles}.");

            if (numArticles <= 3) return false;
            if (numArticles == 4)
            {
                return (currentIndex >= 3);
            }
            if (numArticles == 5)
            {
                return (currentIndex >= 2);
            }

            // >= 6
            return true;
        }

        private static NewspaperMinorArticle GenerateRandomEventArticle(RandomEvent e, bool isSmallArticle)
        {
            var article = new NewspaperMinorArticle(Newspaper, isSmallArticle);
            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/{e.GetArticleIconName()}"));
            article.SetHeadline(e.GetArticleHeadline());
            article.SetBodyText(e.GetArticleBody());
            article.Priority = e.GetArticlePriority();
            return article;
        }

        private static NewspaperMinorArticle GenerateNewsEventArticle(NewsEvent e, bool isSmallArticle)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper, isSmallArticle);

            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/{e.GetArticleIconName()}"));
            article.SetHeadline(e.GetArticleHeadline());
            article.SetBodyText(e.GetArticleBody());
            article.Priority = e.GetArticlePriority();

            return article;
        }

        private static NewspaperMinorArticle GenerateFunArticle(bool isSmallArticle)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper, isSmallArticle);

            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/Quote"));
            article.SetHeadline("Fun Article");
            article.SetBodyText("Just some text to test stuff.");
            article.Priority = 1;

            return article;
        }
    }
}
