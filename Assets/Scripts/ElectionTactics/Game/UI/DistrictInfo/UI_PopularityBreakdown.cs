using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_PopularityBreakdown : MonoBehaviour
    {
        [Header("Prefabs")]
        public UI_PopularityBreakdownEntry Entry;

        public void Init(District district, Party party)
        {
            // Clear list
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

            Dictionary<string, int> popularityBreakdown = district.GetPartyPopularityBreakdown(party);
            foreach(KeyValuePair<string, int> factor in popularityBreakdown.Where(x => x.Value != 0))
            {
                UI_PopularityBreakdownEntry entry = Instantiate(Entry, transform);
                entry.Init(factor.Key, factor.Value);
            }
        }
    }
}
