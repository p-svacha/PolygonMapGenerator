using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class PartyAI
    {
        public Party Party;

        private Dictionary<Policy, int> PolicyWeights;

        public PartyAI(Party p)
        {
            Party = p;
        }

        public void InitRandomPolicyWeights()
        {
            PolicyWeights = new Dictionary<Policy, int>();

            int minPossibleWeight = Random.Range(1, 40);
            int maxPossibleWeight = Random.Range(60, 100);

            foreach(Policy p in Party.Policies)
            {
                PolicyWeights[p] = Random.Range(minPossibleWeight, maxPossibleWeight + 1);
            }
        }

        public void DistributePolicyPoints()
        {
            while (Party.PolicyPoints > 0)
            {
                Dictionary<Policy, int> candidates = new Dictionary<Policy, int>();
                foreach(Policy p in Party.ActivePolicies)
                {
                    if (p.IsActive && p.Value < p.MaxValue) candidates.Add(p, PolicyWeights[p]);
                }

                if (candidates.Count == 0) break; // Safety: all policies maxed

                Policy chosenPolicy = candidates.GetWeightedRandomElement();
                Party.Game.DoIncreasePolicy(Party.Id, chosenPolicy.Id);
            }
        }
    }
}
