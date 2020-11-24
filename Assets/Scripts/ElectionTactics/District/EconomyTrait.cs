using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ElectionTactics
{
    public enum EconomyTrait
    {
        Mining,
        Fishing,
        Forestry,
        Pharmacy,
        Arts,
        Defense,
        Health,
        Aerospace,
        Electronics,
        Textiles,
        [Description("Fossil Fuels")]
        FossilFuels,
        Renewables,
        Manufacturing
    }
}
