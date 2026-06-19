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

            Newspaper = new Newspaper(Game.Year - 1, Game.ElectionCycle - 1);
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

            var seats = ElectionResult.SeatsWon;
            int totalSeats = seats.Values.Sum();
            Party topParty = seats.OrderByDescending(x => x.Value).First().Key;
            int topSeats = seats[topParty];
            List<Party> winners = ElectionResult.WinnerParties;

            // step 1: identify tags
            var tags = new List<string>();
            if (winners.Count > 1) tags.Add("tie");
            else
            {
                var ordered = seats.OrderByDescending(x => x.Value).ToList();
                int lead = ordered.Count > 1 ? ordered[0].Value - ordered[1].Value : ordered[0].Value;
                if (topSeats >= totalSeats * 0.6f) tags.Add("sweep");
                else if (lead <= 2) tags.Add("close");
                else tags.Add("clear_win");
            }

            // step 2: headline (<= 38 chars), picked from a per-tag pool
            string tag = tags[Random.Range(0, tags.Count)];
            string headline = tag switch
            {
                "tie" => PickShort(new[] { "A Divided Parliament!", "No Clear Winner Emerges", "Deadlock!" }),
                "sweep" => PickShort(new[] { "A Landslide Victory!", $"{topParty.Acronym} Sweeps the Nation!", "Total Domination!" }),
                "close" => PickShort(new[] { "A Race to the Wire!", "Nail-Biter Election!", "Too Close to Call!" }),
                _ => PickShort(new[] { "Election Results Are In!", $"{topParty.Acronym} Takes the Lead", "The People Have Spoken" }),
            };
            mainArticle.SetHeadline(headline);

            // step 3: chapter 1 (200-260 chars) about the result
            string c1 = tag switch
            {
                "tie" =>
                    $"The new parliament is split. Multiple parties finished level on {topSeats} seats, " +
                    $"and the nation now faces uncertain times as no single force can claim a mandate. " +
                    $"Backroom negotiations are expected to begin within days.",
                "sweep" =>
                    $"{topParty.Name} has swept the election, claiming {topSeats} of {totalSeats} seats. " +
                    $"Commentators are calling it one of the most decisive results in recent memory, " +
                    $"leaving rival parties scrambling to regroup before the next cycle.",
                "close" =>
                    $"It came down to the wire. {topParty.Name} edged ahead with {topSeats} seats, " +
                    $"but only barely, as challengers pushed them to the very last district. " +
                    $"The result could easily have gone another way.",
                _ =>
                    $"{topParty.Name} has secured a clear victory with {topSeats} of {totalSeats} seats. " +
                    $"The result hands them momentum heading into the next cycle, while opponents are " +
                    $"left to reflect on what went wrong at the ballot box.",
            };
            mainArticle.SetChapter1(c1);

            // step 4: chapter 2 (150-190 chars) about a secondary topic
            mainArticle.SetChapter2(GenerateChapter2());

            return mainArticle;
        }

        private static string GenerateChapter2()
        {
            // Pick a simple topic from the active game state
            var districts = Game.ActiveDistricts;
            District d = districts.RandomElement();

            var options = new List<string>
            {
                $"In {d.Name}, voters continue to weigh local issues heavily, with the district's " +
                $"{d.Economy1.Label.ToLower()} sector remaining a decisive factor at the polls.",

                $"Analysts note that {d.Language.Label}-speaking districts proved especially influential " +
                $"this cycle, shaping outcomes well beyond their borders.",

                $"With the population shifting year on year, several districts edge closer to gaining " +
                $"or losing seats, promising a different map in cycles to come.",
            };
            return options[Random.Range(0, options.Count)];
        }

        private static string PickShort(string[] pool)
        {
            // Prefer entries within the 38-char headline budget; fall back to the shortest
            var fitting = pool.Where(s => s.Length <= 38).ToList();
            if (fitting.Count > 0) return fitting[Random.Range(0, fitting.Count)];
            return pool.OrderBy(s => s.Length).First();
        }

        /// <summary>
        /// Looks at the comparison between the current game state and the one before the election and creates articles about events (i.e. change of seats). Also looks at the event list and generates articles about events of the year. Can also in some cases include a procedurally generated gameplay-unrelevant fun article.
        /// <br/>List is always sorted by importance. And most important article will always get column 1, so the first shared columns are for unimportant articles.
        /// </summary>
        private static List<NewspaperMinorArticle> GenerateMinorArticles()
        {
            List<NewspaperMinorArticle> articles = new List<NewspaperMinorArticle>();

            // Generate articles about random district events
            foreach (RandomEvent randomEvent in Game.RandomEvents.Where(e => e.Year == Game.Year - 1))
            {
                articles.Add(GenerateRandomEventArticle(randomEvent));
            }

            // Generate articles about news events
            foreach (NewsEvent newsEvent in Game.NewsEvents.Where(e => e.Year == Game.Year - 1))
            {
                articles.Add(GenerateNewsEventArticle(newsEvent));
            }

            // Generate fun article
            if (Random.value < 0.3f)
            {
                articles.Add(GenerateFunArticle());
            }

            articles = articles.OrderByDescending(a => a.Priority).Take(6).ToList();
            return articles;
        }


        private static NewspaperMinorArticle GenerateRandomEventArticle(RandomEvent e)
        {
            var article = new NewspaperMinorArticle(Newspaper);
            article.SetSprite(e.GetArticleIcon());
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

        private static NewspaperMinorArticle GenerateFunArticle()
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);

            article.SetHeadline("Fun Article");
            article.SetBodyText("Just some text to test stuff");
            article.Priority = 1;

            return article;
        }
    }
}
