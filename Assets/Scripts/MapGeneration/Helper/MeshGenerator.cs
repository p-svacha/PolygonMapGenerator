﻿using System.Collections;
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

            /*
            // tangent calc
            GraphNode beforeNode = nodes[i == 0 ? vertices2D.Count - 1 : i - 1];
            GraphNode thisNode = nodes[i];
            GraphNode afterNode = nodes[i == vertices2D.Count - 1 ? 0 : i + 1];

            Vector2 beforeVertex = beforeNode.Vertex;
            Vector2 thisVertex = thisNode.Vertex;
            Vector2 afterVertex = afterNode.Vertex;

            Vector2 toBefore = (beforeVertex - thisVertex).normalized;
            Vector2 toAfter = (afterVertex - thisVertex).normalized;

            
            Vector2 toBefore90 = GeometryFunctions.RotateVector(toBefore, 90);
            Vector2 toAfter90 = GeometryFunctions.RotateVector(toAfter, -90);

            float beforeWidth = 1f;
            float afterWidth = 1f;

            if (thisNode.Type == BorderPointType.Shore && beforeNode.Type == BorderPointType.Shore) beforeWidth *= 2;
            if (thisNode.Type == BorderPointType.Shore && afterNode.Type == BorderPointType.Shore) afterWidth *= 2;

            Vector2 beforeParallelStart = thisVertex + toBefore90 * beforeWidth;
            Vector2 beforeParallelEnd = beforeVertex + toBefore90 * beforeWidth;

            Vector2 afterParallelStart = thisVertex + toAfter90 * afterWidth;
            Vector2 afterParallelEnd = afterVertex + toAfter90 * afterWidth;

            Vector2 targetPoint = GeometryFunctions.FindIntersection(beforeParallelStart, beforeParallelEnd, afterParallelStart, afterParallelEnd);
            if(targetPoint == new Vector2(0,0)) // lines are parallel -> no intersection
            {
                Debug.Log("found parallel");
                targetPoint = thisVertex + toBefore90;
            }
            Vector2 tangent = targetPoint - thisVertex;

            Vector2 normalizedBeforeVertex = thisVertex + toBefore;
            Vector2 normalizedAfterVertex = thisVertex + toAfter;
            Vector2 tangent = (normalizedAfterVertex - normalizedBeforeVertex).normalized;
            tangent = GeometryFunctions.RotateVector(tangent, -90);
            

            float angle = Mathf.Abs(Vector2.Angle((beforeVertex - thisVertex).normalized, (afterVertex - thisVertex).normalized));
            float factor = (180 - angle) * 0.005f;
            tangent *= (1 + factor);
            

            if (nodes[i].Type == BorderPointType.Shore) tangent *= 2;

            tangents[i] = new Vector4(tangent.x, 0, tangent.y, 0);
            */
            
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

    public static GameObject CreateSinglePolygonBorder(List<GraphNode> nodes, float width, Color c, float height, bool clockwise = false)
    {
        List<Vector2> outerVertices = nodes.Select(x => x.Vertex).ToList();
        List<Vector2> innerVertices = new List<Vector2>();
        for (int i = 0; i < outerVertices.Count; i++)
        {
            Vector2 beforeVertex = outerVertices[i == 0 ? outerVertices.Count - 1 : i - 1];
            Vector2 thisVertex = outerVertices[i];
            Vector2 afterVertex = outerVertices[i == outerVertices.Count - 1 ? 0 : i + 1];

            Vector2 toBefore = (beforeVertex - thisVertex).normalized;
            Vector2 toAfter = (afterVertex - thisVertex).normalized;

            Vector2 toBefore90 = GeometryFunctions.RotateVector(toBefore, clockwise ? -90 : 90);
            Vector2 toAfter90 = GeometryFunctions.RotateVector(toAfter, clockwise ? 90 : -90);

            Vector2 beforeParallelStart = thisVertex + toBefore90 * width;
            Vector2 beforeParallelEnd = beforeVertex + toBefore90 * width;

            Vector2 afterParallelStart = thisVertex + toAfter90 * width;
            Vector2 afterParallelEnd = afterVertex + toAfter90 * width;

            bool parallel = false;
            Vector2 targetPoint = GeometryFunctions.FindIntersection(beforeParallelStart, beforeParallelEnd, afterParallelStart, afterParallelEnd, ref parallel);
            if (parallel) // lines are parallel -> no intersection
            {
                //Debug.Log("Found a parallel border in border mesh creation");
                targetPoint = thisVertex + toBefore90 * width;
            }

            innerVertices.Add(targetPoint);
        }

        // Create full vertex list (outer0, inner0, outer1, inner1, outer2, inner2, ...)
        Vector3[] vertices = new Vector3[outerVertices.Count + innerVertices.Count];
        for (int i = 0; i < outerVertices.Count; i++)
        {
            vertices[2 * i] = new Vector3(outerVertices[i].x, height, outerVertices[i].y);
            vertices[2 * i + 1] = new Vector3(innerVertices[i].x, height, innerVertices[i].y);
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
        renderer.material = MaterialHandler.Materials.DefaultMaterial;
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
            if(border == outsideBorder) borders.Add(CreateSinglePolygonBorder(border, width, c, height, onOutside ? !isClockwise : isClockwise));
            else borders.Add(CreateSinglePolygonBorder(border, width, c, height, onOutside ? isClockwise : !isClockwise));
        }

        return borders;
    }




}
