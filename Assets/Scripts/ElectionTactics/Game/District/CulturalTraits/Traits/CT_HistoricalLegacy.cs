using UnityEngine;

namespace ElectionTactics
{
    public class CT_HistoricalLegacy : CulturalTrait
    {
        public const int BONUS_VALUE = 10;

        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Positive, BONUS_VALUE, District.CurrentWinnerParty, -1, "Historical Legacy Bonus", source: "'Historical Legacy' Cultural Trait"));
            }
        }

        public override string Description => $"The most popular party gets a +{BONUS_VALUE} permanent popularity bonus.";
    }
}
