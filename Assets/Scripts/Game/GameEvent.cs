using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent
{

    public abstract int GetProbability(GameModel Model);
    public abstract void Execute(GameModel Model, GameEventHandler Handler);
}
