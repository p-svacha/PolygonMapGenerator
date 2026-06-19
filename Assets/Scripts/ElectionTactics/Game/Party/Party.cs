using System;
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
        public Color TextColor;
        public bool IsHuman;
        public bool IsLocalPlayer;
        public PartyAI AI;

        // Win conditions
        public int TotalElectionsWon { get; set; }
        public int TotalSeatsWon { get; set; }
        public int TotalDistrictsWon { get; set; }
        public int TotalVotes { get; set; }

        /// <summary>
        /// Acts as the health in the battle royale game mode. Reaching a legitimacy of 0 means dropping out of the game.
        /// </summary>
        public int Legitimacy { get; set; }

        public int PreviousScore; // Score of party before the most recent elections.

        public bool IsEliminated { get; private set; }
        public int FinalRank { get; private set; }

        // Game variables
        public List<Policy> Policies = new List<Policy>();
        public int PolicyPoints;
        public int CampaignPoints;
        public int Seats;
        public bool IsReady;

        public Party(ElectionTacticsGame game, int id, string name, Color color, Color textColor, bool isAi)
        {
            Id = id;
            Game = game;
            Name = name;
            Acronym = PartyNameGenerator.CreateAcronym(Name);
            Color = color;
            TextColor = textColor;
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

        public void Eliminate(int rank)
        {
            IsEliminated = true;
            FinalRank = rank;
            if(Legitimacy < 0) Legitimacy = 0;
        }


        #region Getters

        public Policy GetPolicy(int id)
        {
            return Policies.First(x => x.Id == id);
        }

        public Policy GetPolicy(District district)
        {
            return Policies.OfType<DistrictPolicy>().First(x => x.District == district);
        }

        public Policy GetPolicy(GeographyTraitDef def)
        {
            return Policies.OfType<GeographyPolicy>().First(x => x.TraitDef == def);
        }
        public int GetPolicyValueFor(GeographyTraitDef def)
        {
           return GetPolicy(def).Value;
        }

        public Policy GetPolicy(EconomicSectorDef def)
        {
            return Policies.OfType<EconomyPolicy>().First(x => x.Trait == def);
        }
        public int GetPolicyValueFor(EconomicSectorDef def)
        {
            return GetPolicy(def).Value;
        }

        public Policy GetPolicy(DensityDef d)
        {
            return Policies.OfType<DensityPolicy>().First(x => x.Density == d);
        }
        public int GetPolicyValueFor(DensityDef d)
        {
            return GetPolicy(d).Value;
        }

        public Policy GetPolicy(AgeGroupDef a)
        {
            return Policies.OfType<AgeGroupPolicy>().First(x => x.AgeGroup == a);
        }
        public int GetPolicyValueFor(AgeGroupDef a)
        {
            return GetPolicy(a).Value;
        }

        public Policy GetPolicy(LanguageDef l)
        {
            return Policies.OfType<LanguagePolicy>().First(x => x.Language == l);
        }
        public int GetPolicyValueFor(LanguageDef l)
        {
            return GetPolicy(l).Value;
        }
        public Policy GetPolicy(ReligionDef r)
        {
            if (r == ReligionDefOf.None) return null;
            return Policies.OfType<ReligionPolicy>().First(x => x.Religion == r);
        }
        public int GetPolicyValueFor(ReligionDef r)
        {
            if (r == ReligionDefOf.None) return 0;
            return GetPolicy(r).Value;
        }

        public List<Policy> ActivePolicies { get { return Policies.Where(x => x.IsActive).ToList(); } }

        /// <summary>
        /// Returns the current game score of this party. Wgat the score exactly represents depends on the game mode.
        /// </summary>
        public int GetGameScore()
        {
            if (Game.GameSettings.GameMode == GameModeDefOf.Classic) return TotalElectionsWon;
            else if (Game.GameSettings.GameMode == GameModeDefOf.BattleRoyale) return Legitimacy;

            throw new Exception("Game Mode " + Game.GameSettings.GameMode.Label + " not handled for party game score.");
        }

        #endregion
    }
}
