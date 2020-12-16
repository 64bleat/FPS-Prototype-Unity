using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MPCore
{
    public struct ThreadedSplayedMesh
    {
        [ReadOnly] public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<int> triangles;
        [ReadOnly] public NativeArray<Vector3> normals;
        [ReadOnly] public NativeArray<int> neighbors;
        [ReadOnly] public NativeArray<Vector3> centers;
        [ReadOnly] public Matrix4x4 world2Local;
        [ReadOnly] public Matrix4x4 local2World;

        public ThreadedSplayedMesh(Mesh mesh, Transform transform)
        {
            vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
            triangles = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
            normals = MakeTirangleNormals(triangles, vertices);
            neighbors = MakeNeighbors(triangles, vertices);
            centers = MakeCenters(triangles, vertices);
            world2Local = transform.worldToLocalMatrix;
            local2World = transform.localToWorldMatrix;
        }

        private static NativeArray<Vector3> MakeTirangleNormals(NativeArray<int> triangles, NativeArray<Vector3> vertices)
        {
            var normals = new NativeArray<Vector3>(triangles.Length / 3, Allocator.Persistent);

            for (int t = 0, te = triangles.Length; t < te; t += 3)
            {
                Vector3 a = vertices[triangles[t + 0]];
                Vector3 b = vertices[triangles[t + 1]];
                Vector3 c = vertices[triangles[t + 2]];

                normals[t / 3] = Vector3.Cross(b - a, c - a);
            }

            return normals;
        }

        private static NativeArray<Vector3> MakeCenters(NativeArray<int> triangles, NativeArray<Vector3> vertices)
        {
            NativeArray<Vector3> centers = new NativeArray<Vector3>(triangles.Length / 3, Allocator.Persistent);

            for (int t = 0; t < triangles.Length; t += 3)
            {
                Vector3 a = vertices[triangles[t + 0]];
                Vector3 b = vertices[triangles[t + 1]];
                Vector3 c = vertices[triangles[t + 2]];

                centers[t / 3] = (a + b + c) / 3f;
            }

            return centers;
        }

        private static Dictionary<Vector3, HashSet<int>> MakeDuplicates(NativeArray<int> triangles, NativeArray<Vector3> vertices)
        {
            Dictionary<Vector3, HashSet<int>> duplicates = new Dictionary<Vector3, HashSet<int>>();

            for (int t = 0; t < triangles.Length; t++)
                if (duplicates.TryGetValue(vertices[triangles[t]], out HashSet<int> set))
                    set.Add(t);
                else
                    duplicates.Add(vertices[triangles[t]], new HashSet<int>() { t });

            return duplicates;
        }

        private static NativeArray<int> MakeNeighbors(NativeArray<int> triangles, NativeArray<Vector3> vertices)
        {
            Dictionary<Vector3, HashSet<int>> duplicates = MakeDuplicates(triangles, vertices);
            NativeArray<int> neighbors = new NativeArray<int>(triangles.Length, Allocator.Persistent);

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
