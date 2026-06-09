using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class IconManager : MonoBehaviour
    {
        [Header("Modifiers")]
        public Sprite ModifierNegativeIcon;
        public Sprite ModifierPositiveIcon;
        public Sprite ModifierExclusionIcon;

        [Header("Language")]
        public Sprite EnglishSprite;
        public Sprite MandarinSprite;
        public Sprite SpanishSprite;
        public Sprite FrenchSprite;
        public Sprite HindiSprite;
        public Sprite BengaliSprite;
        public Sprite GermanSprite;
        public Sprite PortugueseSprite;
        public Sprite RussianSprite;
        public Sprite ArabicSprite;

        [Header("Religion")]
        public Sprite ChristianitySprite;
        public Sprite HinduismSprite;
        public Sprite IslamSprite;
        public Sprite JudaismSprite;
        public Sprite BuddhismSprite;

        [Header("Density")]
        public Sprite UrbanSprite;
        public Sprite SuburbanSprite;
        public Sprite RuralSprite;

        public Sprite GetModifierIcon(ModifierType type)
        {
            if (type == ModifierType.Positive) return ModifierPositiveIcon;
            if (type == ModifierType.Negative) return Singleton.ModifierNegativeIcon;
            if (type == ModifierType.Exclusion) return Singleton.ModifierExclusionIcon;
            throw new System.Exception("Modifier sprite not found for " + type.ToString());
        } 

        
        public Sprite GetLanguageIcon(LanguageDef language)
        {
            if (language == LanguageDefOf.Arabic) return ArabicSprite;
            if (language == LanguageDefOf.Bengali) return BengaliSprite;
            if (language == LanguageDefOf.English) return EnglishSprite;
            if (language == LanguageDefOf.French) return FrenchSprite;
            if (language == LanguageDefOf.German) return GermanSprite;
            if (language == LanguageDefOf.Hindi) return HindiSprite;
            if (language == LanguageDefOf.Mandarin) return MandarinSprite;
            if (language == LanguageDefOf.Portuguese) return PortugueseSprite;
            if (language == LanguageDefOf.Russian) return RussianSprite;
            if (language == LanguageDefOf.Spanish) return SpanishSprite;
            throw new System.Exception("Language sprite not found for " + language.ToString());
        }

        public Sprite GetReligionIcon(ReligionDef religion)
        {
            if (religion == ReligionDefOf.Buddhist) return BuddhismSprite;
            if (religion == ReligionDefOf.Christian) return ChristianitySprite;
            if (religion == ReligionDefOf.Hindu) return HinduismSprite;
            if (religion == ReligionDefOf.Jewish) return JudaismSprite;
            if (religion == ReligionDefOf.Muslim) return IslamSprite;
            throw new System.Exception("Religion sprite not found for " + religion.ToString());
        }

        public Sprite GetDensityIcon(DensityDef density)
        {
            if (density == DensityDefOf.High) return UrbanSprite;
            if (density == DensityDefOf.Medium) return SuburbanSprite;
            if (density == DensityDefOf.Low) return RuralSprite;
            throw new System.Exception("Density sprite not found for " + density.ToString());
        }
        

        public static IconManager Singleton
        {
            get
            {
                return GameObject.Find("IconManager").GetComponent<IconManager>();
            }
        }
    }
}
