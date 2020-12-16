using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public class Navigator
    {
        private static readonly List<PathMesh> pathMeshes = new List<PathMesh>();

        public static float GetPathIndex(Vector3[] path, Vector3 position, float currentIndex, out float offDistance)
        {
            float bestDistance = float.MaxValue;
            float bestIndex = 0;

            for (int i = 0; i < path.Length - 1; i++)
            {
                float clamp = pointInterp(position, path[i], path[i + 1]);
                float dist = Vector3.Distance(position, Vector3.Lerp(path[i], path[i + 1], clamp));
                float index = i + clamp;

                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestIndex = index;
                }
            }

            offDistance = bestDistance;

            return bestIndex;
        }

        public static Vector3 GetPositionOnPath(Vector3[] path, float pathIndex, float offDist = 0)
        {
            //while (offDist > 0 && pathIndex != path.Length - 1)
            //{
            //    int next = Mathf.FloorToInt(pathIndex) + 1;
            //    float segmentDistance = Vector3.Distance(path[next - 1], path[next]);
            //    float nextDistance = Mathf.Min(offDist, (1f - Mathf.Repeat(pathIndex, 1f)) * segmentDistance);

            //    if (segmentDistance == 0)
            //        pathIndex += Mathf.Floor(pathIndex + 1);
            //    else
            //    {
            //        pathIndex += nextDistance / segmentDistance;
            //        offDist -= nextDistance;
            //    }
            //}

            //while(offDist < 0 && pathIndex != 0))
            //{

            //}

            while (offDist > 0 && pathIndex < path.Length - 1f)
            {
                float segLength = Vector3.Distance(path[Mathf.FloorToInt(pathIndex)], path[Mathf.FloorToInt(pathIndex) + 1]);

                if (segLength != 0)
                {
                    float available = segLength * (1f - Mathf.Repeat(pathIndex, 1.0f));
                    float taking = Mathf.Min(available, offDist);
                    pathIndex += taking / segLength;
                    offDist -= taking;
                }
                else
                    pathIndex += 1f;
            }

            int floor = (int)pathIndex;

            if (floor < 0)
                return path[0];
            else if (floor >= path.Length - 1)
                return path[path.Length - 1];
            else
                return Vector3.Lerp(path[floor], path[floor + 1], pathIndex - floor);
        }


        //public static int GetBestDestinationIndex(Vector3[] path, float skipDistance, Vector3 position, Vector3 destination, out float distance, out float destinationDistance)
        //{
        //    // Position
        //    float bestDistance = float.MaxValue;
        //    int bestIndex = 0;

        //    for(int i = 1; i < path.Length; i++)
        //    {
        //        float dist = SegmentDistance(position, path[i], path[i - 1]);

        //        if(dist < bestDistance)
        //        {
        //            bestDistance = dist;
        //            bestIndex = i;
        //        }
        //    }

        //    // Destination
        //    float bestTDistance = float.MaxValue;
        //    int bestTIndex = 0;

        //    for (int i = 0; i < path.Length; i++)
        //        if (Vector3.Distance(destination, path[i]) is var d && d < bestTDistance)
        //        {
        //            bestTIndex = i;
        //            bestTDistance = d;
        //        }

        //    bestTIndex = path.Length - 1;

        //    //Avoid too-close points
        //    if (bestIndex > bestTIndex)
        //    {
        //        bestIndex--;

        //        while (bestIndex > bestTIndex && Vector3.Distance(path[bestIndex], position) < skipDistance)
        //            bestIndex--;
        //    }
        //    else
        //        while (bestIndex < bestTIndex && Vector3.Distance (path[bestIndex], position) < skipDistance)
        //            bestIndex++;

        //    // Outs
        //    distance = bestDistance;
        //    destinationDistance = bestTDistance;

        //    return bestIndex;
        //}

        //private static float SegmentDistance(Vector3 position, Vector3 pointA, Vector3 pointB)
        //{
        //    Vector3 project = Vector3.Project(position - pointA, pointA - pointB) + pointA;
        //    return Vector3.Dot(project - pointA, project - pointB) < 0
        //                ? (position - project).magnitude
        //                : Mathf.Min(
        //                    Vector3.Distance(position, pointA),
        //                    Vector3.Distance(position, pointB));
        //}

        private static float pointInterp(Vector3 position, Vector3 pointA, Vector3 pointB)
        {
            Vector3 segment = pointB - pointA;

            if (Vector3.Dot(position - pointA, segment) < 0)
                return 0;
            else if (Vector3.Dot(position - pointB, -segment) < 0)
                return 1;
            else
                return Vector3.Project(position - pointA, segment).magnitude / segment.magnitude;
        }

        public static Vector3 RandomPoint(float height = 0)
        {
            if (pathMeshes.Count > 0)
            {
                PathMesh pathMesh = pathMeshes[UnityEngine.Random.Range(0, pathMeshes.Count)];
                int triangle = UnityEngine.Random.Range(0, pathMesh.threadMesh.triangles.Length);
                triangle -= triangle % 3;
                float u = UnityEngine.Random.value;
                float v = UnityEngine.Random.value;
                float w = UnityEngine.Random.value;
                float uvw = u + v + w;

                return pathMesh.transform.TransformPoint(
                    pathMesh.threadMesh.vertices[pathMesh.threadMesh.triangles[triangle]] * u / uvw
                    + pathMesh.threadMesh.vertices[pathMesh.threadMesh.triangles[triangle + 1]] * v / uvw
                    + pathMesh.threadMesh.vertices[pathMesh.threadMesh.triangles[triangle + 2]] * w / uvw)
                    + pathMesh.threadMesh.normals[triangle / 3].normalized * height;
            }
            else
                return Vector3.zero;
        }

        public static JobHandle RequestPath(Vector3 origin, Vector3 destination, Action<Vector3[]> onReadyAction, float height = 0)
        {
            PathMesh originPath = (from path in pathMeshes
                              orderby (path.transform.position - origin).sqrMagnitude
                              select path).FirstOrDefault();

            return originPath ? originPath.RequestPath(origin, destination, onReadyAction, height) : default;
        }

        public static void Clear()
        {
            pathMeshes.Clear();
        }

        public static void AddPathMesh(PathMesh pathMesh)
        {
            if (pathMesh)
                pathMeshes.Add(pathMesh);
        }
    }
}
