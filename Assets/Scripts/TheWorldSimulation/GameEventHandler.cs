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
        new E02_NationExpand(),
    };

    private WorldSimulation Model;
    public GameEvent ActiveEvent;

    public GameEventHandler(WorldSimulation model)
    {
        Model = model;
    }

    public void ExecuteRandomEvent()
    {
        GameEvent gameEvent = GetRandomEvent();
        ActiveEvent = gameEvent;
        gameEvent.InitExecution(Model);
    }

    public void Update()
    {
        if (ActiveEvent != null) ActiveEvent.Update(Model, this);
    }

    // Called by events
    public void ExecutionDone()
    {
        ActiveEvent = null;
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
