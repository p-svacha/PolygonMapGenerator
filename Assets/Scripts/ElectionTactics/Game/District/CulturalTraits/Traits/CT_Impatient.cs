using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionTactics
{
    public class CT_Impatient : CulturalTrait
    {
        public const int THRESHOLD = 8;

        public Dictionary<Party, int> ExclusionCountdown;

        protected override void OnInit()
        {
            ExclusionCountdown = new Dictionary<Party, int>();
            foreach (Party p in Game.Parties) ExclusionCountdown.Add(p, 0);
        }

        public override void OnPostElection()
        {
            DistrictElectionResult result = District.GetLatestElectionResult();
            if (result == null) return;

            foreach (Party p in result.Parties)
            {
                if (result.SeatsWon[p] == 0) ExclusionCountdown[p]++;
                else ExclusionCountdown[p] = 0;

                if (ExclusionCountdown[p] == THRESHOLD)
                {
                    Game.AddModifier(District, new Modifier(ModifierType.Exclusion, 0, p, -1, "for repeatedly failing to win seats", "Impatient Cultural Trait"));

                    Game.RegisterNewsEvent(new NewsEvent_ImpatientExclusion(District, p));
                }
            }
        }

        public override string Description => base.Description + $"\n\nCurrently at {UnityEngine.Mathf.Min(ExclusionCountdown[Game.LocalPlayerParty], THRESHOLD)}/{THRESHOLD}";
    }
}
