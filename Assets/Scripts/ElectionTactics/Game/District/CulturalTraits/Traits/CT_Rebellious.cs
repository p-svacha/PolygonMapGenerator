using UnityEngine;

namespace ElectionTactics
{
    public class CT_Rebellious : CulturalTrait
    {
        public const int PENALTY_VALUE = ElectionTacticsGame.STANDARD_MODIFIER_VALUE;

        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Negative, PENALTY_VALUE, District.CurrentWinnerParty, 1, "for winning last election", "Rebellious"));
            }
        }

        public override string Description => $"The districts' governing party gets a -{PENALTY_VALUE} penalty in an election.";
    }
}
