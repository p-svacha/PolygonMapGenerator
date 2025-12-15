using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public static class PartyNameGenerator
    {
        private static Dictionary<string, int> Adjectives = new() // A
        {
            { "Revolutionary", 100 },
            { "Socialist", 100 },
            { "Anarchist", 100 },
            { "Conservative", 100 },
            { "Fascist", 100 },
            { "National", 100 },
            { "Nationalist", 100 },
            { "International", 100 },
            { "Social", 100 },
            { "Democratic", 100 },
            { "Social Democratic", 100 },
            { "Communist", 100 },
            { "Republican", 100 },
            { "Communal", 100 },
            { "Progressive", 100 },
            { "Liberal", 100 },
            { "Libertarian", 100 },
            { "United", 100 },
            { "Eastern", 100 },
            { "Western", 100 },
            { "Northern", 100 },
            { "Southern", 100 },
            { "Radical", 100 },
            { "Red", 100 },
            { "Black", 100 },
            { "Blue", 100 },
            { "Yellow", 100 },
            { "Green", 100 },
            { "Orange", 100 },
            { "White", 100 },
            { "Purple", 100 },
            { "Pink", 100 },
            { "Brown", 100 },
            { "Reformist", 100 },
            { "Capitalist", 100 },
            { "Anti-Capitalist", 100 },
            { "Holy", 100 },
            { "Centrist", 100 },
            { "Populist", 100 },
            { "Federalist", 100 },
            { "Conservationist", 100 },
            { "Marxist", 100 },
            { "Free", 100 },
            { "Individualist", 100 },
            { "Collectivist", 100 },
            { "Queer", 100 },
            { "Imperialist", 100 },
            { "Maoist", 100 },
            { "Great", 100 },
            { "Greater", 100 },
            { "Environmentalist", 100 },
            { "Agrarian", 100 },
            { "New", 100 },
            { "Committed", 100 },
            { "Humanist", 100 },
            { "Civil", 100 },
            { "Alternative", 100 },
        };

        private static Dictionary<string, int> PartyTypeNouns = new() // T
        {
            { "People's", 100 },
            { "Workers'", 100 },
            { "Patriot", 100 },
            { "Common Man's", 100 },
            { "Constitution", 100 },
            { "Elite", 100 },
            { "Mass", 100 },
            { "Niche", 100 },
            { "Civilian", 100 },
            { "Youth", 100 },
            { "Women's", 100 },
            { "Men's", 100 },
            { "Corporate", 100 },
            { "Occupation", 100 },
            { "Rebel", 100 },
            { "Senior", 100 },
            { "Junior", 100 },
            { "Oligarch", 100 },
            { "Monarch", 100 },
            { "Forces", 100 },
            { "Civic", 100 },
            { "Republicans", 100 },
            { "Greens", 100 },
            { "Centre", 100 },
            { "One", 100 },
            { "Reforms", 100 },
        };

        private static Dictionary<string, int> PartyPurposeWords = new() // U
        {
            { "Justice", 100 },
            { "Peace", 100 },
            { "Resurgence", 100 },
            { "Order", 100 },
            { "Law", 100 },
            { "Freedom", 100 },
            { "Liberty", 100 },
            { "Development", 100 },
            { "Liberation", 100 },
            { "Salvation", 100 },
            { "Science", 100 },
            { "Thought", 100 },
            { "Equality", 100 },
            { "Anarchy", 100 },
            { "Integration", 100 },
            { "Unity", 100 },
            { "Human Rights", 100 },
            { "Opportunity", 100 },
            { "Future", 100 },
            { "Society", 100 },
            { "Independence", 100 },
            { "Renewal", 100 },
            { "Production", 100 },
            { "Labour", 100 },
            { "Homeland", 100 },
            { "Motherland", 100 },
            { "Regeneration", 100 },
            { "Mobilization", 100 },
            { "Honor", 100 },
            { "Solidarity", 100 },
            { "Enlightenment", 100 },
            { "Democracy", 100 },
        };

        private static Dictionary<string, int> PartyNouns = new() // P
        {
            { "Party", 400 },

            { "Movement", 200 },
            { "Front", 150 },

            { "Union", 100 },
            { "League", 100 },
            { "Alliance", 100 },
            { "Action", 100 },
            { "Organisation", 100 },
            { "Association", 100 },
            { "Treaty", 100 },
            { "Pact", 100 },
            { "Bloc", 100 },
            { "Federation", 100 },
            { "Coalition", 100 },
            { "Faction", 100 },
            { "Militia", 100 },
            { "Cartel", 100 },
            { "Council", 100 },
            { "Democrats", 100 },
            { "Generation", 100 },
            { "Citizens", 100 },
            { "Nation", 100 },
            { "Voice", 100 },
            { "Network", 100 },

            { "Tribune", 50 },
            { "Tigers", 50 },
            { "Panthers", 50 },
            { "Lions", 50 },
            { "Dawn", 50 },
            { "Guard", 50 },
            { "Proposal", 50 },
            { "Contract", 50 },
        };

        private static Dictionary<string, int> Prepositions = new() // f
        {
            { "of", 100 },
            { "for", 100 },
            { "for the", 40 },
        };


        private static Dictionary<string, int> And = new Dictionary<string, int>() { { "and", 100 } }; // &
        private static Dictionary<string, int> The = new Dictionary<string, int>() { { "The", 100 } }; // t

        private static Dictionary<char, Dictionary<string, int>> ListDict = new Dictionary<char, Dictionary<string, int>>()
        {
            {'A', Adjectives },
            {'T', PartyTypeNouns },
            {'P', PartyNouns },
            {'U', PartyPurposeWords },
            {'f', Prepositions },
            {'&', And },
            {'t', The },
        };

        /// <summary>
        /// This list defines which combinations of the above lists are allowed and with which probabilities they can appear.
        /// </summary>
        private static Dictionary<string, int> NameLanguage = new Dictionary<string, int>()
        {
            // Doubles
            { "AP", 100 },
            { "TP", 100 },
            { "UP", 100 },

            { "PfU", 80 },
            { "U&U", 50 },

            { "tP", 10 },

            // Triples
            { "ATP", 100 },
            { "AUP", 80 },
            { "TUP", 80 },
            { "TAP", 40 },

            { "APfU", 80 },
            { "TPfU", 60 },
            { "UPfU", 60 },
            { "U&UP", 50 },

            { "tAP", 10 },
            { "tTP", 10 },
            { "tUP", 10 },
            { "tPfU", 10 },

            // Fourlets
            { "ATUP", 50 },

            // Special misc
            { "PfU&U", 50 },
            { "PfUfU", 10 },
            { "PfU,U&U", 20 },
            { "U,U&UP", 20 },
            { "UfUP", 20 },
            { "APfAU", 20 },
        };

        private static List<Color> Colors = new List<Color>()
        {
            new Color(0.80f, 0.80f, 0.80f), // 0 - White
            new Color(0.00f, 0.50f, 0.00f), // 1 - Green
            new Color(0.10f, 0.40f, 0.90f), // 2 - Blue
            new Color(0.80f, 0.00f, 0.00f), // 3 - Red
            new Color(0.80f, 0.75f, 0.15f), // 4 - Yellow
            new Color(0.10f, 0.10f, 0.10f), // 5 - Black
            new Color(0.85f, 0.60f, 0.16f), // 6 - Orange
            new Color(0.80f, 0.00f, 0.80f), // 7 - Purple
            new Color(0.66f, 0.32f, 0.10f), // 8 - Brown
            new Color(0.00f, 0.80f, 0.90f), // 9 - Light Blue
            new Color(0.00f, 0.90f, 0.00f), // 10 - Light Green
        };


        public static string GetRandomPartyName(int maxLength = 0, bool log = true)
        {
            string name = "";

            string nameType = NameLanguage.GetWeightedRandomElement();
            foreach (char c in nameType)
            {
                if (c == ',')
                {
                    name = name.TrimEnd(' ');
                    name += ", ";
                }

                else if (ListDict.ContainsKey(c))
                {
                    Dictionary<string, int> list = ListDict[c];
                    name += GetRandomWord(name, list) + " ";
                }

                else throw new System.Exception($"Character {c} is not handled in party name generation language.");
            }
            name = name.Trim(' ');

            if (maxLength > 0 && maxLength < 16)
            {
                maxLength = 0;
                Debug.LogWarning($"maxLength is getting ignored because it was below 16. Too dangerous to not find any names.");
            }
            while (maxLength > 0 && name.Length > maxLength) name = GetRandomPartyName(log: false);

            if (log) Debug.Log($"Generated party name '{name}' out of language word '{nameType}'.");
            return name;
        }

        public static Color GetPartyColor(string partyName, List<Color> alreadyTaken)
        {
            if (partyName.Contains("White") && !alreadyTaken.Contains(Colors[0])) return Colors[0];
            if (partyName.Contains("Green") && !alreadyTaken.Contains(Colors[1])) return Colors[1];
            if (partyName.Contains("Blue") && !alreadyTaken.Contains(Colors[2])) return Colors[2];
            if (partyName.Contains("Red") && !alreadyTaken.Contains(Colors[3])) return Colors[3];
            if (partyName.Contains("Yellow") && !alreadyTaken.Contains(Colors[4])) return Colors[4];
            if (partyName.Contains("Black") && !alreadyTaken.Contains(Colors[5])) return Colors[5];
            if (partyName.Contains("Orange") && !alreadyTaken.Contains(Colors[6])) return Colors[6];
            if ((partyName.Contains("Purple") || partyName.Contains("Pink")) && !alreadyTaken.Contains(Colors[7])) return Colors[7];
            if (partyName.Contains("Brown") && !alreadyTaken.Contains(Colors[8])) return Colors[8];
            else return GetRandomColor(alreadyTaken);
        }

        public static Color GetRandomColor(List<Color> forbiddenColors)
        {
            // Check if we have run out of unique colors
            if (forbiddenColors.Count >= Colors.Count)
            {
                Debug.LogWarning("All party colors taken! Returning white as fallback.");
                return Color.white;
            }

            // Return random remaining color
            Color c = Colors.RandomElement();
            while(forbiddenColors.Contains(c))
            {
                c = Colors.RandomElement();
            }
            return c;
        }

        private static string GetRandomWord(string partyName, Dictionary<string, int> wordList)
        {
            string word = wordList.GetWeightedRandomElement();
            while (partyName.Contains(word)) word = wordList.GetWeightedRandomElement();
            return word;
        }

        public static string CreateAcronym(string partyName)
        {
            // Special rule if party name is one word
            if (!partyName.Contains(' ')) return partyName.Substring(0, Mathf.Min(partyName.Length, 3));

            // Acronym is first letter of each capitalized word
            string acronym = "";
            foreach (string word in partyName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (char.IsUpper(word[0])) acronym += word[0];
                else if (word == "and") acronym += "&";
            }
            return acronym;
        }
    }
}
