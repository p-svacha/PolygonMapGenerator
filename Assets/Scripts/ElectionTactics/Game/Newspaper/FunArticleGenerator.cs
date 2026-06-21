using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public static class FunArticleGenerator
    {
        enum FunArticleType
        {
            TimeFrameComparison,
            OutrageHeadline,
            HealthFad,
            NewTrend,
            QuoteOfYear,
            ExpertClaim,
            PublicService,
            DangerWarning,
            FreakAccident,
            SportSwitch,
            AnimalSport,
        }

        // --- Word lists ---

        private static List<string> nounsPlural = new List<string>()
        {
            "cows", "children", "monks", "penguins", "hamsters", "babies", "dogs", "ducks",
            "investment bankers", "downhill skiers", "preschoolers", "elephants", "tomatoes",
            "pensioners", "lighthouse keepers", "competitive eaters", "synchronized swimmers",
            "beekeepers", "mimes", "geese", "librarians", "lumberjacks", "astronauts",
        };

        private static List<string> animals = new List<string>()
        {
            "cow", "penguin", "hamster", "dog", "duck",
            "elephant", "cat", "parrot", "rabbit", "ferret",
            "iguana", "capybara", "hedgehog", "flamingo", "alpaca",
            "sloth", "otter", "meerkat", "lemur", "pelican",
            "walrus", "narwhal", "platypus", "axolotl", "pangolin",
            "wombat", "manatee", "tapir", "monkey", "worm",
            "goat", "llama", "porcupine", "armadillo", "bison",
            "cassowary", "quokka", "echidna", "okapi", "aardvark",
            "mantis shrimp", "komodo dragon", "tardigrade", "wolverine", "binturong",
            "fossa", "numbat", "kinkajou", "babirusa", "saiga",
        };

        private static List<string> matchingAttribute = new List<string>()
        {
            "temperament", "color", "height", "wealth", "dietary habits", "political leanings", "star sign",
        };

        private static List<string> verbsPast = new List<string>()
        {
            "eaten", "consumed", "gathered", "multiplied", "vanished", "rebelled",
            "assembled", "migrated", "protested", "celebrated", "unionized", "fled",
        };

        private static List<string> activitiesGerund = new List<string>()
        {
            "knitting", "competitive napping", "extreme ironing", "interpretive dance",
            "underwater chess", "speed-walking", "artisanal yodeling", "synchronized blinking",
            "cheese rolling", "professional queuing", "aggressive gardening",
        };

        private static List<string> comparator = new List<string>()
        {
            "more", "less", "an equal amount of", "considerably more", "alarmingly less",
        };

        private static List<string> timeFrames = new List<string>()
        {
            "morning", "evening", "week", "month", "year", "decade", "afternoon", "fortnight",
        };

        private static List<string> reasonAdjectives = new List<string>()
        {
            "bad", "good", "unusually mild", "historically poor", "surprisingly favorable",
            "downright suspicious", "frankly inexplicable",
        };

        private static List<string> reasonNouns = new List<string>()
        {
            "weather", "lighting", "scheduling", "leadership", "ventilation",
            "tax policy", "background music", "regional pride", "music", "emotions"
        };

        private static List<string> consumables = new List<string>()
        {
            "paracetamol", "benzine", "alcohol", "beef", "fruit", "fish", "dandelion",
            "chocolate", "bacteria", "water", "pizza", "gravel", "oat milk", "mustard",
            "chalk", "vinegar", "mayonnaise", "cinnamon", "lard", "seaweed",
            "prunes", "beetroot", "raw dough", "mineral water", "cabbage",
            "sardines", "dried ketchup", "ice cubes", "sunscreen",
        };

        private static List<string> authorities = new List<string>()
        {
            "local monks", "leading scientists", "the national football team", "city planners",
            "dogschool teachers", "the beekeepers' guild", "retired admirals", "tax inspectors",
            "the association of lighthouse keepers", "concerned parents", "the cheese board",
            "marine biologists", "the puppeteers' union", "the bakers' federation",
            "retired circus performers", "the national napping association",
            "the institute of competitive eating", "the royal society of mimes",
            "professional whistlers", "the association of amateur geologists",
            "the synchronized swimming committee", "the national lumberjack council",
            "enthusiastic amateurs",
        };

        private static List<string> reactions = new List<string>()
        {
            "are outraged", "are not amused", "remain divided", "urge calm",
            "have expressed concern", "are baffled", "have called for a review",
            "are cautiously optimistic", "have issued a strongly worded letter",
            "are broadly supportive", "have refused to comment", "are furious",
            "have demanded accountability", "are delighted", "are deeply confused",
        };

        private static List<string> concepts = new List<string>()
        {
            "the growing hamster population", "recreational mathematics", "the return of the T-Rex",
            "mandatory joy", "the nuclear bouillon", "competitive crucifixion", "the tomato shortage",
            "double-use playgrounds", "the handball uprising", "artisanal silence",
        };

        private static List<string> symptoms = new List<string>()
        {
            "excessive blinking", "a refusal to fetch", "sudden interest in jazz",
            "unexplained elegance", "a thousand-yard stare", "spontaneous tap dancing",
            "an aversion to Mondays", "compulsive list-making",
        };

        private static List<string> remedies = new List<string>()
        {
            "reduce its screen time", "consult a licensed mime", "increase exposure to sunlight",
            "limit its tax burden", "introduce more potholes to its routine",
            "schedule fewer meetings", "provide additional snacks",
        };

        private static List<string> colors = new List<string>()
        {
            "green", "yellow", "beige", "ultraviolet", "navy", "crimson", "teal",
            "magenta", "black", "infrared", "mauve", "red", "white",
        };

        private static List<string> quoteRemark = new List<string>()
        {
            "The remark has since been printed on mugs.",
            "It has already appeared on at least three motivational posters.",
            "A limited-edition tote bag featuring the quote sold out within hours.",
            "The phrase has since been cross-stitched and hung in several public offices.",
            "It was immediately adopted as the slogan of a mid-sized logistics company.",
            "Local artisans have begun embroidering it on cushions.",
            "The quote was spotted on a billboard outside the capital within days.",
            "A follow-up calendar featuring the remark is due in spring.",
        };

        private static List<string> sports = new List<string>()
        {
            "volleyball", "handball", "underwater hockey", "competitive trampolining",
            "rhythmic gymnastics", "table tennis", "curling", "dressage",
            "water polo", "bobsledding", "lacrosse", "synchronized diving",
            "minigolf", "football", "golf", "unicycling", "wrestling", "ice hockey"
        };

        // Freak accident ingredients
        private static List<string> accidentAgents = new List<string>()
        {
            "AI-powered pool noodle", "autonomous water bottle", "self-driving lawnmower",
            "rogue vending machine", "smart umbrella", "sentient traffic cone",
            "bluetooth-enabled garden gnome", "voice-activated cheese grater",
            "subscription-based doorbell", "AI-enhanced park bench",
            "over-eager roomba", "algorithmically optimised broom",
            "cloud-connected watering can", "GPS-guided stapler",
        };

        private static List<string> accidentVerbs = new List<string>()
        {
            "strangulates", "lightly inconveniences", "emotionally overwhelms",
            "rams into", "runs over", "kills", "kidnaps", "shoots", "kills",
            "briefly disorients", "softly confronts", "electrocutes",
            "persistently follows", "injures", "tramples", "decapitates",
        };

        private static List<string> accidentVictims = new List<string>()
        {
            "elderly woman", "retired tax inspector", "local councillor",
            "competitive napper", "visiting dignitary", "amateur geologist",
            "professional mime", "lighthouse keeper", "semi-professional yodeler",
            "beekeeping enthusiast", "part-time lumberjack",
        };

        private static List<string> accidentOutcomes = new List<string>()
        {
            "Authorities are investigating.",
            "The manufacturer has released a firmware update.",
            "A full inquiry has been launched.",
            "The device has been recalled pending review.",
            "Witnesses described the scene as \"unprecedented\".",
            "Legal proceedings are expected to follow.",
            "The victim is reported to be in stable condition.",
            "No comment has been issued by the relevant ministry.",
        };

        // Animal sport combinations
        private static List<string> animalSportRoles = new List<string>()
        {
            "announced their intention to compete in",
            "have formally applied to join the national league for",
            "have been spotted training intensively for",
            "have issued a formal challenge to the reigning champions of",
            "have demanded equal representation in",
            "have staged a walkout from",
        };

        public static NewspaperMinorArticle GenerateFunArticle(Newspaper newspaper)
        {
            NewspaperMinorArticle article = new NewspaperMinorArticle(newspaper);
            article.SetSprite(ResourceManager.LoadSprite($"ElectionTactics/Icons/ArticleIcons/Quote"));
            article.Priority = 1;

            var values = System.Enum.GetValues(typeof(FunArticleType));
            FunArticleType chosenType = (FunArticleType)values.GetValue(Random.Range(0, values.Length));

            switch (chosenType)
            {
                case FunArticleType.TimeFrameComparison: GenerateTimeFrameComparison(article); break;
                case FunArticleType.OutrageHeadline: GenerateOutrage(article); break;
                case FunArticleType.HealthFad: GenerateHealthFad(article); break;
                case FunArticleType.NewTrend: GenerateNewTrend(article); break;
                case FunArticleType.QuoteOfYear: GenerateQuoteOfYear(article); break;
                case FunArticleType.ExpertClaim: GenerateExpertClaim(article); break;
                case FunArticleType.PublicService: GeneratePublicService(article); break;
                case FunArticleType.DangerWarning: GenerateDangerWarning(article); break;
                case FunArticleType.FreakAccident: GenerateFreakAccident(article); break;
                case FunArticleType.SportSwitch: GenerateSportSwitch(article); break;
                case FunArticleType.AnimalSport: GenerateAnimalSport(article); break;
                default:
                    article.SetHeadline("ERROR");
                    article.SetBodyText("error");
                    break;
            }

            return article;
        }

        // --- Shared helpers ---

        /// <summary>
        /// Returns "{authority} {reaction}" as a reaction sentence, capitalized.
        /// E.g. "The cheese board are not amused."
        /// </summary>
        private static string GetAuthorityReaction()
        {
            return $"{authorities.RandomElement().CapitalizeFirst()} {reactions.RandomElement()}.";
        }

        // --- Generators ---

        private static void GenerateTimeFrameComparison(NewspaperMinorArticle article)
        {
            string subjects = nounsPlural.RandomElement();
            article.SetHeadline($"News About {subjects.CapitalizeEachWord()}");
            article.SetBodyText(
                $"The {subjects} have {verbsPast.RandomElement()} {comparator.RandomElement()} this " +
                $"{timeFrames.RandomElement()} than {nounsPlural.RandomElement()} did last " +
                $"{timeFrames.RandomElement()}, owing to {reasonAdjectives.RandomElement()} {reasonNouns.RandomElement()}.");
        }

        private static void GenerateOutrage(NewspaperMinorArticle article)
        {
            string what = concepts.RandomElement();
            article.SetHeadline($"Outrage Over {what.CapitalizeEachWord()}");
            article.SetBodyText(
                $"{GetAuthorityReaction()} " +
                $"The controversy stems from {what}, which has divided opinion across the country. " +
                $"{GetAuthorityReaction()}");
        }

        private static void GenerateHealthFad(NewspaperMinorArticle article)
        {
            string animal = animals.RandomElement();
            article.SetHeadline($"Dangerous Health Fad Sweeps {(animal + "s").CapitalizeFirst()}");
            article.SetBodyText(
                $"A worrying trend has emerged among {animal}s: influencers now encourage them toward " +
                $"{activitiesGerund.RandomElement()}. Experts have been made aware. If your {animal} exhibits " +
                $"{symptoms.RandomElement()}, {remedies.RandomElement()} immediately.");
        }

        private static void GenerateNewTrend(NewspaperMinorArticle article)
        {
            string activity = activitiesGerund.RandomElement();
            string group = nounsPlural.RandomElement();
            article.SetHeadline($"New Trend: {activity.CapitalizeEachWord()}");
            article.SetBodyText(
                $"The nation's {group} have embraced {activity} as the season's must-have pastime. " +
                $"Critics call it {reasonAdjectives.RandomElement()}; enthusiasts insist it is the future. " +
                $"{GetAuthorityReaction()}");
        }

        private static void GenerateQuoteOfYear(NewspaperMinorArticle article)
        {
            string a = colors.RandomElement();
            string b = colors.Where(c => c != a).ToList().RandomElement();
            article.SetHeadline("Quote of the Year");
            article.SetBodyText(
                $"\"{a.CapitalizeFirst()} is the new {b},\" declared a leading figure this year, " +
                $"to widespread {(Random.value < 0.5f ? "acclaim" : "confusion")}. " +
                $"{quoteRemark.RandomElement()}");
        }

        private static void GenerateExpertClaim(NewspaperMinorArticle article)
        {
            string claim = concepts.RandomElement();
            article.SetHeadline($"Experts Weigh In on {claim.CapitalizeEachWord()}");
            article.SetBodyText(
                $"{GetAuthorityReaction()} " +
                $"\"We saw it coming,\" one spokesperson noted, pointing to {reasonAdjectives.RandomElement()} {reasonNouns.RandomElement()}. " +
                $"Skeptics remain unconvinced.");
        }

        private static void GeneratePublicService(NewspaperMinorArticle article)
        {
            string animal = animals.RandomElement();
            article.SetHeadline($"Interested in {animal.WithArticle().CapitalizeEachWord()}?");
            article.SetBodyText(
                $"Good news for animal lovers: mortality among {animal}s has dropped below zero. " +
                $"You can now adopt one via a website that matches you by {matchingAttribute.RandomElement()}. " +
                $"Demand is high, so act fast.");
        }

        private static void GenerateDangerWarning(NewspaperMinorArticle article)
        {
            string consumable = consumables.RandomElement();
            article.SetHeadline($"{consumable.CapitalizeEachWord()} Overconsumption: A Hidden Risk");
            article.SetBodyText(
                $"Health officials warn that {consumable.WithArticle()} a day may no longer keep the doctor away. " +
                $"Overconsumption has been linked to {symptoms.RandomElement()} and {reasonAdjectives.RandomElement()} {reasonNouns.RandomElement()}. " +
                $"Moderation, they stress, is key.");
        }

        private static void GenerateFreakAccident(NewspaperMinorArticle article)
        {
            string agent = accidentAgents.RandomElement();
            string victim = accidentVictims.RandomElement();
            string verb = accidentVerbs.RandomElement();

            article.SetHeadline($"{agent.CapitalizeEachWord()} {verb.CapitalizeFirst()} {victim.WithArticle()}");
            article.SetBodyText(
                $"In what is being described as a first of its kind, {agent.WithArticle()} {verb} {victim.WithArticle()} " +
                $"in {reasonAdjectives.RandomElement()} circumstances near the town centre. " +
                $"{accidentOutcomes.RandomElement()} " +
                $"{GetAuthorityReaction()}");
        }

        private static void GenerateSportSwitch(NewspaperMinorArticle article)
        {
            string from = sports.RandomElement();
            string to = sports.Where(s => s != from).ToList().RandomElement();
            string team = authorities.RandomElement();

            article.SetHeadline($"{team.CapitalizeEachWord()} Announce Switch to {to.CapitalizeEachWord()}");
            article.SetBodyText(
                $"{team.CapitalizeFirst()} have announced they will no longer compete in {from}, " +
                $"citing {reasonAdjectives.RandomElement()} {reasonNouns.RandomElement()} as the deciding factor. " +
                $"They will take up {to} with immediate effect. " +
                $"{GetAuthorityReaction()}");
        }

        private static void GenerateAnimalSport(NewspaperMinorArticle article)
        {
            string animal = animals.RandomElement();
            string sport = sports.RandomElement();
            string role = animalSportRoles.RandomElement();

            article.SetHeadline($"{(animal + "s").CapitalizeFirst()} Demand Place in {sport.CapitalizeEachWord()}");
            article.SetBodyText(
                $"The nation's {animal}s {role} {sport}. " +
                $"Officials have so far declined to respond formally to the request. " +
                $"{GetAuthorityReaction()} " +
                $"A petition has reportedly gathered over three thousand signatures.");
        }
    }
}
