using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent
{
    protected enum ExecutionState
    {
        ZoomIn,
        Zoomed,
        ZoomOut
    }
    protected float CameraZoomTime = 1f; // s
    protected float CameraStayTime = 1f; // s
    protected float CurrentZoomTime;
    protected Vector3 CameraTargetPosition;
    protected ExecutionState State;

    public abstract int GetProbability(GameModel Model);

    public virtual void InitExection(GameModel Model)
    {
        CurrentZoomTime = 0f;
        State = ExecutionState.ZoomIn;
    }

    public void Update(GameModel Model, GameEventHandler Handler)
    {
        switch (State)
        {
            case ExecutionState.ZoomIn:
                if (CurrentZoomTime < CameraZoomTime)
                {
                    Camera.main.transform.position = Vector3.Lerp(Model.DefaultCameraPosition, CameraTargetPosition, CurrentZoomTime / CameraZoomTime);
                    CurrentZoomTime += Time.deltaTime;
                }
                else
                {
                    Camera.main.transform.position = CameraTargetPosition;
                    Execute(Model, Handler);
                    State = ExecutionState.Zoomed;
                    CurrentZoomTime = 0;
                }
                break;

            case ExecutionState.Zoomed:
                if (CurrentZoomTime < CameraStayTime)
                {
                    CurrentZoomTime += Time.deltaTime;
                }
                else
                {
                    State = ExecutionState.ZoomOut;
                    CurrentZoomTime = 0;
                }
                break;

            case ExecutionState.ZoomOut:
                if (CurrentZoomTime < CameraZoomTime)
                {
                    Camera.main.transform.position = Vector3.Lerp(CameraTargetPosition, Model.DefaultCameraPosition, CurrentZoomTime / CameraZoomTime);
                    CurrentZoomTime += Time.deltaTime;
                }
                else
                {
                    Camera.main.transform.position = Model.DefaultCameraPosition;
                    ExecutionDone(Handler);
                }
                break;
        }
    }

    protected abstract void Execute(GameModel Model, GameEventHandler Handler);
    public void ExecutionDone(GameEventHandler Handler)
    {
        Handler.ExecutionDone();
    }
}
