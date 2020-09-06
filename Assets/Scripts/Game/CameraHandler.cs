using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    // Move
    public bool IsMoving { get; private set; }
    private Vector3 MoveStartPosition;
    private Vector3 MoveTargetPosition;
    private float MoveTime;
    private float CurrentMoveTime;

    // Const
    private const float DefaultZoomTime = 1f; // s

    private Map Map;
    

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Camera Lerp
        if(IsMoving)
        {
            if(CurrentMoveTime >= MoveTime)
            {
                transform.position = MoveTargetPosition;
                IsMoving = false;
            }
            else
            {
                transform.position = Vector3.Lerp(MoveStartPosition, MoveTargetPosition, CurrentMoveTime / MoveTime);
                CurrentMoveTime += Time.deltaTime;
            }
        }
    }

    private void InitMovement(Vector3 targetPosition, float time)
    {
        if (IsMoving) return;
        IsMoving = true;
        MoveStartPosition = transform.position;
        MoveTargetPosition = targetPosition;
        MoveTime = time;
        CurrentMoveTime = 0f;
    }

    public void ReturnToDefaultView()
    {
        MoveToFocusMap(Map);
    }

    public void MoveToFocusRegion(Region r)
    {
        Vector3 targetPosition = new Vector3(r.XPos + r.Width / 2f, Math.Max(r.Width, r.Height) * 1.5f, r.YPos + r.Height / 2f);
        InitMovement(targetPosition, DefaultZoomTime);
    }

    public void MoveToFocusMap(Map m)
    {
        Vector3 targetPosition = new Vector3(m.Width / 2f, m.Height, m.Height / 2f);
        InitMovement(targetPosition, DefaultZoomTime);
    }

    public void JumpToFocusMap(Map m)
    {
        transform.position = new Vector3(m.Width / 2f, m.Height, m.Height / 2f);
    }


    public void SetMap(Map map)
    {
        Map = map;
    }
}
