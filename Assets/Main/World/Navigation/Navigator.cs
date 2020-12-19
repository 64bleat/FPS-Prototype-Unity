using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public static class Navigator
    {
        //private static readonly List<PathMesh> pathMeshes = new List<PathMesh>();

        public static JobHandle RequestPath(Vector3 startPosition, Vector3 endPosition, List<Vector3> fillPath, float height = 0)
        {
            PathMesh originPath = null;
            float distance = float.MaxValue;

            foreach (PathMesh pm in PathMesh.activeMeshes)
                if ((pm.transform.position - startPosition).sqrMagnitude < distance)
                    originPath = pm;

            return originPath ? originPath.RequestPath(startPosition, endPosition, fillPath, height) : default;
        }

        public static float GetCoordinatesOnPath(List<Vector3> path, Vector3 position, float currentIndex, out float offDistance)
        {
            float bestDistance = float.MaxValue;
            float bestIndex = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                float clamp = PointInterp(position, path[i], path[i + 1]);
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

        public static Vector3 GetPositionOnPath(List<Vector3> path, float pathIndex, float offDist = 0)
        {
            while (offDist > 0 && pathIndex < path.Count - 1f)
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
            else if (floor >= path.Count - 1)
                return path[path.Count - 1];
            else
                return Vector3.Lerp(path[floor], path[floor + 1], pathIndex - floor);
        }

        private static float PointInterp(Vector3 position, Vector3 pointA, Vector3 pointB)
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
            if (PathMesh.activeMeshes.Count > 0)
            {
                PathMesh pathMesh = PathMesh.activeMeshes[Random.Range(0, PathMesh.activeMeshes.Count)];
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
    }
}
