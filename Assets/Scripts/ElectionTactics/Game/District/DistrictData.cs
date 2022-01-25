using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DistrictData is used to communicate districts across multiplayer.
/// </summary>
namespace ElectionTactics
{
    [System.Serializable]
    public class DistrictData
    {
        public int RegionId;
        public Random.State Seed;
        public string Name;

        public DistrictData(District d)
        {
            RegionId = d.Region.Id;
            Seed = d.Seed;
            Name = d.Name;
        }

        public DistrictData(int regionId, Random.State seed, string name)
        {
            RegionId = regionId;
            Seed = seed;
            Name = name;
        }
    }
}
