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

        public Sprite GetLanguageIcon(Language language)
        {
            if (language == Language.Arabic) return ArabicSprite;
            if (language == Language.Bengali) return BengaliSprite;
            if (language == Language.English) return EnglishSprite;
            if (language == Language.French) return FrenchSprite;
            if (language == Language.German) return GermanSprite;
            if (language == Language.Hindi) return HindiSprite;
            if (language == Language.Mandarin) return MandarinSprite;
            if (language == Language.Portuguese) return PortugueseSprite;
            if (language == Language.Russian) return RussianSprite;
            if (language == Language.Spanish) return SpanishSprite;
            throw new System.Exception("Language sprite not found for " + language.ToString());
        }

        public Sprite GetReligionIcon(Religion religion)
        {
            if (religion == Religion.Buddhist) return BuddhismSprite;
            if (religion == Religion.Christian) return ChristianitySprite;
            if (religion == Religion.Hindu) return HinduismSprite;
            if (religion == Religion.Jewish) return JudaismSprite;
            if (religion == Religion.Muslim) return IslamSprite;
            throw new System.Exception("Religion sprite not found for " + religion.ToString());
        }

        public Sprite GetDensityIcon(Density density)
        {
            if (density == Density.Urban) return UrbanSprite;
            if (density == Density.Suburban) return SuburbanSprite;
            if (density == Density.Rural) return RuralSprite;
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
