using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E01_NewNationEmerge : GameEvent
{
    private Region Capital;

    public override int GetProbability(GameModel Model)
    {
        return (int)(Model.Map.Regions.Where(x => !x.IsWater && x.Nation == null).Count() * 0.3f);
    }

    public override void InitExection(GameModel Model)
    {
        List<Region> candidates = Model.Map.Regions.Where(x => !x.IsWater && x.Nation == null).ToList();
        Capital = candidates[Random.Range(0, candidates.Count)];
        CameraTargetPosition = Capital.GetCameraPosition();

        base.InitExection(Model);
    }

    protected override void Execute(GameModel Model, GameEventHandler Hanlder)
    {
        Nation newNation = Model.CreateNation(Capital);
        Model.AddLog("A new nation " + newNation.Name + " has emerged. It has called its capital " + newNation.Capital.Name + ".");
    }

    
}