using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class GeneralElectionResult
    {
        public int ElectionCycle;
        public int Year;
        public List<DistrictElectionResult> DistrictResults;

        public GeneralElectionResult(int electionCycle, int year, List<DistrictElectionResult> districtResults)
        {
            ElectionCycle = electionCycle;
            Year = year;
            DistrictResults = districtResults;
        }
    }
}
