using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class Mentality
    {
        public MentalityType Type { get; protected set; }
        public string Description { get; protected set; }

        public Mentality(MentalityType type)
        {
            Type = type;
            
            switch(type)
            {
                case MentalityType.Undecided:
                    Description = "Parties have a very low base chance of being voted for.";
                    break;

                case MentalityType.Decided:
                    Description = "Parties have a very low base chance of being voted for.";
                    break;

                case MentalityType.Religious:
                    Description = "Religion policy effect is doubled.";
                    break;

                case MentalityType.Secular:
                    Description = "Religion policy effect is halved.";
                    break;

                case MentalityType.Linguistic:
                    Description = "Language policy effect is doubled.";
                    break;

                case MentalityType.Nonlinguistic:
                    Description = "Language policy effect is halved.";
                    break;

                case MentalityType.Predictable:
                    Description = "Higher voter turnout leads to consistent results.";
                    break;

                case MentalityType.Unpredictable:
                    Description = "Due to low voter turnout results can vary a lot.";
                    break;

                default:
                    throw new System.Exception("MentalityType not handled");
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

                case MentalityType.Religious:
                    return (d.Religion != Religion.None && !d.MentalityTypes.Contains(MentalityType.Secular));
                case MentalityType.Secular:
                    return (d.Religion != Religion.None && !d.MentalityTypes.Contains(MentalityType.Religious));

                case MentalityType.Linguistic:
                    return !d.MentalityTypes.Contains(MentalityType.Nonlinguistic);
                case MentalityType.Nonlinguistic:
                    return !d.MentalityTypes.Contains(MentalityType.Linguistic);

                case MentalityType.Predictable:
                    return !d.MentalityTypes.Contains(MentalityType.Unpredictable);
                case MentalityType.Unpredictable:
                    return !d.MentalityTypes.Contains(MentalityType.Predictable);
            }
            throw new System.Exception("MentalityType not handled in CanAdoptMentality()");
        }
    }
}
