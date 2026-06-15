using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class CT_HistoricalLegacy : CulturalTrait
    {
        public const int BONUS_VALUE = 10;
        private const string MODIFIER_DESC = "Historical Legacy Bonus";

        public override void OnPostElection()
        {
            if (District.CurrentWinnerParty != null)
            {
                Game.AddModifier(District, new Modifier(ModifierType.Positive, BONUS_VALUE, District.CurrentWinnerParty, -1, MODIFIER_DESC, source: "'Historical Legacy' Cultural Trait"));

                // Write an article if a party reaches 100
                if (District.Modifiers.Any(m => m.Value == 100 && m.Party == District.CurrentWinnerParty && m.Description == MODIFIER_DESC))
                {
                    Game.RegisterNewsEvent(new NewsEvent_HistoricalLegacy100(District, District.CurrentWinnerParty));
                }
            }
        }

        public override string Description => $"The most popular party gets a +{BONUS_VALUE} permanent popularity bonus.";
    }
}
