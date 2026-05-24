using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HelperFunctions
{
    #region Bounds

    /// <summary>
    /// Checks and return if a given coordinate is within a 2-dimensional array.
    /// </summary>
    public static bool InBounds<T>(Vector2Int coords, T[,] grid)
    {
        return coords.x >= 0 && coords.y >= 0 &&
               coords.x < grid.GetLength(0) &&
               coords.y < grid.GetLength(1);
    }

    #endregion

    #region Math

    /// <summary>
    /// Modulo that handles negative values in a logical way.
    /// </summary>
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static float SmoothLerp(float start, float end, float t)
    {
        t = Mathf.Clamp01(t);
        t = t * t * (3f - 2f * t);
        return Mathf.Lerp(start, end, t);
    }

    public static Vector3 SmoothLerp(Vector3 start, Vector3 end, float t)
    {
        t = Mathf.Clamp01(t);
        t = t * t * (3f - 2f * t);
        return Vector3.Lerp(start, end, t);
    }

    /// <summary>
    /// Rasterizes a line between two points using Bresenham's line algorithm.
    /// Returns a list of all grid cells that should be filled, considering the specified line thickness.
    /// </summary>
    public static List<Vector2Int> RasterizeLine(Vector2 start, Vector2 end, int lineThickness)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        float x0 = start.x;
        float y0 = start.y;
        float x1 = end.x;
        float y1 = end.y;

        float dx = Mathf.Abs(x1 - x0);
        float dy = Mathf.Abs(y1 - y0);
        float sx = x0 < x1 ? 1f : -1f;
        float sy = y0 < y1 ? 1f : -1f;
        float err = dx - dy;

        // Calculate half thickness
        float additionalWidthOnEachSide = ((lineThickness - 1f) / 2f);

        while (true)
        {
            // Add points around the main point to achieve the desired thickness
            for (float tx = -additionalWidthOnEachSide; tx <= additionalWidthOnEachSide; tx += 0.1f)
            {
                for (float ty = -additionalWidthOnEachSide; ty <= additionalWidthOnEachSide; ty += 0.1f)
                {
                    // Add point only if it's within the square around the thickness radius
                    if (Mathf.Abs(tx) + Mathf.Abs(ty) <= additionalWidthOnEachSide)
                    {
                        Vector2Int point = new Vector2Int(Mathf.RoundToInt(x0 + tx), Mathf.RoundToInt(y0 + ty));
                        if (!points.Contains(point))
                        {
                            points.Add(point);
                        }
                    }
                }
            }

            if (Mathf.Abs(x0 - x1) <= 1f && Mathf.Abs(y0 - y1) <= 1f)
                break;

            float e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return points;
    }

    public static void SetAsMirrored(GameObject obj)
    {
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * -1f, obj.transform.localScale.y, obj.transform.localScale.z);
    }

    /// <summary>
    /// Creates a list of points along a parabolic arc between start and end.
    /// </summary>
    /// <param name="start">Starting point of the arc.</param>
    /// <param name="end">Ending point of the arc.</param>
    /// <param name="height">Maximum height of the arc.</param>
    /// <param name="segments">Number of segments the arc is divided into.</param>
    /// <returns>A list of Vector3 points along the arc.</returns>
    public static List<Vector3> CreateArc(Vector3 start, Vector3 end, float height, int segments)
    {
        List<Vector3> arcPoints = new List<Vector3>();

        // Add the start point
        arcPoints.Add(start);

        // Calculate the arc
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments; // Normalized parameter (0 to 1)

            // Linear interpolation between start and end
            Vector3 linearPoint = Vector3.Lerp(start, end, t);

            // Add vertical parabolic height
            float parabolicHeight = 4 * height * t * (1 - t); // Parabolic equation
            linearPoint.y += parabolicHeight;

            arcPoints.Add(linearPoint);
        }

        return arcPoints;
    }

    #endregion

    #region Random

    /// <summary>
    /// Returns a random number in a gaussian distribution. About 2/3 of generated numbers are within the standard deviation of the mean.
    /// </summary>
    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }
    private static float NextGaussian()
    {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }
    public static Vector2Int GetRandomNearPosition(Vector2Int pos, float standard_deviation)
    {
        float x = NextGaussian(pos.x, standard_deviation);
        float y = NextGaussian(pos.y, standard_deviation);

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    #endregion

    #region UI

    /// <summary>
    /// Destroys all children of a GameObject immediately.
    /// </summary>
    public static void DestroyAllChildredImmediately(GameObject obj, int skipElements = 0)
    {
        int numChildren = obj.transform.childCount;
        for (int i = skipElements; i < numChildren; i++) GameObject.DestroyImmediate(obj.transform.GetChild(skipElements).gameObject);
    }
    
    public static Sprite TextureToSprite(Texture tex) => TextureToSprite((Texture2D)tex);
    public static Sprite TextureToSprite(Texture2D tex)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
    public static Sprite GetAssetPreviewSprite(string path)
    {
#if UNITY_EDITOR
        // Only executes in the Unity Editor
        UnityEngine.Object asset = Resources.Load(path);
        if (asset == null)
            throw new System.Exception($"Could not find asset with path {path}.");

        // The AssetPreview class is also editor-only
        Texture2D assetPreviewTexture = UnityEditor.AssetPreview.GetAssetPreview(asset);
        // if (assetPreviewTexture == null) 
        //    throw new System.Exception($"Could not create asset preview texture of {asset} ({path}).");

        return TextureToSprite(assetPreviewTexture);
#else
    // Always returns null in builds
    return null;
#endif
    }

    public static Sprite TextureToSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        return TextureToSprite(texture);
    }

    /// <summary>
    /// Sets the Left, Right, Top and Bottom attribute of a RectTransform
    /// </summary>
    public static void SetRectTransformMargins(RectTransform rt, float left, float right, float top, float bottom)
    {
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public static void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    /// <summary>
    /// Unfocusses any focussed button/dropdown/toggle UI element so that keyboard inputs don't get 'absorbed' by the UI element.
    /// </summary>
    public static void UnfocusNonInputUiElements()
    {
        if (EventSystem.current.currentSelectedGameObject != null && (
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null ||
            EventSystem.current.currentSelectedGameObject.GetComponent<TMP_Dropdown>() != null ||
            EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>() != null
            ))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Returns if any ui element is currently focussed.
    /// </summary>
    public static bool IsUiFocussed()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }

    /// <summary>
    /// Returns is the mouse is currently hovering over a UI element.
    /// </summary>
    public static bool IsMouseOverUi(params GameObject[] excludedButtons)
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Checks if the mouse is currently over a UI element, excluding certain UI objects
    /// and all their children.
    /// </summary>
    /// <param name="excludedUiElements">
    /// Optional list of UI GameObjects to ignore in the check (including any of their children).
    /// </param>
    /// <returns>
    /// True if mouse is over a UI element that is not excluded; false otherwise.
    /// </returns>
    public static bool IsMouseOverUiExcept(params GameObject[] excludedUiElements)
    {
        // Quick check: if pointer isn't over *any* UI elements, we can stop.
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        // Perform a UI raycast from the mouse pointer
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // If no UI elements are hit, we can stop
        if (results.Count == 0)
        {
            return false;
        }

        // Check each UI element that was hit by the raycast
        foreach (RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            // If the hit object is not in the excluded list and not a child of an excluded object,
            // then we consider the mouse to be over a "meaningful" UI element.
            if (!IsExcluded(hitObject, excludedUiElements))
            {
                return true;
            }
        }

        // If we only hit excluded objects, return false
        return false;
    }

    /// <summary>
    /// Returns true if the given object is the same as one of the excluded objects
    /// or is a child of one of them.
    /// </summary>
    private static bool IsExcluded(GameObject candidate, GameObject[] excludedUiElements)
    {
        foreach (GameObject excluded in excludedUiElements)
        {
            if (excluded == null) continue;

            // If candidate is the excluded object itself or is a descendant
            if (candidate.transform == excluded.transform ||
                candidate.transform.IsChildOf(excluded.transform))
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Color

    public static Color GetColorFromRgb255(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static Color SmoothLerpColor(Color c1, Color c2, float t)
    {
        t = Mathf.Clamp01(t); // Ensure t is in the range [0, 1]
        return Color.Lerp(c1, c2, SmoothStep(t));
    }

    // SmoothStep function for smoother interpolation
    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    public static Vector4 ColorToVec4(Color c) => new Vector4(c.r, c.g, c.b, c.a);

    #endregion

    #region Layers

    public static bool LayerMaskContainsLayer(LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    #endregion
}
