using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Constitution
    {
        private ElectionTacticsGame Game;

        public List<ConstitutionChapter> Chapters = new List<ConstitutionChapter>();
        public WinCondition WinCondition;

        public Constitution(ElectionTacticsGame game)
        {
            Game = game;
            WinCondition = new WinCondition(game);
            Chapters.Add(WinCondition);
        }

        public string GetConstitutionText()
        {
            string cons = "";

            int counter = 1;
            foreach(ConstitutionChapter cc in Chapters)
            {
                cons += counter + ". Win Condition\n";
                cons += WinCondition.GetConstitutionText();
                cons += "\n\n";
                counter++;
            }
            
            return cons;
        }
    }
}
