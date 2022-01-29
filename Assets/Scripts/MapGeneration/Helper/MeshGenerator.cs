using MapGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator
{
    public static GameObject GeneratePolygon(List<Vector2> vertices2D, PolygonMapGenerator PMG, float layer)
    {
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Count];
        Vector4[] tangents = new Vector4[vertices2D.Count];
        Vector2[] uvs = new Vector2[vertices2D.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, layer, vertices2D[i].y);
            if(PMG != null) uvs[i] = new Vector2(vertices2D[i].x / PMG.GenerationSettings.Width, vertices2D[i].y / PMG.GenerationSettings.Height);


            // tangent calc
            Vector2 beforeVertex = vertices2D[i == 0 ? vertices2D.Count - 1 : i - 1];
            Vector2 thisVertex = vertices2D[i];
            Vector2 afterVertex = vertices2D[i == vertices2D.Count - 1 ? 0 : i + 1];

            Vector2 targetPoint = GeometryFunctions.GetOffsetIntersection(beforeVertex, thisVertex, afterVertex, 1f, 1f, false);

            Vector2 tangent = targetPoint - thisVertex;
            /*
            float angle = Mathf.Abs(Vector2.Angle((beforeVertex - thisVertex).normalized, (afterVertex - thisVertex).normalized));
            float factor = (180 - angle) * 0.005f;
            tangent *= (1 + factor);
            */

            tangents[i] = new Vector4(tangent.x, 0, tangent.y, 0);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.tangents = tangents;
        msh.uv = uvs;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        GameObject polygon = new GameObject("Polygon (" + vertices2D.Count + ")");
        MeshRenderer renderer = polygon.AddComponent<MeshRenderer>();
        Color ranCol = Color.red; // new Color(Random.Range(0f, 0.3f), Random.Range(0.4f, 1f), Random.Range(0.1f, 0.6f));
        renderer.material.color = ranCol;
        MeshFilter filter = polygon.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        return polygon;
    }

    public static GameObject CreateSinglePolygonBorder(List<GraphNode> nodes, float width, Color c, float layer, bool clockwise = false)
    {
        List<Vector2> outerVertices = nodes.Select(x => x.Vertex).ToList();
        List<Vector2> innerVertices = new List<Vector2>();
        for (int i = 0; i < outerVertices.Count; i++)
        {
            Vector2 beforeVertex = outerVertices[i == 0 ? outerVertices.Count - 1 : i - 1];
            Vector2 thisVertex = outerVertices[i];
            Vector2 afterVertex = outerVertices[i == outerVertices.Count - 1 ? 0 : i + 1];

            Vector2 targetPoint = GeometryFunctions.GetOffsetIntersection(beforeVertex, thisVertex, afterVertex, width, width, clockwise);

            innerVertices.Add(targetPoint);
        }

        // Create full vertex list (outer0, inner0, outer1, inner1, outer2, inner2, ...)
        Vector3[] vertices = new Vector3[outerVertices.Count + innerVertices.Count];
        for (int i = 0; i < outerVertices.Count; i++)
        {
            vertices[2 * i] = new Vector3(outerVertices[i].x, layer, outerVertices[i].y);
            vertices[2 * i + 1] = new Vector3(innerVertices[i].x, layer, innerVertices[i].y);
        }

        // Create triangles ( 0/2/1, 1/2/3, 2/4/3, 3/4/5, 4/6/5, 5/6/7, ...) 
        int[] triangles = new int[vertices.Length * 3];
        for (int i = 0; i < vertices.Length; i++)
        {
            if (clockwise)
            {
                triangles[3 * i] = i;
                triangles[3 * i + 1] = ((((i + 1) / 2) * 2) + 1) % vertices.Length;
                triangles[3 * i + 2] = (((i / 2) * 2) + 2) % vertices.Length;
            }
            else
            {
                // One column represents one triangle
                triangles[3 * i] = i;                                                 // 0, 1, 2, 3, 4, 5, ...
                triangles[3 * i + 1] = (((i / 2) * 2) + 2) % vertices.Length;         // 2, 2, 4, 4, 6, 6, ...
                triangles[3 * i + 2] = ((((i + 1) / 2) * 2) + 1) % vertices.Length;   // 1, 3, 3, 5, 5, 7, ...
            }
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = triangles;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        GameObject border = new GameObject("Border (" + vertices.Length + ")");
        MeshRenderer renderer = border.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = MapDisplayResources.Singleton.DefaultMaterial;
        renderer.material.color = c;
        MeshFilter filter = border.AddComponent<MeshFilter>();
        filter.mesh = msh;

        return border;
    }

    /// <summary>
    /// Creates a GameObject with a mesh that represents the bounds of multiple polygons. onOutside means the border will be drawn on the outside of the polygons.
    /// All given polygons must be connected by land for this function to work! If they are not, first split it into clusters with PolygonMapFunctions.FindClusters()
    /// </summary>
    public static List<GameObject> CreatePolygonGroupBorder(List<GraphPolygon> polygons, float width, Color c, bool onOutside, float yPos)
    {
        List<GameObject> borders = new List<GameObject>();

        // Find outer mesh vertices
        List<List<GraphNode>> outerNodes = PolygonMapFunctions.FindOutsideNodes(polygons);
        List<GraphNode> outsideBorder = outerNodes.First(x => x.Count == outerNodes.Max(y => y.Count));
        foreach(List<GraphNode> border in outerNodes)
        {
            List<Vector2> outerVertices = border.Select(x => x.Vertex).ToList();
            bool isClockwise = GeometryFunctions.IsClockwise(outerVertices);
            if(border == outsideBorder) borders.Add(CreateSinglePolygonBorder(border, width, c, yPos, onOutside ? !isClockwise : isClockwise));
            else borders.Add(CreateSinglePolygonBorder(border, width, c, yPos, onOutside ? isClockwise : !isClockwise));
        }

        return borders;
    }

    /// <summary>
    /// Draws a line between to points in space
    /// </summary>
    public static GameObject DrawLine(Vector2 from, Vector2 to, float width, Color color, float yPos = 0.01f, float height = 0.01f)
    {
        GameObject lineObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Vector2 center = new Vector2((from.x + to.x) / 2, (from.y + to.y) / 2);
        float length = Vector2.Distance(from, to);
        float angle = Vector2.SignedAngle(to - from, new Vector2(1, 0));

        lineObject.transform.position = new Vector3(center.x, yPos, center.y);
        lineObject.transform.rotation = Quaternion.Euler(0, angle, 0);
        lineObject.transform.localScale = new Vector3(length, height, width);

        lineObject.GetComponent<MeshRenderer>().material.color = color;
        lineObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineObject.GetComponent<MeshRenderer>().receiveShadows = false;

        GameObject.Destroy(lineObject.GetComponent<BoxCollider>());

        return lineObject;
    }

    public static GameObject DrawArrow(Vector2 from, Vector2 to, Color color, float width = 0.1f, float arrowHeadWidth = 0.2f, float arrowHeadLength = 0.1f, float yPos = 0.01f)
    {
        GameObject arrow = new GameObject("Arrow");
       
        // Create mesh vertices and triangles
        Vector2[] vertices2D = new Vector2[7];

        Vector2 v = to - from;
        Vector2 v90 = GeometryFunctions.RotateVector(v, 90).normalized;

        vertices2D[0] = from + (v90 * width);
        vertices2D[6] = from - (v90 * width);

        float arrowHeadLengthRatio = arrowHeadLength / Vector2.Distance(from, to);
        Vector2 headStart = Vector2.Lerp(from, to, 1f - arrowHeadLengthRatio);
        vertices2D[1] = headStart + (v90 * width);
        vertices2D[5] = headStart - (v90 * width);

        vertices2D[2] = headStart + (v90 * arrowHeadWidth);
        vertices2D[4] = headStart - (v90 * arrowHeadWidth);

        vertices2D[3] = to;

        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices2D.Length; i++) vertices[i] = new Vector3(vertices2D[i].x, yPos, vertices2D[i].y);

        int[] triangles = { 0, 5, 6, 1, 5, 0, 2, 3, 4 };

        // Add mesh renderer
        MeshRenderer renderer = arrow.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = MapDisplayResources.Singleton.DefaultMaterial;
        renderer.material.color = color;

        // Create the mesh
        MeshFilter meshFilter = arrow.AddComponent<MeshFilter>();
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = triangles;
        msh.RecalculateNormals();
        msh.RecalculateBounds();
        meshFilter.mesh = msh;

        return arrow;
    }

    public static TextMesh DrawTextMesh(Vector2 position, float yPos, string text, int fontSize)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.AddComponent<MeshRenderer>();
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.transform.position = new Vector3(position.x, yPos, position.y);
        textMesh.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        textMesh.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        textMesh.color = Color.black;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.text = text;

        return textMesh;
    }

    public static GameObject DrawRectangle(Vector2 position, float yPos, float width, float height, Color color)
    {
        GameObject rectangle = new GameObject("Rectangle");

        // Create mesh vertices and triangles
        Vector3 sourcePos = new Vector3(position.x, yPos, position.y);
        Vector3[] vertices =
        {
            sourcePos + new Vector3(width / 2, 0f, height / 2),
            sourcePos + new Vector3(- (width / 2), 0f, height / 2),
            sourcePos + new Vector3(- (width / 2), 0f, - (height / 2)),
            sourcePos + new Vector3(width / 2, 0f, - (height / 2)),
        };

        int[] triangles = { 0, 2, 1, 3, 2, 0};

        // Add mesh renderer
        MeshRenderer renderer = rectangle.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = MapDisplayResources.Singleton.DefaultMaterial;
        renderer.material.color = color;

        // Create the mesh
        MeshFilter meshFilter = rectangle.AddComponent<MeshFilter>();
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = triangles;
        msh.RecalculateNormals();
        msh.RecalculateBounds();
        meshFilter.mesh = msh;

        return rectangle;
    }

}
