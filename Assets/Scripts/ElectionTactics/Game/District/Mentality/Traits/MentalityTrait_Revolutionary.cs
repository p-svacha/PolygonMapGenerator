using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Revolutionary : MentalityTrait
    {
        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Exclusion, District.CurrentWinnerParty, 1, "Excluded for winning last election", "Revolutionary Mentality"));
            }
        }
    }
}
