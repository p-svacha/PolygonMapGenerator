using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This is the default controls for handling camera movement and zoom on a polygon map (as used in the editor)
/// </summary>
public class BaseMapCameraHandler : MonoBehaviour
{
    protected Camera Camera;

    private static float ZOOM_SPEED = 0.3f;
    private static float DRAG_SPEED = 0.015f;
    private bool IsLeftMouseDown;
    private bool IsMouseWheelDown;
    private float MinCameraHeight;
    private float MaxCameraHeight;

    private void Start()
    {
        Camera = GetComponent<Camera>();
    }

    public void Init(Map map)
    {
        MinCameraHeight = 0.4f;
        MaxCameraHeight = Mathf.Max(map.Attributes.Width, map.Attributes.Height) * 1.2f;
    }

    public virtual void Update()
    {
        // Scroll
        if (Input.mouseScrollDelta.y != 0) 
        {
            transform.position += new Vector3(0f, -Input.mouseScrollDelta.y * ZOOM_SPEED, 0f);
            if (transform.position.y < MinCameraHeight) transform.position = new Vector3(transform.position.x, MinCameraHeight, transform.position.z);
            if (transform.position.y > MaxCameraHeight) transform.position = new Vector3(transform.position.x, MaxCameraHeight, transform.position.z);
        }

        // Dragging with middle mouse button
        if (Input.GetKeyDown(KeyCode.Mouse2)) IsMouseWheelDown = true;
        if (Input.GetKeyUp(KeyCode.Mouse2)) IsMouseWheelDown = false;
        if (IsMouseWheelDown)
        {
            float speed = transform.position.y * DRAG_SPEED;
            transform.position += new Vector3(-Input.GetAxis("Mouse X") * speed, 0f, -Input.GetAxis("Mouse Y") * speed);
        }

        // Drag triggers
        if(Input.GetKeyDown(KeyCode.Mouse0) && !IsLeftMouseDown)
        {
            IsLeftMouseDown = true;
            OnLeftMouseDragStart();
        }
        if(Input.GetKeyUp(KeyCode.Mouse0) && IsLeftMouseDown)
        {
            IsLeftMouseDown = false;
            OnLeftMouseDragEnd();
        }
    }

    public Region GetHoveredRegion()
    {
        if (!IsHoveringUi())
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                return hit.transform.gameObject.GetComponent<Region>();
            }
        }

        return null;
    }

    protected Vector2 GetMousePositionOnMap()
    {
        if (!IsHoveringUi())
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                return new Vector2(hit.point.x, hit.point.z);
            }
        }
        return Vector2.zero;
    }

    public bool IsHoveringUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    #region Triggers

    protected virtual void OnLeftMouseDragStart() { }

    protected virtual void OnLeftMouseDragEnd() { }

    #endregion
}
