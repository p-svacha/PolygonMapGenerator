using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Stable : MentalityTrait
    {
        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Positive, District.CurrentWinnerParty, 1, "Bonus for winning last election", "Stable Mentality"));
            }
        }
    }
}
