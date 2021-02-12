using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class WinCondition : ConstitutionChapter
    {
        public WinConditionType Type;
        public WinConditionTime Time;

        public int ConditionValue;

        private const int MinElectionWins = 5;
        private const int MaxElectionWins = 10;
        private const int StepElectionWins = 1;

        private const int MinSeatWins = 100;
        private const int MaxSeatWins = 500;
        private const int StepSeatWins = 10;

        private const int MinDistrictWins = 40;
        private const int MaxDistrictWins = 100;
        private const int StepDistrictWins = 5;

        private const int MinTotalVotes = 1000000;
        private const int MaxTotalVotes = 5000000;
        private const int StepTotalVotes = 100000;

        private const int MinYear = 2040;
        private const int MaxYear = 2100;
        private const int StepYear = 4;
        

        public WinCondition(ElectionTacticsGame game) : base(game)
        {
            ChapterTitle = "Win Condition";
        }

        protected override void SetInitialRandomValues()
        {
            Type = GetRandomType();
            Time = GetRandomTime();
            if (Time == WinConditionTime.Year) ConditionValue = GetRandomConditionValue(MinYear, MaxYear, StepYear);
            else
            {
                if (Type == WinConditionType.TotalElectionsWon) ConditionValue = GetRandomConditionValue(MinElectionWins, MaxElectionWins, StepElectionWins);
                else if (Type == WinConditionType.TotalSeatsWon) ConditionValue = GetRandomConditionValue(MinSeatWins, MaxSeatWins, StepSeatWins);
                else if (Type == WinConditionType.TotalDistrictsWon) ConditionValue = GetRandomConditionValue(MinDistrictWins, MaxDistrictWins, StepDistrictWins);
                else if (Type == WinConditionType.TotalVotes) ConditionValue = GetRandomConditionValue(MinTotalVotes, MaxTotalVotes, StepTotalVotes);
            }
        }

        public override string GetConstitutionText()
        {
            string type = "";
            if (Type == WinConditionType.TotalElectionsWon) type = "election wins";
            else if (Type == WinConditionType.TotalSeatsWon) type = "total seats";
            else if (Type == WinConditionType.TotalDistrictsWon) type = "total districts";
            else if (Type == WinConditionType.TotalVotes) type = "total votes";

            string time = "";
            if (Time == WinConditionTime.Year) time = "in the year " + ConditionValue;
            else if (Time == WinConditionTime.Value) time = "when the first party has reached " + ConditionValue + " " + type;

            string text = "The party with the most " + type + " wins the game.\nThe game ends " + time;

            return text;
        }

        public Party GetWinner()
        {
            return GetWinner(Type, Time, ConditionValue);
        }

        /// <summary>
        /// Returns the winner party. If the game is not over yet, returns null.
        /// </summary>
        private Party GetWinner(WinConditionType type, WinConditionTime time, int conditionValue)
        {
            switch(time)
            {
                case WinConditionTime.Year:
                    if (Game.Year < conditionValue) return null;
                    else return GetCurrentLeader(type);

                case WinConditionTime.Value:
                    Party leader = GetCurrentLeader(type);
                    switch(type)
                    {
                        case WinConditionType.TotalElectionsWon:
                            if (leader.TotalElectionsWon < conditionValue) return null;
                            else return leader;
                        case WinConditionType.TotalSeatsWon:
                            if (leader.TotalSeatsWon < conditionValue) return null;
                            else return leader;
                        case WinConditionType.TotalDistrictsWon:
                            if (leader.TotalDistrictsWon < conditionValue) return null;
                            else return leader;
                        case WinConditionType.TotalVotes:
                            if (leader.TotalVotes < conditionValue) return null;
                            else return leader;
                    }
                    throw new System.Exception("Win condition type not handled");
            }
            throw new System.Exception("Win condition time not handled");
        }

        private Party GetCurrentLeader(WinConditionType type)
        {
            switch(type)
            {
                case WinConditionType.TotalElectionsWon:
                    return Game.Parties.OrderByDescending(x => x.TotalElectionsWon).First();
                case WinConditionType.TotalDistrictsWon:
                    return Game.Parties.OrderByDescending(x => x.TotalDistrictsWon).First();
                case WinConditionType.TotalSeatsWon:
                    return Game.Parties.OrderByDescending(x => x.TotalSeatsWon).First();
                case WinConditionType.TotalVotes:
                    return Game.Parties.OrderByDescending(x => x.TotalVotes).First();
            }
            throw new System.Exception("Win condition type not handled");
        }

        private static WinConditionType GetRandomType()
        {
            Array values = Enum.GetValues(typeof(WinConditionType));
            return (WinConditionType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        private static WinConditionTime GetRandomTime()
        {
            Array values = Enum.GetValues(typeof(WinConditionTime));
            return (WinConditionTime)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }

    public enum WinConditionType
    {
        TotalElectionsWon,
        TotalSeatsWon,      
        TotalDistrictsWon,
        TotalVotes
    }

    public enum WinConditionTime
    {
        Year,   // The game ends at a certain year
        Value   // The game ends when a party has reached a certain value at the win condition
    }
}
