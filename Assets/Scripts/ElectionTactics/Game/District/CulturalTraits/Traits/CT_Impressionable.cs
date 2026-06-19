using System.Collections.Generic;

namespace ElectionTactics
{
    public class CT_Impressionable : CulturalTrait
    {
        private const int BONUS_PER_DISTRICT = 10;

        public override void OnDistrictActivated()
        {
            ApplyModifiersFromElection();
        }

        public override void OnPostElection()
        {
            ApplyModifiersFromElection();
        }

        private void ApplyModifiersFromElection()
        {
            Dictionary<Party, int> bonuses = new Dictionary<Party, int>();

            // Calculate bonuses
            foreach (District neighbour in District.ActiveLandNeighbours)
            {
                DistrictElectionResult result = neighbour.GetLatestElectionResult();
                if (result == null) continue;

                bonuses.Increment(result.WinnerParty, BONUS_PER_DISTRICT);
            }

            // Apply modifiers
            foreach (var x in bonuses)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Positive, x.Value, x.Key, 1, "Impressionable Bonus", "Impressionable Cultural Trait"));
            }
        }
    }
}
