using System.Collections.Generic;
using UnityEngine;

namespace ARFireNavigation.Utilities
{
    public static class ArrowMeshGenerator
    {
        public static Mesh CreateArrowMesh(float shaftLength = 0.5f, float shaftWidth = 0.12f, float shaftHeight = 0.05f,
                                            float headLength = 0.35f, float headWidth = 0.32f, float headHeight = 0.07f)
        {
            Mesh mesh = new Mesh { name = "ProceduralNavigationArrowMesh" };

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            float halfSW = shaftWidth * 0.5f;
            float halfSH = shaftHeight * 0.5f;
            float halfHW = headWidth * 0.5f;
            float halfHH = headHeight * 0.5f;

            // Shaft corners (-shaftLength to 0 along Z)
            Vector3 p0 = new Vector3(-halfSW, -halfSH, -shaftLength);
            Vector3 p1 = new Vector3( halfSW, -halfSH, -shaftLength);
            Vector3 p2 = new Vector3( halfSW,  halfSH, -shaftLength);
            Vector3 p3 = new Vector3(-halfSW,  halfSH, -shaftLength);

            Vector3 p4 = new Vector3(-halfSW, -halfSH, 0f);
            Vector3 p5 = new Vector3( halfSW, -halfSH, 0f);
            Vector3 p6 = new Vector3( halfSW,  halfSH, 0f);
            Vector3 p7 = new Vector3(-halfSW,  halfSH, 0f);

            void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int start = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
                triangles.Add(start);     triangles.Add(start + 2); triangles.Add(start + 1);
                triangles.Add(start);     triangles.Add(start + 3); triangles.Add(start + 2);
            }

            // Shaft faces
            AddQuad(p0, p1, p2, p3); // Back
            AddQuad(p7, p6, p5, p4); // Front
            AddQuad(p3, p2, p6, p7); // Top
            AddQuad(p4, p5, p1, p0); // Bottom
            AddQuad(p0, p3, p7, p4); // Left
            AddQuad(p5, p6, p2, p1); // Right

            // Head (from Z = 0 to Z = headLength)
            Vector3 tipTop = new Vector3(0f, halfHH, headLength);
            Vector3 tipBot = new Vector3(0f, -halfHH, headLength);
            Vector3 baseLTop = new Vector3(-halfHW, halfHH, 0f);
            Vector3 baseLBot = new Vector3(-halfHW, -halfHH, 0f);
            Vector3 baseRTop = new Vector3( halfHW, halfHH, 0f);
            Vector3 baseRBot = new Vector3( halfHW, -halfHH, 0f);

            void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
            {
                int start = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(0.5f, 1));
                triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
            }

            // Top and Bottom triangles
            AddTriangle(baseLTop, tipTop, baseRTop);
            AddTriangle(baseRBot, tipBot, baseLBot);

            // Head Side quads
            AddQuad(baseLBot, baseLTop, tipTop, tipBot);
            AddQuad(tipBot, tipTop, baseRTop, baseRBot);
            AddQuad(baseRBot, baseRTop, baseLTop, baseLBot);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
