using UnityEngine;

namespace ElectionTactics
{
    public class MentalityTrait_Rebellious : CulturalTrait
    {
        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Negative, District.CurrentWinnerParty, 1, "Malus for winning last election", "Rebellious Mentality"));
            }
        }
    }
}
