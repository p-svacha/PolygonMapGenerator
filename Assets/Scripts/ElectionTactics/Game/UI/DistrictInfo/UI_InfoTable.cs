using ElectionTactics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_InfoTable : MonoBehaviour
{
    [Header("Elements")]
    public GameObject RowContainer;

    [Header("Prefabs")]
    public UI_InfoTableRow RowPrefab;

    public void InitPopularityBreakdown(District district, Party party)
    {
        // Clear list
        for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

        List<(string Label, int Value)> popularityBreakdown = district.GetPartyPopularityBreakdown(party, includeOtherDistrictPopularityInfluence: true);
        foreach (var factor in popularityBreakdown.Where(x => x.Value != 0))
        {
            UI_InfoTableRow entry = Instantiate(RowPrefab, transform);
            int value = factor.Value;
            string valueText = value >= 0 ? "+" + value : value.ToString();
            entry.Init(factor.Label, valueText);
        }
    }
}
