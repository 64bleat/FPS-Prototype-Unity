using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public struct GetPathJob : IJob
    {
        public Vector3 start;
        public Vector3 end;
        public float height;
        public ThreadedSplayedMesh mesh;
        public NativeArray<Vector3> nPath;
        public NativeArray<int> nLength;

        public void Execute()
        {
            Vector3 localClampedStart = ClampPointToMesh(start, out int startTriangle);
            Vector3 localClampedEnd = ClampPointToMesh(end, out int endTriangle);
            Dictionary<int, int> flow = GetTriangleFlow(endTriangle);
            ForgePath(flow, localClampedStart, startTriangle, localClampedEnd);
        }

        private Vector3 ClampPointToMesh(Vector3 worldPoint, out int triangle)
        {
            Vector3 localOrigin = mesh.world2Local.MultiplyPoint(worldPoint);
            Vector3 clampedPoint = Vector3.zero;
            float minDistance = float.PositiveInfinity;
            Vector3[] tVerts = new Vector3[3];
            float[] bary = new float[3];

            triangle = -1;

            for(int t = 0; t < mesh.triangles.Length; t += 3)
            {
                Vector3 closestPoint;
                tVerts[0] = mesh.vertices[mesh.triangles[t + 0]];
                tVerts[1] = mesh.vertices[mesh.triangles[t + 1]];
                tVerts[2] = mesh.vertices[mesh.triangles[t + 2]];
                Vector3 normArea = mesh.normals[t / 3];
                Vector3 planePoint = tVerts[0] + Vector3.ProjectOnPlane(localOrigin - tVerts[0], normArea);
                float sqrArea = Mathf.Pow(normArea.magnitude, 2);
                bary[0] = Vector3.Dot(normArea, Vector3.Cross(tVerts[2] - tVerts[1], planePoint - tVerts[1])) / sqrArea;
                bary[1] = Vector3.Dot(normArea, Vector3.Cross(tVerts[0] - tVerts[2], planePoint - tVerts[2])) / sqrArea;
                bary[2] = Vector3.Dot(normArea, Vector3.Cross(tVerts[1] - tVerts[0], planePoint - tVerts[0])) / sqrArea;
                int firstOff = bary[0] > 0 ? bary[1] > 0 ? bary[2] > 0 ? 3 : 2 : 1 : 0;
                float sqrDist;

                if (firstOff < 3)
                {
                    Vector3 origin = tVerts[(firstOff + 1) % 3];
                    Vector3 axis = tVerts[(firstOff + 2) % 3] - origin;
                    closestPoint = origin + Vector3.ClampMagnitude(axis, Mathf.Max(0, Vector3.Dot(planePoint - origin, axis.normalized)));
                }
                else
                    closestPoint = tVerts[0] * bary[0] + tVerts[1] * bary[1] + tVerts[2] * bary[2];

                sqrDist = (closestPoint - localOrigin).sqrMagnitude;

                if (sqrDist < minDistance && Vector3.Dot(localOrigin - planePoint, mesh.normals[t / 3]) > 0 )
                {
                    minDistance = sqrDist;
                    clampedPoint = closestPoint;
                    triangle = t;
                }
            }

            return clampedPoint;
        }

        /// <summary> A* flow to destination </summary>
        private Dictionary<int, int> GetTriangleFlow(int destTriangle)
        {
            var heads = new HashSet<int>();
            var flowDistance = new Dictionary<int, float>() { { destTriangle, 0 } };
            var flowNext = new Dictionary<int, int>(){ { destTriangle, -1 } };

            for (int e = 0; e < 3; e++)
            {
                int neighboringTriangle = mesh.neighbors[destTriangle + e];

                if (neighboringTriangle >= 0)
                    heads.Add(neighboringTriangle - neighboringTriangle % 3);
            }

            while (heads.Count != 0)
            {
                HashSet<int> newHeads = new HashSet<int>();

                foreach (int h in heads)
                    if (!flowNext.ContainsKey(h)) // No backtracking!
                    {
                        int bestNext = -1;
                        float bestDist = float.MaxValue;

                        for (int e = 0; e < 3; e++)
                        {
                            int neighbor = mesh.neighbors[h + e];

                            if (neighbor >= 0) // negatives are invalid
                            {
                                int tNeighbor = neighbor - neighbor % 3; //convert edge to triangle

                                // Untrodden triangles make the new generation of heads.
                                if (flowDistance.TryGetValue(tNeighbor, out float neighborDistance))
                                {
                                    float distance = neighborDistance + Vector3.Distance(mesh.centers[h / 3], mesh.centers[neighbor / 3]);

                                    if(distance < bestDist)
                                    {
                                        bestNext = neighbor;
                                        bestDist = distance;
                                    }
                                }
                                else
                                    newHeads.Add(tNeighbor);
                            }
                            // else maybe negative pointers can do special actions?
                        }

                        flowNext.Add(h, bestNext);
                        flowDistance.Add(h, bestDist);
                    }

                heads = newHeads;
            }

            return flowNext;
        }

        private void ForgePath(Dictionary<int, int> flow, Vector3 localStart, int startTri, Vector3 localEnd)
        {
            List<PathEdge> slideLine = new List<PathEdge>();
            Stack<int> bends = new Stack<int>(new int[] { 0 });
            int triIndex = startTri;

            // Rope Pulling Algorithm
            while (flow.TryGetValue(triIndex, out int neighbor) && neighbor != -1 && slideLine.Count < nPath.Length - 4)
            {
                int v1 = mesh.triangles[neighbor];
                int v2 = mesh.triangles[neighbor - neighbor % 3 + (neighbor + 1) % 3];
                Vector3 heightOffset = mesh.normals[neighbor / 3].normalized * height;
                Vector3 leftPoint = mesh.vertices[v1] + heightOffset;
                Vector3 rightPoint = mesh.vertices[v2] + heightOffset;
                PathEdge point = new PathEdge(leftPoint, rightPoint - leftPoint);

                if (point.SlideAndCollide(slideLine.Count == 0 ? localStart : slideLine[bends.Peek()], localEnd))
                {
                    for (int j = slideLine.Count - 1; j > 0; j--)
                    {
                        if (j == bends.Peek())
                            bends.Pop();

                        if (slideLine[j].SlideAndCollide(slideLine[bends.Peek()], point))
                        {
                            bends.Push(j);
                            break;
                        }
                    }

                    bends.Push(slideLine.Count);
                }

                slideLine.Add(point);

                triIndex = neighbor - neighbor % 3;
            }

            nLength[0] = 0;
            nPath[nLength[0]++] = mesh.local2World.MultiplyPoint(localStart + mesh.normals[startTri / 3].normalized * height);

            foreach (Vector3 sp in slideLine)
            {
                Vector3 worldPoint = mesh.local2World.MultiplyPoint(sp);

                if (worldPoint != nPath[nPath.Length - 1])
                    nPath[nLength[0]++] = worldPoint;
            }

            nPath[nLength[0]++] = mesh.local2World.MultiplyPoint(localEnd + mesh.normals[triIndex / 3].normalized * height);
        }
    }
}
