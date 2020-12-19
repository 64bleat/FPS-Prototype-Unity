using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public struct PathRequest : IJob
    {
        public Guid guid;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float height;
        public ThreadedSplayedMesh mesh;
        public UnsafeRingQueue<int> heads;
        public NativeList<Vector3> path;
        public NativeArray<int> flow;
        public NativeArray<float> flowDistance;
        public NativeList<PathNode> slideLine;

        private struct Triangle
        {
            public Vector3 v0, v1, v2;

            public Vector3 this[int index]
            {
                get
                {
                    switch (index % 3)
                    {
                        case 2:
                            return v2;
                        case 1:
                            return v1;
                        default:
                            return v0;
                    }
                }
                set
                {
                    switch (index % 3)
                    {
                        case 2:
                            v2 = value;
                            break;
                        case 1:
                            v1 = value;
                            break;
                        default:
                            v0 = value;
                            break;
                    }
                }
            }
        }

        public void Allocate()
        {
            heads = new UnsafeRingQueue<int>(32, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            path = new NativeList<Vector3>(Allocator.Persistent);
            flow = new NativeArray<int>(mesh.triangles.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            flowDistance = new NativeArray<float>(mesh.triangles.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            slideLine = new NativeList<PathNode>(Allocator.Persistent);
        }

        public void Dispose()
        {
            heads.Dispose();
            path.Dispose();
            flow.Dispose();
            flowDistance.Dispose();
            slideLine.Dispose();
        }

        public void Execute()
        {
            Vector3 localClampedStart = NearestPointOnMesh(startPosition, out int startTriangle);
            Vector3 localClampedEnd = NearestPointOnMesh(endPosition, out int endTriangle);
            ComputeTriangleFlow(endTriangle);
            ComputePath(localClampedStart, startTriangle, localClampedEnd);
        }

        private Vector3 NearestPointOnMesh(Vector3 worldPosition, out int nearestTriangle)
        {
            Vector3 localPosition = mesh.world2Local.MultiplyPoint(worldPosition);
            Vector3 meshPosition = Vector3.zero;
            float distance = float.PositiveInfinity;
            Triangle triangle = default;
            Vector3 baryPosition;

            nearestTriangle = -1;

            for(int t = 0; t < mesh.triangles.Length; t += 3)
            {
                Vector3 closestPoint;
                triangle[0] = mesh.vertices[mesh.triangles[t + 0]];
                triangle[1] = mesh.vertices[mesh.triangles[t + 1]];
                triangle[2] = mesh.vertices[mesh.triangles[t + 2]];
                Vector3 normArea = mesh.normals[t / 3];
                Vector3 planePoint = triangle[0] + Vector3.ProjectOnPlane(localPosition - triangle[0], normArea);
                float sqrArea = Mathf.Pow(normArea.magnitude, 2);
                baryPosition.x = Vector3.Dot(normArea, Vector3.Cross(triangle[2] - triangle[1], planePoint - triangle[1])) / sqrArea;
                baryPosition.y = Vector3.Dot(normArea, Vector3.Cross(triangle[0] - triangle[2], planePoint - triangle[2])) / sqrArea;
                baryPosition.z = Vector3.Dot(normArea, Vector3.Cross(triangle[1] - triangle[0], planePoint - triangle[0])) / sqrArea;
                int firstOff = baryPosition.x > 0 ? baryPosition.y > 0 ? baryPosition.z > 0 ? 3 : 2 : 1 : 0;
                float sqrDist;

                if (firstOff < 3)
                {
                    Vector3 origin = triangle[(firstOff + 1) % 3];
                    Vector3 axis = triangle[(firstOff + 2) % 3] - origin;
                    closestPoint = origin + Vector3.ClampMagnitude(axis, Mathf.Max(0, Vector3.Dot(planePoint - origin, axis.normalized)));
                }
                else
                    closestPoint = triangle[0] * baryPosition.x + triangle[1] * baryPosition.y + triangle[2] * baryPosition.z;

                sqrDist = (closestPoint - localPosition).sqrMagnitude;

                if (sqrDist < distance && Vector3.Dot(localPosition - planePoint, mesh.normals[t / 3]) > 0 )
                {
                    distance = sqrDist;
                    meshPosition = closestPoint;
                    nearestTriangle = t;
                }
            }

            return meshPosition;
        }

        /// <summary> A* flow to destination </summary>
        private void ComputeTriangleFlow(int destTriangle)
        {
            for (int i = 0; i < flow.Length; i++)
                flow[i] = -1;

            for (int i = 0; i < flow.Length; i++)
                flowDistance[i] = float.NaN;

            flowDistance[destTriangle] = 0;

            for (int e = 0; e < 3; e++)
            {
                int neighboringTriangle = mesh.neighbors[destTriangle + e];

                if (neighboringTriangle >= 0)
                    heads.Enqueue(neighboringTriangle - neighboringTriangle % 3);
            }

            while (heads.Length != 0)
            {
                for (int i = heads.Length; i > 0; i--)
                {
                    int h = heads.Dequeue();

                    if (flow[h] == -1)
                    {
                        int nextPoint = -1;
                        float nextPointDistance = float.MaxValue;

                        for (int e = 0; e < 3; e++)
                        {
                            int pointNeighbor = mesh.neighbors[h + e];

                            if (pointNeighbor != -1)
                            {
                                int triangleNeighbor = pointNeighbor - pointNeighbor % 3; //convert edge to triangle

                                if(!float.IsNaN(flowDistance[triangleNeighbor]))
                                {
                                    float distance = flowDistance[triangleNeighbor] + Vector3.Distance(mesh.centers[h / 3], mesh.centers[pointNeighbor / 3]);

                                    if (distance < nextPointDistance)
                                    {
                                        nextPoint = pointNeighbor;
                                        nextPointDistance = distance;
                                    }
                                }
                                else
                                    heads.Enqueue(triangleNeighbor);
                            }
                            // else maybe negative pointers can do special actions?
                        }

                        flow[h] = nextPoint;
                        flowDistance[h] = nextPointDistance;
                    }
                }
            }
        }

        private void ComputePath(Vector3 localStart, int strartTriangle, Vector3 localEnd)
        {
            Stack<int> bendIndeces = new Stack<int>();
            int index = strartTriangle;

            slideLine.Clear();
            path.Clear();
            slideLine.Add(new PathNode(localStart, Vector3.zero));
            bendIndeces.Push(0);

            // Follow flow until a dead end is reached
            while (flow[index] != -1)
            {
                int neighbor = flow[index];
                int v1 = mesh.triangles[neighbor];
                int v2 = mesh.triangles[neighbor - neighbor % 3 + (neighbor + 1) % 3];
                Vector3 heightOffset = mesh.normals[neighbor / 3].normalized * height;
                Vector3 leftPoint = mesh.vertices[v1] + heightOffset;
                Vector3 rightPoint = mesh.vertices[v2] + heightOffset;
                Vector3 bendPoint = slideLine[bendIndeces.Peek()];
                PathNode node = new PathNode(leftPoint, rightPoint - leftPoint);

                // Pull on the path until a bend is found
                if (node.SlideAndCollide(bendPoint, localEnd, out PathNode newNode))
                {
                    node = newNode;

                    for (int j = slideLine.Length - 1; j > 0; j--)
                    {
                        if (j == bendIndeces.Peek())
                            bendIndeces.Pop();

                        if (slideLine[j].SlideAndCollide(slideLine[bendIndeces.Peek()], node, out PathNode newJ))
                        {
                            slideLine[j] = newJ;
                            bendIndeces.Push(j);
                            break;
                        }
                    }

                    bendIndeces.Push(slideLine.Length);
                }

                slideLine.Add(node);

                index = neighbor - neighbor % 3;
            }

            path.Add(mesh.local2World.MultiplyPoint(localStart + mesh.normals[strartTriangle / 3].normalized * height));

            foreach (Vector3 sp in slideLine)
            {
                Vector3 worldPoint = mesh.local2World.MultiplyPoint(sp);

                if (worldPoint != path[path.Length - 1])
                    path.Add(worldPoint);
            }

            path.Add(mesh.local2World.MultiplyPoint(localEnd + mesh.normals[index / 3].normalized * height));
        }
    }
}
