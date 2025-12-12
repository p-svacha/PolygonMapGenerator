using System.Collections;
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
        public int TotalElectionsWon { get; set; }
        public int TotalSeatsWon { get; set; }
        public int TotalDistrictsWon { get; set; }
        public int TotalVotes { get; set; }

        /// <summary>
        /// In the Battle Royale game mode each party has health. Once this reaches 0, the party is eliminated.
        /// </summary>
        public float BattleRoyaleHealth;

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

        public Policy GetPolicy(int id)
        {
            return Policies.First(x => x.Id == id);
        }

        public Policy GetPolicy(GeographyTraitType t)
        {
            return Policies.OfType<GeographyPolicy>().First(x => x.Trait == t);
        }
        public int GetPolicyValueFor(GeographyTraitType t)
        {
           return GetPolicy(t).Value;
        }

        public Policy GetPolicy(EconomyTrait t)
        {
            return Policies.OfType<EconomyPolicy>().First(x => x.Trait == t);
        }
        public int GetPolicyValueFor(EconomyTrait t)
        {
            return GetPolicy(t).Value;
        }

        public Policy GetPolicy(Density d)
        {
            return Policies.OfType<DensityPolicy>().First(x => x.Density == d);
        }
        public int GetPolicyValueFor(Density d)
        {
            return GetPolicy(d).Value;
        }

        public Policy GetPolicy(AgeGroup a)
        {
            return Policies.OfType<AgeGroupPolicy>().First(x => x.AgeGroup == a);
        }
        public int GetPolicyValueFor(AgeGroup a)
        {
            return GetPolicy(a).Value;
        }

        public Policy GetPolicy(Language l)
        {
            return Policies.OfType<LanguagePolicy>().First(x => x.Language == l);
        }
        public int GetPolicyValueFor(Language l)
        {
            return GetPolicy(l).Value;
        }
        public Policy GetPolicy(Religion r)
        {
            if (r == Religion.None) return null;
            return Policies.OfType<ReligionPolicy>().First(x => x.Religion == r);
        }
        public int GetPolicyValueFor(Religion r)
        {
            if (r == Religion.None) return 0;
            return GetPolicy(r).Value;
        }

        public List<Policy> ActivePolicies { get { return Policies.Where(x => x.IsActive).ToList(); } }

        #endregion
    }
}
