﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Mentality
    {
        public MentalityType Type { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public Mentality(MentalityType type)
        {
            Type = type;
            
            switch(type)
            {
                case MentalityType.Undecided:
                    Name = "Undecided";
                    Description = "Parties have a very high\nbase chance of being voted for.";
                    break;

                case MentalityType.Decided:
                    Name = "Decided";
                    Description = "Parties have a very low base chance of being voted for.";
                    break;

                case MentalityType.Religious:
                    Name = "Religious";
                    Description = "Religion policy effectiveness in this district is doubled.";
                    break;

                case MentalityType.Secular:
                    Name = "Secular";
                    Description = "Religion policy effectiveness in this district is halved.";
                    break;

                case MentalityType.Linguistic:
                    Name = "Linguistic";
                    Description = "Language policy effectiveness in this district is doubled.";
                    break;

                case MentalityType.Nonlinguistic:
                    Name = "Non-Linguistic";
                    Description = "Language policy effectiveness in this district is halved.";
                    break;

                case MentalityType.HighVoterTurnout:
                    Name = "High Voter Turnout";
                    Description = "A big portion of the population in this district will cast their vote.";
                    break;

                case MentalityType.LowVoterTurnout:
                    Name = "Low Voter Turnout";
                    Description = "A low portion of the population in this district will cast their vote.";
                    break;

                case MentalityType.Rebellious:
                    Name = "Rebellious";
                    Description = "The party that won the last election will get a malus for the next one";
                    break;

                case MentalityType.Stable:
                    Name = "Stable";
                    Description = "The party that won the last election will get a bonus for the next one";
                    break;

                case MentalityType.Revolutionary:
                    Name = "Revolutionary";
                    Description = "The party that won the last election will be excluded for the next one";
                    break;

                case MentalityType.Predictable:
                    Name = "Predictable";
                    Description = "Election results reflect the popularity of the parties very well which will lead to very consistent and non-random results.";
                    break;

                case MentalityType.Unpredictable:
                    Name = "Unpredictable";
                    Description = "Election results reflect the popularity of the parties poorly which can lead to inconsistent and random results.";
                    break;

                default:
                    throw new System.Exception("MentalityType not handled for Mentality Name & Description");
            }
        }

        public bool CanAdoptMentality(District d)
        {
            if (d.MentalityTypes.Contains(Type)) return false;

            switch(Type)
            {
                case MentalityType.Decided:
                    return !d.MentalityTypes.Contains(MentalityType.Undecided);
                case MentalityType.Undecided:
                    return !d.MentalityTypes.Contains(MentalityType.Decided);

                case MentalityType.Predictable:
                    return !d.MentalityTypes.Contains(MentalityType.Unpredictable);
                case MentalityType.Unpredictable:
                    return !d.MentalityTypes.Contains(MentalityType.Predictable);

                case MentalityType.Religious:
                    return (d.Religion != Religion.None && !d.MentalityTypes.Contains(MentalityType.Secular));
                case MentalityType.Secular:
                    return (d.Religion != Religion.None && !d.MentalityTypes.Contains(MentalityType.Religious));

                case MentalityType.Linguistic:
                    return !d.MentalityTypes.Contains(MentalityType.Nonlinguistic);
                case MentalityType.Nonlinguistic:
                    return !d.MentalityTypes.Contains(MentalityType.Linguistic);

                case MentalityType.HighVoterTurnout:
                    return !d.MentalityTypes.Contains(MentalityType.LowVoterTurnout);
                case MentalityType.LowVoterTurnout:
                    return !d.MentalityTypes.Contains(MentalityType.HighVoterTurnout);

                case MentalityType.Rebellious:
                    return (!d.MentalityTypes.Contains(MentalityType.Stable) && !d.MentalityTypes.Contains(MentalityType.Revolutionary));
                case MentalityType.Stable:
                    return (!d.MentalityTypes.Contains(MentalityType.Rebellious) && !d.MentalityTypes.Contains(MentalityType.Revolutionary));
                case MentalityType.Revolutionary:
                    return (!d.MentalityTypes.Contains(MentalityType.Rebellious) && !d.MentalityTypes.Contains(MentalityType.Stable));
            }
            throw new System.Exception("MentalityType " + Type + " not handled in CanAdoptMentality()");
        }
    }
}
