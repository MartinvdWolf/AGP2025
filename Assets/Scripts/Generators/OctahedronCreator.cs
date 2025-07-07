using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class OctahedronCreator
{
    private static Vector3[] directions = {
        Vector3.left, Vector3.back,
        Vector3.right, Vector3.forward
    };

    //linear interpolation between tris
    private static int CreateVertexLine(Vector3 from, Vector3 to, int steps, int v, Vector3[] vertices)
    {
        for (int i = 1; i <= steps; i++)
        {
            vertices[v++] = Vector3.Lerp(from, to, (float)i / steps);
        }
        return v;
    }

    //top half: connect to bottom half set of tris
    private static int CreateUpperStrip(int steps, int vTop, int vBottom, int t, int[] triangles)
    {
        triangles[t++] = vBottom;
        triangles[t++] = vTop - 1;
        triangles[t++] = ++vBottom;

        for (int i = 1; i <= steps; i++)
        {
            triangles[t++] = vTop - 1;
            triangles[t++] = vTop;
            triangles[t++] = vBottom;

            triangles[t++] = vBottom;
            triangles[t++] = vTop++;
            triangles[t++] = ++vBottom;
        }
        return t;
    }

    //bottom half: connect to top half set of tris
    private static int CreateLowerStrip(int steps, int vTop, int vBottom, int t, int[] triangles)
    {
        for (int i = 1; i < steps; i++)
        {
            triangles[t++] = vBottom;
            triangles[t++] = vTop - 1;
            triangles[t++] = vTop;

            triangles[t++] = vBottom++;
            triangles[t++] = vTop++;
            triangles[t++] = vBottom;
        }

        triangles[t++] = vBottom;
        triangles[t++] = vTop - 1;
        triangles[t++] = vTop;

        return t;
    }

    private static void CreateTangents(Vector3[] vertices, Vector4[] tangents)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            v.y = 0f;
            v = v.normalized;
            Vector4 tangent;
            tangent.x = -v.z;
            tangent.y = 0f;
            tangent.z = v.x;
            tangent.w = -1f;
            tangents[i] = tangent;
        }

        //fix the pole vertices
        tangents[vertices.Length - 4] = tangents[0] = new Vector3(-1f, 0, -1f).normalized;
        tangents[vertices.Length - 3] = tangents[1] = new Vector3(1f, 0f, -1f).normalized;
        tangents[vertices.Length - 2] = tangents[2] = new Vector3(1f, 0f, 1f).normalized;
        tangents[vertices.Length - 1] = tangents[3] = new Vector3(-1f, 0f, 1f).normalized;

        //change the direction of the pole tangents
        for (int i = 0; i < 4; i++)
            tangents[vertices.Length - 1 - i].w = tangents[i].w = -1f;
    }

    private static void CreateOctahedron(Vector3[] vertices, int[] triangles, int resolution)
    {
        int v = 0, vBottom = 0, t = 0;

        //create bottom pole
        for (int i = 0; i < 4; i++)
            vertices[v++] = Vector3.down;

        //create bottom half
        for (int i = 1; i <= resolution; i++)
        {
            float progress = (float)i / resolution;
            Vector3 from, to;
            vertices[v++] = to = Vector3.Lerp(Vector3.down, Vector3.forward, progress);
            for (int dir = 0; dir < 4; dir++)
            {
                from = to;
                to = Vector3.Lerp(Vector3.down, directions[dir], progress);
                t = CreateLowerStrip(i, v, vBottom, t, triangles);
                v = CreateVertexLine(from, to, i, v, vertices);
                vBottom += i > 1 ? (i - 1) : 1;
            }
            vBottom = v - 1 - i * 4;
        }

        //create top half
        for (int i = resolution - 1; i >= 1; i--)
        {
            float progress = (float)i / resolution;
            Vector3 from, to;
            vertices[v++] = to = Vector3.Lerp(Vector3.up, Vector3.forward, progress);

            for (int dir = 0; dir < 4; dir++)
            {
                from = to;
                to = Vector3.Lerp(Vector3.up, directions[dir], progress);
                t = CreateUpperStrip(i, v, vBottom, t, triangles);
                v = CreateVertexLine(from, to, i, v, vertices);
                vBottom += i + 1;
            }
            vBottom = v - 1 - i * 4;
        }

        //connect the top pole's tris to the rest
        for (int i = 0; i < 4; i++)
        {
            triangles[t++] = vBottom;
            triangles[t++] = v;
            triangles[t++] = ++vBottom;
            vertices[v++] = Vector3.up;
        }
    }

    private static void Normalize(Vector3[] vertices, Vector3[] normals)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            normals[i] = vertices[i] = vertices[i].normalized;
        }
    }

    private static void CreateUV(Vector3[] vertices, Vector2[] uv)
    {
        float prevX = 1f;
        //fix the seam by changing the current uv index's U to 1f if x is the same as the previous index.
        for (int i = 0; i < vertices.Length; ++i)
        {
            Vector3 v = vertices[i];

            if (v.x == prevX)
            {
                uv[i - 1].x = 1f;
            }
            prevX = v.x;

            Vector2 textureCoords;
            //convert coords to spherical projection
            textureCoords.x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI);

            if (textureCoords.x < 0)
                textureCoords.x += 1f;

            textureCoords.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f;
            uv[i] = textureCoords;
        }
        //patch the top and bottom vertices to fix the weird uv mapping issue on the poles, wlooks good enough with the subdivision range 0-6
        uv[vertices.Length - 4].x = uv[0].x = 0.125f;
        uv[vertices.Length - 3].x = uv[1].x = 0.375f;
        uv[vertices.Length - 2].x = uv[2].x = 0.625f;
        uv[vertices.Length - 1].x = uv[3].x = 0.875f;
    }

    //unwanted behaviour with subdivisions outside of the 0-6 range, clamped
    public static Mesh Create(int subdivisions, float radius)
    {
        if (subdivisions < 0)
        {
            Debug.Log("Subdivisions:" + subdivisions + " Set to 0");
            subdivisions = 0;
        }

        if (subdivisions > 6)
        {
            Debug.Log("Subdivisions:" + subdivisions + " Set to 6");
            subdivisions = 6;
        }

        int resolution = (int)Mathf.Pow(2, subdivisions);
        //Calculate vertices and triangles based on resulting size
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3];
        int triangleCount = (int)Mathf.Pow(2, subdivisions * 2 + 3) * 3;
        int[] triangles = new int[triangleCount];

        CreateOctahedron(vertices, triangles, resolution);

        Vector3[] normals = new Vector3[vertices.Length];
        Normalize(vertices, normals);

        Vector2[] uv = new Vector2[vertices.Length];
        CreateUV(vertices, uv);

        Vector4[] tangents = new Vector4[vertices.Length];
        CreateTangents(vertices, tangents);

        //radi scaling
        if (radius != 1f)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= radius;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Sphere";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.tangents = tangents;
        mesh.triangles = triangles;
        return mesh;
    }
}
