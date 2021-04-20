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
        renderer.material = MapDisplaySettings.Settings.DefaultMaterial;
        renderer.material.color = c;
        MeshFilter filter = border.AddComponent<MeshFilter>();
        filter.mesh = msh;

        return border;
    }

    /// <summary>
    /// Creates a GameObject with a mesh that represents the bounds of multiple polygons. onOutside means the border will be drawn on the outside of the polygons,
    /// </summary>
    public static List<GameObject> CreatePolygonGroupBorder(List<GraphPolygon> polygons, float width, Color c, bool onOutside, float height)
    {
        List<GameObject> borders = new List<GameObject>();

        // Find outer mesh vertices
        List<List<GraphNode>> outerNodes = PolygonMapFunctions.FindOutsideNodes(polygons);
        List<GraphNode> outsideBorder = outerNodes.First(x => x.Count == outerNodes.Max(y => y.Count));
        foreach(List<GraphNode> border in outerNodes)
        {
            List<Vector2> outerVertices = border.Select(x => x.Vertex).ToList();
            bool isClockwise = GeometryFunctions.IsClockwise(outerVertices);
            if(border == outsideBorder) borders.Add(CreateSinglePolygonBorder(border, width, c, layer: PolygonMapGenerator.LAYER_SHORE, onOutside ? !isClockwise : isClockwise));
            else borders.Add(CreateSinglePolygonBorder(border, width, c, layer: PolygonMapGenerator.LAYER_SHORE, onOutside ? isClockwise : !isClockwise));
        }

        return borders;
    }




}
