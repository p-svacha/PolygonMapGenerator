﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics {
    public class Party
    {
        public ElectionTacticsGame Game;

        // Static attributes
        public int Id;
        public string Name;
        public string Acronym;
        public Color Color;
        public bool IsHuman;
        public PartyAI AI;

        // Win conditions
        public int TotalElectionsWon;
        public int TotalSeatsWon;
        public int TotalDistrictsWon;
        public int TotalVotes;

        // Game variables
        public List<Policy> Policies = new List<Policy>();
        public int PolicyPoints;
        public int CampaignPoints;
        public int Seats;
        public bool IsReady;

        public Party(ElectionTacticsGame game, int id, string name, Color c, bool isAi)
        {
            Id = id;
            Game = game;
            Name = name;
            Acronym = "";
            string[] words = name.Split(' ');
            foreach (string w in words) Acronym += (w[0] + "").ToUpper(); 
            Color = c;
            IsHuman = !isAi;
            if (isAi)
            {
                AI = new PartyAI(this);
                IsReady = true;
            }
        }

        public void AddPolicy(Policy p)
        {
            Policies.Add(p);
        }


        #region Getters

        public int GetPolicyValueFor(GeographyTraitType t)
        {
            return Policies.OfType<GeographyPolicy>().First(x => x.Trait == t).Value;
        }
        public int GetPolicyValueFor(EconomyTrait t)
        {
            return Policies.OfType<EconomyPolicy>().First(x => x.Trait == t).Value;
        }
        public int GetPolicyValueFor(Density d)
        {
            return Policies.OfType<DensityPolicy>().First(x => x.Density == d).Value;
        }
        public int GetPolicyValueFor(AgeGroup a)
        {
            return Policies.OfType<AgeGroupPolicy>().First(x => x.AgeGroup == a).Value;
        }
        public int GetPolicyValueFor(Language l)
        {
            return Policies.OfType<LanguagePolicy>().First(x => x.Language == l).Value;
        }
        public int GetPolicyValueFor(Religion r)
        {
            if (r == Religion.None) return 0;
            return Policies.OfType<ReligionPolicy>().First(x => x.Religion == r).Value;
        }

        #endregion
    }
}
