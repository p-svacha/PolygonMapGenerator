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

        private static float FUN_ARTICLE_CHANCE = 0.1f;

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
            Party winnerParty = seats.OrderByDescending(x => x.Value).First().Key;
            int topSeats = seats[winnerParty];
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
                    $"{winnerParty.Acronym} Wins!",
                    "The People Have Spoken",
                    $"A Year for {winnerParty.Acronym}",
                    $"{winnerParty.Acronym} Wins its {winnerParty.TotalElectionsWon.ToOrdinal()} Election",
                    $"Win Number {winnerParty.TotalElectionsWon} for {winnerParty.Acronym}",
                    $"{ElectionResult.Year}: The Year of {winnerParty.Acronym}",
                    $"{winnerParty.Acronym} Wins Cycle {ElectionResult.ElectionCycle}",
                };
                var chapters = new List<string>
                {
                    $"{winnerParty.NameOrAcr} has secured victory with {topSeats} of {totalSeats} seats. {secondParty.NameOrAcr} followed with {secondSeats}. It is the {winnerParty.TotalElectionsWon.ToOrdinal()} general election {winnerParty.Acronym} has won.",
                    $"With {topSeats} of {totalSeats} seats, {winnerParty.NameOrAcr} comes out on top this year, ahead of {secondParty.NameOrAcr} on {secondSeats}. Attention now turns to the cycle ahead.",
                    $"The votes are counted: {winnerParty.NameOrAcr} takes {topSeats} seats to {secondParty.NameOrAcr}'s {secondSeats}, claiming the year's election. It is the pary's {winnerParty.TotalElectionsWon.ToOrdinal()} win.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 15);
            }

            // Case: one party takes every seat
            if (topSeats == ElectionResult.GetTotalSeats())
            {
                var headlines = new List<string>
                {
                    "A Clean Sweep!",
                    $"{winnerParty.Acronym} Takes Everything",
                    "Total Domination",
                    "Not a Seat to Spare",
                };
                var chapters = new List<string>
                {
                    $"In a staggering result, {winnerParty.NameOrAcr} has claimed every single one of the {totalSeats} seats on offer. No rival managed to win even one. Such complete dominance is rarely seen.",
                    $"{winnerParty.NameOrAcr} has swept the board, winning all {totalSeats} seats. The opposition is left with nothing to show for the cycle.",
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
                    $"The nation wakes to a split decision: {winnerParty.Name} and {secondParty.Name} have tied on {topSeats} seats apiece. With no single victor, uneasy negotiations lie ahead.",
                    $"It ends level. {winnerParty.Name} and {secondParty.Name} each took {topSeats} seats, leaving the parliament evenly split and the path forward unclear.",
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
                    $"In an extraordinary outcome, the three parties {winnerParty.Acronym}, {secondParty.Acronym} and {party3.Acronym} have finished level on {topSeats} seats each. Commentators are at a loss for precedent.",
                    $"{winnerParty.Acronym}, {secondParty.Acronym} and {party3.Acronym} each claimed {topSeats} seats, fracturing the parliament three ways and guaranteeing a turbulent cycle.",
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
                    $"It could hardly have been closer. {winnerParty.NameOrAcr} edged out {secondParty.NameOrAcr} by a single seat, {topSeats} to {secondSeats}. A handful of votes elsewhere could have flipped the result.",
                    $"{winnerParty.NameOrAcr} clings to victory by one seat: {topSeats} against {secondParty.NameOrAcr}'s {secondSeats}. The thinnest of margins hands them the year.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 250);
            }

            // Case: close finish (< 3% of seats), but not the exact one-seat case
            if (winners.Count == 1 && winnerMargin > 1 && winnerMargin < totalSeats * 0.03f)
            {
                var headlines = new List<string>
                {
                    "A Tight Race",
                    "Closer Than Expected",
                    "Neck and Neck to the End",
                };
                var chapters = new List<string>
                {
                    $"{winnerParty.NameOrAcr} held on through a tense count, finishing {winnerMargin} seats clear of {secondParty.NameOrAcr}, {topSeats} to {secondSeats}. The result was in doubt until the final districts reported.",
                    $"A narrow win for {winnerParty.NameOrAcr}, just {winnerMargin} seats ahead of {secondParty.NameOrAcr}. Few would have called it with confidence beforehand.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 50);
            }

            // Case: absolute majority (> 50%)
            if (winners.Count == 1 && topSeats > totalSeats * 0.5f)
            {
                var headlines = new List<string>
                {
                    "A Commanding Majority",
                    $"{winnerParty.Acronym} Takes Control",
                    "An Absolute Majority",
                };
                var chapters = new List<string>
                {
                    $"{winnerParty.NameOrAcr} has won an outright majority, claiming {topSeats} of {totalSeats} seats. With more than half the parliament, they can govern without compromise.",
                    $"More than half the seats now belong to {winnerParty.NameOrAcr} ({topSeats} of {totalSeats}). It is a mandate few parties ever achieve.",
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
                    $"{winnerParty.Acronym} Sweeps the Nation",
                };
                var chapters = new List<string>
                {
                    $"This was no ordinary win. {winnerParty.NameOrAcr} took {topSeats} of {totalSeats} seats, more than three quarters of the parliament, leaving the opposition in disarray.",
                    $"{winnerParty.NameOrAcr} has all but obliterated its rivals, securing {topSeats} of {totalSeats} seats. Such a margin reshapes the political landscape entirely.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 100);
            }

            // Case: top 3 within 4% of seats
            if (party3 != null && winners.Count == 1 && (topSeats - seats3) < totalSeats * 0.04f)
            {
                var headlines = new List<string>
                {
                    "A Three-Horse Race",
                    "Three Parties in Contention",
                    "Tightly Packed at the Top",
                };
                var chapters = new List<string>
                {
                    $"Three parties finished within touching distance: {winnerParty.Acronym} on {topSeats}, {secondParty.Acronym} on {secondSeats}, and {party3.Acronym} on {seats3}. {winnerParty.NameOrAcr} comes out ahead, but only just.",
                    $"The top three ({winnerParty.Acronym}, {secondParty.Acronym} and {party3.Acronym}) are separated by a mere handful of seats. {winnerParty.NameOrAcr} leads a crowded field.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 100);
            }

            // Case: top 4 within 5% of seats
            if (party4 != null && winners.Count == 1 && (topSeats - seats4) < totalSeats * 0.05f)
            {
                var headlines = new List<string>
                {
                    "A Crowded Field",
                    "Four-Way Scramble",
                    "Anyone's Election",
                };
                var chapters = new List<string>
                {
                    $"Four parties remain firmly in the hunt this cycle. {winnerParty.NameOrAcr} edges ahead on {topSeats}, but {secondParty.Acronym}, {party3.Acronym} and {party4.Acronym} are all within striking distance.",
                    $"Rarely has the top of the table been so congested: {winnerParty.Acronym}, {secondParty.Acronym}, {party3.Acronym} and {party4.Acronym} are all within {topSeats - seats4} seats of one another.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 100);
            }

            // Case: party wins its FIRST general election (cycle 5+ only)
            if (winners.Count == 1 && winnerParty.TotalElectionsWon == 1 && ElectionResult.ElectionCycle >= 5)
            {
                var headlines = new List<string>
                {
                    $"{winnerParty.Acronym} Wins its First Election!",
                    $"A First for {winnerParty.Acronym}",
                    $"{winnerParty.Acronym} Tastes Victory at Last",
                    $"A Maiden Win: {winnerParty.NameOrAcr}",
                };
                var chapters = new List<string>
                {
                    $"After cycles of trying, {winnerParty.NameOrAcr} has finally won its first general election, taking {topSeats} of {totalSeats} seats. Supporters who waited years celebrated long into the night.",
                    $"It has been a long road, but {winnerParty.NameOrAcr} claims its maiden general election victory with {topSeats} seats. {secondParty.NameOrAcr} will rue letting it slip.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 100);
            }

            // Case: a party is ONE general election away from winning the game (classic mode only)
            if (Game.IsClassicMode && winners.Count == 1)
            {
                int target = Game.Constitution.WinCondition.ConditionValue;
                if (winnerParty.TotalElectionsWon == target - 1)
                {
                    var headlines = new List<string>
                    {
                        $"{winnerParty.Acronym} almost there!",
                        $"{winnerParty.Acronym} One Election from Glory",
                        $"{winnerParty.Acronym} Closes In",
                        $"The Finish Line Beckons for {winnerParty.Acronym}",
                        $"The End is Nigh for {winnerParty.Acronym}",
                    };
                    var chapters = new List<string>
                    {
                        $"{winnerParty.NameOrAcr} stands a single election away from winning it all. With {winnerParty.TotalElectionsWon} of the required {target} general elections now secured, the nation watches nervously to see if anyone can stop them.",
                        $"One more win. That is all {winnerParty.NameOrAcr} now needs, having reached {winnerParty.TotalElectionsWon} general elections. Rivals must find an answer immediately or concede the game.",
                    };
                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 200);
                }
            }

            // Case: party TAKES the lead in total general elections won (cycle 5+, sole lead)
            if (winners.Count == 1 && ElectionResult.ElectionCycle >= 5)
            {
                int topWins = winnerParty.TotalElectionsWon;
                var others = Game.Parties.Where(p => p != winnerParty).ToList();
                int prevTopWins = topWins - 1; // before this election's win was counted
                bool nowSoleLeader = others.All(p => p.TotalElectionsWon < topWins);
                bool wasNotSoleLeaderBefore = others.Any(p => p.TotalElectionsWon >= prevTopWins);
                if (nowSoleLeader && wasNotSoleLeaderBefore)
                {
                    var headlines = new List<string>
                    {
                        $"{winnerParty.Acronym} Seizes the Lead!",
                        $"{winnerParty.Acronym} Moves Ahead",
                        $"A New Frontrunner",
                        $"{winnerParty.Acronym} Pulls Clear",
                    };
                    var chapters = new List<string>
                    {
                        $"With this victory, {winnerParty.NameOrAcr} moves into the outright lead in general elections won, now standing alone at {topWins}. The race for the game has a new frontrunner.",
                        $"{winnerParty.NameOrAcr} has wrested top spot from its rivals, reaching {topWins} general elections won, more than any other party. Momentum is firmly with them.",
                    };
                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 80);
                }
            }

            // Case: party EQUALS the leader in total general elections won (cycle 5+, now tied at the top)
            if (winners.Count == 1 && ElectionResult.ElectionCycle >= 5)
            {
                int topWins = Game.Parties.Max(p => p.TotalElectionsWon);
                var others = Game.Parties.Where(p => p != winnerParty).ToList();
                int leadersTied = others.Count(p => p.TotalElectionsWon == topWins);
                if (winnerParty.TotalElectionsWon == topWins && leadersTied >= 1)
                {
                    Party rival = others.First(p => p.TotalElectionsWon == topWins);
                    var headlines = new List<string>
                    {
                        $"{winnerParty.Acronym} Equals the top with {winnerParty.TotalElectionsWon} wins",
                        $"{winnerParty.Acronym} Catches Up in Election Wins",
                        "A New Tie for the Overall Lead",
                        $"A tie at the top after {winnerParty.Acronym} wins"
                    };
                    var chapters = new List<string>
                    {
                        $"{winnerParty.NameOrAcr} has pulled level at the summit, matching {rival.NameOrAcr} on {topWins} general elections won. The contest for overall victory could hardly be tighter.",
                        $"By winning this year, {winnerParty.NameOrAcr} draws equal with the frontrunners on {topWins} general elections. The lead is now shared, and the pressure mounts.",
                    };
                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 70);
                }
            }

            // Case: a DIFFERENT party won than in the previous general election (cycle 2+)
            if (winners.Count == 1 && ElectionResult.ElectionCycle >= 2)
            {
                Party previousWinner = GetPreviousElectionSoleWinner();
                if (previousWinner != null && previousWinner != winnerParty)
                {
                    var headlines = new List<string>
                    {
                        "Power Changes Hands",
                        $"{winnerParty.Acronym} Unseats {previousWinner.Acronym}",
                        $"{winnerParty.Acronym} emerges as new most popular Party",
                        $"{previousWinner.Acronym} loses power",
                    };
                    var chapters = new List<string>
                    {
                        $"The wheel turns: {winnerParty.NameOrAcr} has won this year's election with {topSeats} seats, ending {previousWinner.NameOrAcr}'s reign from the previous cycle. Fortunes can shift quickly in politics.",
                        $"Last cycle belonged to {previousWinner.NameOrAcr}; this one is {winnerParty.NameOrAcr}'s. The new victors took {topSeats} of {totalSeats} seats to claim the crown.",
                    };
                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 50);
                }
            }

            // Case: SAME party won 3+ general elections in a row
            if (winners.Count == 1 && GetConsecutiveWinStreak(winnerParty) >= 3)
            {
                int streak = GetConsecutiveWinStreak(winnerParty);
                var headlines = new List<string>
                {
                    $"{winnerParty.Acronym} Reigns On",
                    $"{streak} in a Row for {winnerParty.Acronym}",
                    "A Dynasty in the Making",
                    $"{winnerParty.Acronym} Won't Let Go",
                };
                var chapters = new List<string>
                {
                    $"Make it {streak} straight. {winnerParty.NameOrAcr} has won its {streak.ToOrdinal()} consecutive general election, taking {topSeats} of {totalSeats} seats. Rivals are running out of ideas.",
                    $"{winnerParty.NameOrAcr}'s grip on power shows no sign of loosening, with a {streak.ToOrdinal()} successive victory this year. Talk of a dynasty grows louder.",
                };
                candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 50);
            }

            // Case: one or more parties were eliminated this cycle (Battle Royale only)
            if (Game.IsBattleRoyale)
            {
                var eliminatedThisCycle = GetPartiesEliminatedThisCycle();
                if (eliminatedThisCycle.Count > 0)
                {
                    string who = eliminatedThisCycle.Count == 1
                        ? eliminatedThisCycle[0].NameOrAcr
                        : JoinPartyNames(eliminatedThisCycle);

                    var headlines = eliminatedThisCycle.Count == 1
                        ? new List<string>
                        {
                            $"{eliminatedThisCycle[0].Acronym} Is Out!",
                            $"The End for {eliminatedThisCycle[0].Acronym}",
                            "One Falls",
                            $"{eliminatedThisCycle[0].Acronym} Eliminated",
                        }
                        : new List<string>
                        {
                            "Multiple Parties Fall!",
                            "A Brutal Cycle",
                            "The Field Thins",
                            $"{eliminatedThisCycle.Count} Parties Eliminated",
                        };

                    var chapters = eliminatedThisCycle.Count == 1
                        ? new List<string>
                        {
                            $"{who} has run out of legitimacy and is eliminated from the contest. Its supporters are left to wonder what might have been.",
                            $"It is over for {who}. With legitimacy exhausted, the party exits the race, leaving the survivors to fight on.",
                        }
                        : new List<string>
                        {
                            $"A merciless cycle claims {eliminatedThisCycle.Count} parties at once: {who} have all been eliminated. The race tightens dramatically.",
                            $"{who} are gone, all eliminated in the same brutal cycle. Few expected the field to thin so suddenly.",
                        };

                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 120);
                }
            }

            // Case: multi-way tie at the top of the standings (3+ parties sharing the leading score)
            {
                var standings = Game.GetCurrentStandings();
                int topScore = standings.Values.Max();
                List<Party> tiedLeaders = standings.Where(x => x.Value == topScore).Select(x => x.Key).ToList();

                if (tiedLeaders.Count >= 3)
                {
                    string scoreLabel = Game.IsClassicMode ? "election win" : "legitimacy";
                    string tiedNames = JoinPartyNames(tiedLeaders);

                    var headlines = new List<string>
                    {
                        "No One Pulls Ahead",
                        "A Deadlocked Race",
                        "All Tied at the Top",
                        $"{tiedLeaders.Count}-Way Tie in the Standings",
                        "The Race Could Not Be Tighter",
                    };
                    var chapters = new List<string>
                    {
                        $"After the dust settles, {tiedNames} all share the lead with {topScore} {scoreLabel.Pluralize(topScore)}. With {tiedLeaders.Count} parties level at the top, the race for victory remains entirely open.",
                        $"The standings offer no clarity: {tiedNames} are all locked on {topScore} {scoreLabel.Pluralize(topScore)}. Any one of them could pull ahead next cycle.",
                        $"A {tiedLeaders.Count}-way tie at the summit. {tiedNames} each sit on {topScore} {scoreLabel.Pluralize(topScore)}, and no party can claim the initiative heading into the next cycle.",
                    };
                    candidates.Add((headlines.RandomElement(), chapters.RandomElement()), 140);
                }
            }

            var chosen = candidates.GetWeightedRandomElement();
            mainArticle.SetHeadline(chosen.Headline);
            mainArticle.SetChapter1(chosen.Chapter1);
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
                foreach(Party party in ElectionResult.SeatsWon.Keys)
                {
                    weights[party] = ElectionResult.SeatsWon[party] + 2;
                }
                return weights.GetWeightedRandomElement();
            }

            // Option: an opponent's most-invested policies + whether they'll keep investing.
            {
                Party opp = PickWeightedOpponent();
                if (opp != null)
                {
                    var top = opp.ActivePolicies.Where(p => p.Value > 0).OrderByDescending(p => p.Value).Take(2).ToList();
                    if (top.Count > 0)
                    {
                        string list = JoinPolicies(top.Select(p => $"{p.Name}"));
                        bool keepGoing = top.Any(p => opp.AI != null && (opp.AI.GetPolicyWeight(p) > 0.5f && !p.IsMaxed));
                        string tail = keepGoing
                            ? $" Insiders expect {opp.Acronym} to keep pouring resources into these areas."
                            : $" That {opp.Acronym} continues down this path is unlikely.";
                        candidates.Add($"Word from within {opp.NameOrAcr} suggests their efforts have centred on the {list} policies.{tail}");
                    }
                }
            }

            // Option: an opponent's highest-weighted policies + current points spent.
            {
                Party opp = PickWeightedOpponent();
                int amount = Random.Range(2, 3 + 1);
                if (opp != null && opp.AI != null)
                {
                    var favoured = opp.ActivePolicies
                        .OrderByDescending(p => opp.AI.GetPolicyWeight(p) + (Random.value * 0.1f))
                        .Take(amount).ToList();
                    if (favoured.Count > 0)
                    {
                        string list = JoinPolicies(favoured.Select(p => $"{p.Name}"));
                        candidates.Add($"Strategists close to {opp.NameOrAcr} reveal a strong leaning toward {list} policies. Expect those priorities to shape their campaign.");
                    }
                }
            }

            // Option: an opponent's lowest-weighted policies (areas they neglect).
            {
                Party opp = PickWeightedOpponent();
                int amount = Random.Range(2, 3 + 1);
                if (opp != null && opp.AI != null)
                {
                    var neglected = opp.ActivePolicies
                        .OrderBy(p => opp.AI.GetPolicyWeight(p) + (Random.value * 0.1f))
                        .Take(amount).ToList();
                    if (neglected.Count > 0)
                    {
                        string list = JoinPolicies(neglected.Select(p => p.Name));
                        candidates.Add($"One weakness for {opp.NameOrAcr}: they show little interest in {list} policies. A rival willing to contest those areas may find an opening.");
                    }
                }
            }

            // Option: a single attribute the very next district will have (and how many of the next few share it).
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
                        candidates.Add($"{howMany} the nation {(count == 1 ? "is rumoured to be" : "are rumoured to be")} {label}. Parties will want to plan accordingly.");
                    }
                }
            }

            // Option: an attribute shared by all of the next few districts.
            {
                var upcoming = GetUpcomingDistricts(3);
                if (upcoming.Count >= 2)
                {
                    var (label, _) = GetSharedUpcomingAttribute(upcoming, requireAllThree: true);
                    if (label != null)
                        candidates.Add($"Surveyors report that the coming wave of new districts will share a common thread: all are {label}.");
                }
            }

            // Option: a random policy and how much the top opponents have invested in it.
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
                        candidates.Add($"Notably, none of the leading parties have invested in {probe.Name} policy, a gap that remains wide open.");
                    }
                }
            }

            // Option: spotlight the next upcoming district, its seats and a descriptive label.
            {
                var upcoming = GetUpcomingDistricts(1);
                if (upcoming.Count > 0)
                {
                    District next = upcoming[0];
                    int seats = next.GetSeats();
                    string label = next.GetDescripiveLabel();

                    string seatClause = seats <= 4
                        ? $"a modest {seats} {"seat".Pluralize(seats)}"
                        : seats <= 7
                            ? $"{seats} {"seat".Pluralize(seats)}"
                            : $"a substantial {seats} {"seat".Pluralize(seats)}";

                    var options = new List<string>
                    {
                        $"Sources reveal information about a district that will join the nation soon. The {label} will be worth {seatClause} in parliament. Parties would do well to prepare.",
                        $"Information was leaked regarding {next.Name}, a district said to join the nation soon. The {label} will be worth {seatClause} in parliament. Parties would do well to prepare.",
                        $"All eyes are on {next.Name}, an upcoming district. The {label} is expected to bring {seatClause} to the parliament. Those who plan ahead will have an edge.",
                        $"There have been reports about a district, that will join the nation in {ElectionResult.Year + 2}. With {seatClause} on offer, the {label} will be worth watching closely.",
                    };
                    candidates.Add(options.RandomElement());
                }
            }

            // Option: pure flavour, no hint. Always available as a floor.
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

        #region Helpers

        private static string JoinPartyNames(List<Party> parties)
        {
            var names = parties.Select(p => p.NameOrAcr).ToList();
            if (names.Count == 1) return names[0];
            return string.Join(", ", names.Take(names.Count - 1)) + " and " + names.Last();
        }

        /// <summary>
        /// Returns the sole winner of the previous general election, or null if there wasn't one (tie or no prior election).
        /// </summary>
        private static Party GetPreviousElectionSoleWinner()
        {
            var all = Game.GetAllElectionResults(); // assumed accessor, see note
            if (all.Count < 2) return null;
            var prev = all[all.Count - 2];
            return prev.WinnerParties.Count == 1 ? prev.WinnerParties[0] : null;
        }

        private static List<Party> GetPartiesEliminatedThisCycle()
        {
            // At newspaper-generation time eliminations haven't been applied yet,
            // so detect parties whose legitimacy has hit zero this cycle.
            return Game.Parties
                .Where(p => !p.IsEliminated && p.Legitimacy <= 0)
                .ToList();
        }

        /// <summary>
        /// Counts how many consecutive most-recent general elections this party has won outright (sole winner).
        /// </summary>
        private static int GetConsecutiveWinStreak(Party party)
        {
            var all = Game.GetAllElectionResults();
            int streak = 0;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                if (all[i].WinnerParties.Count == 1 && all[i].WinnerParties[0] == party) streak++;
                else break;
            }
            return streak;
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
                ($"{first.AgeGroup.Label}-dominated", d => d.AgeGroup == first.AgeGroup),
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
            bool hasFunArticle = Random.value < FUN_ARTICLE_CHANCE;

            int numArticles = randomEvents.Count + newsEvents.Count + (hasFunArticle ? 1 : 0);

            // Generate articles about random district events
            foreach (RandomEvent randomEvent in randomEvents)
            {
                articles.Add(GenerateRandomEventArticle(randomEvent));
            }

            // Generate articles about news events
            foreach (NewsEvent newsEvent in newsEvents)
            {
                articles.Add(GenerateNewsEventArticle(newsEvent));
            }

            // Generate fun article
            if (hasFunArticle)
            {
                articles.Add(FunArticleGenerator.GenerateFunArticle(Newspaper));
            }

            articles = articles.OrderByDescending(a => a.Priority).Take(6).ToList();

            int currentArticleIndex = 1;
            foreach (NewspaperMinorArticle article in articles)
            {
                article.SetIsSmallVersion(IsSmallArticle(currentArticleIndex, Mathf.Min(numArticles, 6)));
                currentArticleIndex++;
            }

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

        private static NewspaperMinorArticle GenerateRandomEventArticle(RandomEvent e)
        {
            var article = new NewspaperMinorArticle(Newspaper);
            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/{e.GetArticleIconName()}"));
            article.SetHeadline(e.GetArticleHeadline());
            article.SetBodyText(e.GetArticleBody());
            article.Priority = e.GetArticlePriority();
            return article;
        }

        private static NewspaperMinorArticle GenerateNewsEventArticle(NewsEvent e)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);

            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/{e.GetArticleIconName()}"));
            article.SetHeadline(e.GetArticleHeadline());
            article.SetBodyText(e.GetArticleBody());
            article.Priority = e.GetArticlePriority();

            return article;
        }

        #endregion
    }
}
