using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator
{
    public static GameObject GeneratePolygon(List<Vector2> vertices2D, PolygonMapGenerator PMG)
    {
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Count];
        Vector2[] uvs = new Vector2[vertices2D.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
            uvs[i] = new Vector2(vertices2D[i].x / PMG.Width, vertices2D[i].y / PMG.Height);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
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
