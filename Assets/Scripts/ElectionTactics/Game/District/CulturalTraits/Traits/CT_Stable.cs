using UnityEngine;

namespace ElectionTactics
{
    public class CT_Stable : CulturalTrait
    {
        public const int BONUS_VALUE = ElectionTacticsGame.STANDARD_MODIFIER_VALUE;

        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Positive, BONUS_VALUE, District.CurrentWinnerParty, 1, "for winning last election", "'Stable' Cultural Trait"));
            }
        }

        public override string Description => $"The districts' governing party gets a +{BONUS_VALUE} bonus in an election.";
    }
}
