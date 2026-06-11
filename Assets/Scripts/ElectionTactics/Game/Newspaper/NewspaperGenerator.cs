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

            // step 1: identify election result tags (i.e. single winner, 2 winners with same seats, close duel, sweep, one party wins all seats, all parties very close, 2 parties dominating out of more, 1 party alone at the very back, etc.) multiple can apply

            // step 2: pick out one tag and generate a headline for it out of a pool (max 38 characters)

            // step 3: pick out one tag and write chapter 1 about it. Chapter 1 should be 200-260 characters.

            // step 4: pick out one topic (out of predefined fixed pool) and generate chapter 2 about it. topics can be like a specific category (i.e. demography, economy, geography), a specific trait inside one of those, a specific part, a specific district, a policy etc. Chapter 2 should be 150-190 characters

            return mainArticle;
        }

        /// <summary>
        /// Looks at the comparison between the current game state and the one before the election and creates articles about events (i.e. change of seats). Also looks at the event list and generates articles about events of the year. Can also in some cases include a procedurally generated gameplay-unrelevant fun article.
        /// <br/>List is always sorted by importance. And most important article will always get column 1, so the first shared columns are for unimportant articles.
        /// </summary>
        /// <returns></returns>
        private static List<NewspaperMinorArticle> GenerateMinorArticles()
        {
            List<NewspaperMinorArticle> articles = new List<NewspaperMinorArticle>();

            // Generate articles about random district events
            foreach(RandomEvent randomEvent in Game.RandomEvents.Where(e => e.Year == Game.Year))
            {
                GenerateRandomEventArticle(randomEvent);
            }

            // Generate articles about seat changes in districts
            foreach(District d in Game.ActiveDistricts.Where(d => ElectionResult.DistrictResults.Any(dr => dr.District == d) && ElectionResult.GetDistrictResult(d).Seats != d.Seats))
            {
                GenerateSeatChangeArticle(d);
            }

            // Generate articles about density changes
            foreach (District d in Game.ActiveDistricts.Where(d => ElectionResult.DistrictResults.Any(dr => dr.District == d) && ElectionResult.GetDistrictResult(d).Density != d.Density))
            {
                GenerateDensityChangeArticle(d);
            }

            // Generate article about newly added district
            District addedDistrict = Game.ActiveDistricts.Last();
            GenerateDistrictAddedArticle(addedDistrict);

            // Generate fun article
            if (Random.value < 0.3f)
            {
                articles.Add(GenerateFunArticle());
            }

            return articles;
        }


        private static NewspaperMinorArticle GenerateRandomEventArticle(RandomEvent randomEvent)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);


            return article;
        }

        private static NewspaperMinorArticle GenerateSeatChangeArticle(District district)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);


            return article;
        }

        private static NewspaperMinorArticle GenerateDensityChangeArticle(District district)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);


            return article;
        }

        private static NewspaperMinorArticle GenerateDistrictAddedArticle(District district)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);


            return article;
        }

        private static NewspaperMinorArticle GenerateFunArticle()
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(Newspaper);


            return article;
        }
    }
}
