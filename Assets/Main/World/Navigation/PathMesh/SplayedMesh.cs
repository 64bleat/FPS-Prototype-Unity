using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MPCore
{
    /// <summary> Extended mesh information for navigation </summary>
    public class SplayedMesh
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] normals;
        public int[] neighbors;
        public Vector3[] centers;
        public Dictionary<Vector3, HashSet<int>> dublicates;

        public SplayedMesh(Mesh mesh)
        {
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            normals = MakeTirangleNormals(triangles, vertices);
            dublicates = MakeDuplicates(triangles, vertices);
            neighbors = MakeNeighbors(triangles, vertices, dublicates);
            centers = MakeCenters(triangles, vertices);
        }

        private static Vector3[] MakeTirangleNormals(int[] triangles, Vector3[] vertices)
        {
            Vector3[] normals = new Vector3[triangles.Length / 3];

            for (int t = 0, te = triangles.Length; t < te; t += 3)
            {
                Vector3 a = vertices[triangles[t + 0]];
                Vector3 b = vertices[triangles[t + 1]];
                Vector3 c = vertices[triangles[t + 2]];

                normals[t / 3] = Vector3.Cross(b - a, c - a);
            }

            return normals;
        }

        private static Vector3[] MakeCenters(int[] triangles, Vector3[] vertices)
        {
            Vector3[] centers = new Vector3[triangles.Length / 3];

            for (int t = 0; t < triangles.Length; t += 3)
            {
                Vector3 a = vertices[triangles[t + 0]];
                Vector3 b = vertices[triangles[t + 1]];
                Vector3 c = vertices[triangles[t + 2]];

                centers[t / 3] = (a + b + c) / 3f;
            }

            return centers;
        }

        private static Dictionary<Vector3, HashSet<int>> MakeDuplicates(int[] triangles, Vector3[] vertices)
        {
            Dictionary<Vector3, HashSet<int>> duplicates = new Dictionary<Vector3, HashSet<int>>();

            for (int t = 0; t < triangles.Length; t++)
                if (duplicates.TryGetValue(vertices[triangles[t]], out HashSet<int> set))
                    set.Add(t);
                else
                    duplicates.Add(vertices[triangles[t]], new HashSet<int>() { t });

            return duplicates;
        }

        private static int[] MakeNeighbors(int[] triangles, Vector3[] vertices, Dictionary<Vector3, HashSet<int>> duplicates)
        {
            int[] neighbors = new int[triangles.Length];

            for (int i = 0; i < neighbors.Length; i++)
                neighbors[i] = -1;

            for (int t = 0; t < triangles.Length; t++)
                if (duplicates.TryGetValue(vertices[triangles[t]], out HashSet<int> vertSetA) && vertSetA.Count > 1)
                    if (duplicates.TryGetValue(vertices[triangles[t - t % 3 + (t + 1) % 3]], out HashSet<int> vertSetB) && vertSetB.Count > 1)
                        foreach (int a in vertSetA)
                            foreach (int b in vertSetB)
                                if (a / 3 != t / 3 && a / 3 == b / 3)
                                    neighbors[t] = (a + 1) % 3 == b % 3 ? a : b;

            return neighbors;
        }
    }
}
