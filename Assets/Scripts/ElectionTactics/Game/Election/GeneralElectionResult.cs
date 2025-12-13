using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    [System.Serializable]
    public class GeneralElectionResult
    {
        public int ElectionCycle;
        public int Year;
        public List<DistrictElectionResult> DistrictResults;

        public Dictionary<Party, int> SeatsWon;
        public List<Party> WinnerParties;

        public GeneralElectionResult(int electionCycle, int year, List<DistrictElectionResult> districtResults)
        {
            ElectionCycle = electionCycle;
            Year = year;
            DistrictResults = districtResults;

            SeatsWon = new Dictionary<Party, int>();
            foreach (DistrictElectionResult dr in DistrictResults) SeatsWon.IncrementMultiple(dr.SeatsWon);
            WinnerParties = SeatsWon.Where(x => x.Value == SeatsWon.Values.Max()).Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Applies the result to the game, meaning that it will be added to the history and victory points will be awarded accordingly.
        /// </summary>
        public void Apply(ElectionTacticsGame game)
        {
            // Add result to the game
            game.AddGeneralElectionResult(this);

            // Save state before elections (used for election animation)
            foreach (Party p in game.Parties) p.PreviousScore = p.GetGameScore();

            // Apply district election results
            foreach (DistrictElectionResult result in DistrictResults) result.Apply(game);

            // Award total election victory points
            foreach (Party p in WinnerParties) p.TotalElectionsWon++;

            // Add legitimacy bonus for winners (battle royale only)
            foreach (Party p in WinnerParties) p.Legitimacy += GetWinnerLegitimacyBonus();
        }

        public int GetWinnerLegitimacyBonus()
        {
            return ElectionTacticsGame.BR_BASE_HEAL_PER_ELECTION_WON + ElectionCycle * ElectionTacticsGame.BR_HEAL_PER_ELECTION_WON_PER_TURN;
        }
    }
}
