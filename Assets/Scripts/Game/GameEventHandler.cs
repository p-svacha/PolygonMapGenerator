using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;

public class GameEventHandler
{
    private List<GameEvent> Events = new List<GameEvent>()
    {
        new E01_NewNationEmerge(),
    };

    private GameModel Model;
    public bool IsExecuting;

    public GameEventHandler(GameModel model)
    {
        Model = model;
    }

    public void ExecuteRandomEvent()
    {
        GameEvent gameEvent = GetRandomEvent();
        IsExecuting = true;
        gameEvent.Execute(Model, this);
    }

    // Called by events
    public void ExecutionDone()
    {
        IsExecuting = false;
    }

    private GameEvent GetRandomEvent()
    {
        int totalProbability = Events.Sum(x => x.GetProbability(Model));
        int rng = Random.Range(0, totalProbability + 1);
        int tmpSum = 0;
        foreach(GameEvent gameEvent in Events)
        {
            tmpSum += gameEvent.GetProbability(Model);
            if (rng <= tmpSum) return gameEvent;
        }
        return null;
    }
}
