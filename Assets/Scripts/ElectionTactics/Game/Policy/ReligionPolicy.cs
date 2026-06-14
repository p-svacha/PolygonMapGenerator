using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class ReligionPolicy : Policy
    {
        public ReligionDef Religion;

        public ReligionPolicy(int id, Party p, ReligionDef religion, int maxValue) : base(id, p, maxValue)
        {
            Religion = religion;
            Name = religion.Label;
            Sprite = religion == ReligionDefOf.None ? null : religion.Sprite;
            Type = PolicyType.Religion;
        }

        public override int GetSinglePointBaseImpact(District district)
        {
            if (Religion != district.Religion) return 0;

            return MEDIUM_POPULARITY_IMPACT;
        }
    }
}