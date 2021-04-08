using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent
{
    // Zoom
    protected enum ExecutionState
    {
        ZoomIn,
        Zoomed,
        ZoomOut
    }
    protected ExecutionState State;
    protected float CameraFocusTime = 1f; // s
    protected float CameraCurrentFocusTime;

    protected CameraHandler CameraHandler;

    public abstract int GetProbability(WorldSimulation Model);

    public virtual void InitExecution(WorldSimulation Model)
    {
        CameraCurrentFocusTime = 0f;
        if(CameraHandler == null) CameraHandler = Camera.main.GetComponent<CameraHandler>();
        State = ExecutionState.ZoomIn;
    }

    public void Update(WorldSimulation Model, GameEventHandler Handler)
    {
        switch (State)
        {
            case ExecutionState.ZoomIn:
                if (!CameraHandler.IsMoving)
                {
                    Execute(Model, Handler);
                    State = ExecutionState.Zoomed;
                }
                break;

            case ExecutionState.Zoomed:
                if (CameraCurrentFocusTime < CameraFocusTime)
                {
                    CameraCurrentFocusTime += Time.deltaTime;
                }
                else
                {
                    State = ExecutionState.ZoomOut;
                    CameraHandler.ReturnToDefaultView();
                }
                break;

            case ExecutionState.ZoomOut:
                if(!CameraHandler.IsMoving) ExecutionDone(Handler);
                break;
        }
    }

    protected abstract void Execute(WorldSimulation Model, GameEventHandler Handler);
    public void ExecutionDone(GameEventHandler Handler)
    {
        Handler.ExecutionDone();
    }
}
