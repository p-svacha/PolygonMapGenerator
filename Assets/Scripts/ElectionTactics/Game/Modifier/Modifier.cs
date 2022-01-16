using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// Modifiers are temporary effects that apply to a party in a specific district.
    /// </summary>
    [System.Serializable]
    public class Modifier
    {
        public ModifierType Type;
        public int RegionId;
        public int PartyId;
        [System.NonSerialized] public District District;
        [System.NonSerialized] public Party Party;
        public int TotalLength;
        public int RemainingLength;
        public string Description;
        public string Source;

        public Modifier(ModifierType type, Party p, int length, string description, string source)
        {
            Type = type;
            Party = p;
            PartyId = p.Id;
            TotalLength = length;
            RemainingLength = length;
            Description = description;
            Source = source;
        }

        public void SetDistrict(District d)
        {
            District = d;
            RegionId = d.Region.Id;
        }
    }
}
