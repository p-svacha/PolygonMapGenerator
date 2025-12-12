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

        public GeneralElectionResult(int electionCycle, int year, List<DistrictElectionResult> districtResults)
        {
            ElectionCycle = electionCycle;
            Year = year;
            DistrictResults = districtResults;
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
            List<Party> winnerParties = game.Parties.Where(x => x.Seats == game.Parties.Max(y => y.Seats)).ToList();
            foreach (Party p in winnerParties) p.TotalElectionsWon++;

            // Add legitimacy bonus for winners (battle royale only)
            int legitimacyBonus = ElectionTacticsGame.BR_BASE_HEAL_PER_ELECTION_WON + ElectionTacticsGame.Instance.ElectionCycle * ElectionTacticsGame.BR_HEAL_PER_ELECTION_WON_PER_TURN;
            foreach (Party p in winnerParties) p.Legitimacy += legitimacyBonus;
        }
    }
}
