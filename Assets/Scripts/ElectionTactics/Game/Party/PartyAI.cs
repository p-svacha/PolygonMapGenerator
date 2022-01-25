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
                Policy policy = Party.Policies[Random.Range(0, Party.Policies.Count)];
                Party.Game.DoIncreasePolicy(Party.Id, policy.Id);
            }
        }
    }
}
