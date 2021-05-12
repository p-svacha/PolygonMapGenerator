using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public enum GameState
    {
        Initializing,
        Ready,
        DistributionPhase,
        PlanningPhase,
        CombatPhase,
        CombatPhaseEnded,
        Ended
    }
}
