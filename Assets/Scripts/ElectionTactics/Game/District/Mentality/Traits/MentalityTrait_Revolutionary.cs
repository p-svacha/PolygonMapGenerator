using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Revolutionary : CulturalTrait
    {
        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Exclusion, 0, District.CurrentWinnerParty, 1, "for winning last election", "Revolutionary Mentality"));
            }
        }

        public override string Description => $"The districts' governing party is excluded from the election.";
    }
}
