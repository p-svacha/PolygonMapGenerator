using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This is the default controls for handling camera movement and zoom on a polygon map (as used in the editor)
/// Attach this script to the main camera.
/// </summary>
public class BaseMapCameraHandler : MonoBehaviour
{
    protected Camera Camera;

    protected static float ZOOM_SPEED = 0.2f;
    protected static float DRAG_SPEED = 0.025f;
    protected static float MIN_CAMERA_HEIGHT = 0.4f;
    protected bool IsLeftMouseDown;
    protected bool IsRightMouseDown;
    protected bool IsMouseWheelDown;

    // Boundaries
    protected Vector2 CenterOffset; // If the camera center if off-center (because of UI for example), set this value to alter the center.

    protected float MinX;
    protected float MaxX;
    protected float MinY;
    protected float MaxY;
    protected float MinCameraHeight;
    protected float MaxCameraHeight;

    public virtual void Init()
    {
        Camera = Camera.main;
        MinCameraHeight = MIN_CAMERA_HEIGHT;
    }

    public void SetZoomToFullMap(Map map)
    {
        MinX = 0;
        MaxX = map.Attributes.Width;
        MinY = 0;
        MaxY = map.Attributes.Height;
        MaxCameraHeight = Mathf.Max(map.Attributes.Width, map.Attributes.Height) * 1.2f;
    }

    /// <summary>
    /// Sets the boundaries of the camera and zoom to the given regions, meaning that the camera can not be moved away from the regions.
    /// Set focusDistrict Flag to center camera above the region group.
    /// </summary>
    public void SetBoundariesToRegions(List<Region> regions, bool focusDistricts)
    {
        MinX = regions.Min(x => x.MinWorldX);
        MinY = regions.Min(x => x.MinWorldY);
        MaxX = regions.Max(x => x.MaxWorldX);
        MaxY = regions.Max(x => x.MaxWorldY);
        float width = MaxX - MinX;
        float height = MaxY - MinY;
        float altitude = height > width ? height : width;
        MaxCameraHeight = altitude * 1.2f;

        if (focusDistricts)
        {
            transform.position = GetCenterPosition(regions);
        }
    }

    public Vector3 GetCenterPosition(List<Region> regions)
    {
        float minX = regions.Min(x => x.MinWorldX);
        float minY = regions.Min(x => x.MinWorldY);
        float maxX = regions.Max(x => x.MaxWorldX);
        float maxY = regions.Max(x => x.MaxWorldY);
        float width = maxX - minX;
        float height = maxY - minY;
        float altitude = height > width ? height : width;
        altitude *= 1.2f;
        return new Vector3(MinX + (width / 2) + CenterOffset.x * altitude, altitude, MinY + (height / 2) + CenterOffset.y * altitude);
    }

    public virtual void Update()
    {
        // Scroll
        if (Input.mouseScrollDelta.y != 0) 
        {
            transform.position += new Vector3(0f, -Input.mouseScrollDelta.y * ZOOM_SPEED, 0f);

            // Zoom Boundaries
            if (transform.position.y < MinCameraHeight) transform.position = new Vector3(transform.position.x, MinCameraHeight, transform.position.z);
            if (transform.position.y > MaxCameraHeight) transform.position = new Vector3(transform.position.x, MaxCameraHeight, transform.position.z);
        }

        // Dragging with right/middle mouse button
        if (Input.GetKeyDown(KeyCode.Mouse2)) IsMouseWheelDown = true;
        if (Input.GetKeyUp(KeyCode.Mouse2)) IsMouseWheelDown = false;
        if (Input.GetKeyDown(KeyCode.Mouse1)) IsRightMouseDown = true;
        if (Input.GetKeyUp(KeyCode.Mouse1)) IsRightMouseDown = false;
        if (IsMouseWheelDown || IsRightMouseDown)
        {
            float speed = transform.position.y * DRAG_SPEED;
            transform.position += new Vector3(-Input.GetAxis("Mouse X") * speed, 0f, -Input.GetAxis("Mouse Y") * speed);

            // Position Boundaries
            if (transform.position.x < MinX + transform.position.y * CenterOffset.x) transform.position = new Vector3(MinX + transform.position.y * CenterOffset.x, transform.position.y, transform.position.z);
            if (transform.position.x > MaxX + transform.position.y * CenterOffset.x) transform.position = new Vector3(MaxX + transform.position.y * CenterOffset.x, transform.position.y, transform.position.z);
            if (transform.position.z < MinY + transform.position.y * CenterOffset.y) transform.position = new Vector3(transform.position.x, transform.position.y, MinY + transform.position.y * CenterOffset.y);
            if (transform.position.z > MaxY + transform.position.y * CenterOffset.y) transform.position = new Vector3(transform.position.x, transform.position.y, MaxY + transform.position.y * CenterOffset.y);
        }

        // Drag triggers
        if(Input.GetKeyDown(KeyCode.Mouse0) && !IsLeftMouseDown)
        {
            IsLeftMouseDown = true;
            OnLeftMouseDragStart();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && IsLeftMouseDown)
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
