using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class PartyAI
    {
        public Party Party;

        public PartyAI(Party p)
        {
            Party = p;
        }

        public void DistributePolicyPoints()
        {
            while(Party.PolicyPoints > 0)
            {
                Policy p = Party.Policies[Random.Range(0, Party.Policies.Count)];
                Party.Game.IncreasePolicy(p);
            }
        }
    }
}
