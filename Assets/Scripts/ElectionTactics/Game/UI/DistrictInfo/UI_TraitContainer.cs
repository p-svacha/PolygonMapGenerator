using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_TraitContainer : MonoBehaviour
    {
        [Header("Display Settings")]
        public int MaxTraitsPerRow = 3;

        [Header("Elements")]
        public GameObject RowContainer;

        [Header("Prefabs")]
        public UI_TraitRow TraitRowPrefab;
        public UI_Trait TraitPrefab;

        public void InitGeographyTraits(District d)
        {
            HelperFunctions.DestroyAllChildredImmediately(RowContainer);

            List<GeographyTrait> traits = d.Geography;
            traits = traits.OrderByDescending(t => t.Category).ToList();

            UI_TraitRow currentRow = null;
            for (int i = 0; i < traits.Count; i++)
            {
                if (i %  MaxTraitsPerRow == 0)
                {
                    currentRow = Instantiate(TraitRowPrefab, RowContainer.transform);
                    HelperFunctions.DestroyAllChildredImmediately(currentRow.gameObject);
                }
                UI_Trait traitElem = Instantiate(TraitPrefab, currentRow.transform);
                traitElem.InitGeographyTrait(traits[i]);
            }
        }

        public void InitCulturalTraits(District d)
        {
            HelperFunctions.DestroyAllChildredImmediately(RowContainer);

            List<CulturalTrait> traits = d.CulturalTraits;

            UI_TraitRow currentRow = null;
            for (int i = 0; i < traits.Count; i++)
            {
                if (i % MaxTraitsPerRow == 0)
                {
                    currentRow = Instantiate(TraitRowPrefab, RowContainer.transform);
                    HelperFunctions.DestroyAllChildredImmediately(currentRow.gameObject);
                }
                UI_Trait traitElem = Instantiate(TraitPrefab, currentRow.transform);
                traitElem.InitCulturalTrait(traits[i]);
            }
        }
    }
}
