using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator
{
    public static GameObject GeneratePolygon(List<GraphNode> nodes, PolygonMapGenerator PMG)
    {
        List<Vector2> vertices2D = nodes.Select(x => x.Vertex).ToList();

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Count];
        Vector4[] tangents = new Vector4[vertices2D.Count];
        Vector2[] uvs = new Vector2[vertices2D.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
            if(PMG != null) uvs[i] = new Vector2(vertices2D[i].x / PMG.Width, vertices2D[i].y / PMG.Height);

            // tangent calc


            Vector2 beforeVertex = vertices2D[i == 0 ? vertices2D.Count - 1 : i - 1];
            Vector2 thisVertex = vertices2D[i];
            Vector2 afterVertex = vertices2D[i == vertices2D.Count - 1 ? 0 : i + 1];

            Vector2 toBefore = (beforeVertex - thisVertex).normalized;
            Vector2 toAfter = (afterVertex - thisVertex).normalized;

            /*
            Vector2 toBefore90 = GeometryFunctions.RotateVector(toBefore, 90);
            Vector2 toAfter90 = GeometryFunctions.RotateVector(toAfter, 90);
            float defaultWidth = 0.1f;
            Vector2 b1 = beforeVertex + toBefore90 * defaultWidth;
            Vector2 b2 = thisVertex + toBefore90 * defaultWidth;
            Vector2 t1 = thisVertex + toAfter90 * defaultWidth;
            Vector2 t2 = afterVertex + toAfter90 * defaultWidth;

            Vector2 targetPoint = GeometryFunctions.FindIntersection(b1, b2, t1, t2);
            Vector2 tangent = targetPoint - thisVertex;
            */

            
            Vector2 normalizedBeforeVertex = thisVertex + toBefore;
            Vector2 normalizedAfterVertex = thisVertex + toAfter;
            Vector2 tangent = (normalizedAfterVertex - normalizedBeforeVertex).normalized;
            tangent = GeometryFunctions.RotateVector(tangent, -90);

            float angle = Mathf.Abs(Vector2.Angle((beforeVertex - thisVertex).normalized, (afterVertex - thisVertex).normalized));
            float factor = (180 - angle) * 0.005f;
            tangent *= (1 + factor);
            if (nodes[i].Type == BorderPointType.Shore) tangent *= 2;
            

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

    





}
