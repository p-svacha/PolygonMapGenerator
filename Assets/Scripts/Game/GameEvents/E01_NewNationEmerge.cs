using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E01_NewNationEmerge : GameEvent
{
    public override int GetProbability(GameModel Model)
    {
        return Model.Map.Regions.Where(x => !x.IsWater && x.Nation == null).Count();
    }

    public override void Execute(GameModel Model, GameEventHandler Handler)
    {
        List<Region> candidates = Model.Map.Regions.Where(x => !x.IsWater && x.Nation == null).ToList();
        Region emergeRegion = candidates[Random.Range(0, candidates.Count)];
        Nation newNation = Model.CreateNation(emergeRegion);

        Model.AddLog("A new nation " + newNation.Name + " has emerged. It has called its capital " + newNation.Capital.Name + ".");

        Handler.ExecutionDone();
    }

    
}
