using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// A collection of methods related to path navigation
    /// </summary>
    public static class Navigator
    {
        /// <summary>
        /// Asynchronously requests a navigation path.
        /// </summary>
        /// <param name="startPosition">world coordinates start</param>
        /// <param name="endPosition">world coordinates destination</param>
        /// <param name="fillPath">path to be cleared and re-filled on job completion</param>
        /// <param name="height">how high off the ground the path will be</param>
        /// <returns>a handle to the request job</returns>
        public static JobHandle RequestPath(Vector3 startPosition, Vector3 endPosition, List<Vector3> fillPath, float height = 0)
        {
            PathMesh originPath = null;
            float distance = float.MaxValue;

            foreach (PathMesh pm in PathMesh.activeMeshes)
                if ((pm.transform.position - startPosition).sqrMagnitude < distance)
                    originPath = pm;

            return originPath ? originPath.RequestPath(startPosition, endPosition, fillPath, height) : default;
        }

        /// <summary>
        /// Clamp a position to a path.
        /// </summary>
        /// <param name="path">world-space path</param>
        /// <param name="position">world-space position to clamp</param>
        /// <param name="pIndex">linearly-interpolated index representation of the clamped position</param>
        /// <returns>a position clamped to path</returns>
        public static Vector3 ClampToPath(List<Vector3> path, Vector3 position, out float pIndex)
        {
            float pDistance = float.MaxValue;
            Vector3 pClamp = position;
            pIndex = 0;

            for (int i = 1; i < path.Count; i++)
            {
                int h = i - 1;
                Vector3 offset = position - path[h];
                Vector3 direction = path[i] - path[h];
                float sqrMag = Mathf.Max(direction.sqrMagnitude, float.Epsilon);
                float t = Mathf.Clamp01(Vector3.Dot(offset, direction) / sqrMag);
                Vector3 clamp = Vector3.Lerp(path[h], path[i], t);
                float sqrDistance = (clamp - position).sqrMagnitude;
                
                if(sqrDistance < pDistance)
                {
                    pIndex = t + h;
                    pClamp = clamp;
                    pDistance = sqrDistance;
                }
            }

            return pClamp;
        }

        /// <summary>
        /// Interpolate a path index value over a path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pIndex"></param>
        /// <param name="distanceDelta"></param>
        /// <returns></returns>
        public static Vector3 PathLerp(IList<Vector3> path, float pIndex, float distanceDelta = 0)
        {
            int floor = Mathf.FloorToInt(pIndex);
            int ceil = Mathf.CeilToInt(pIndex);
            float t = pIndex % 1f;
            Vector3 position = Vector3.Lerp(path[floor], path[ceil], t);

            if (distanceDelta != 0)
            {
                int direction = Math.Sign(distanceDelta);
                int i = direction > 0 ? ceil : floor;
                int pCount = path.Count;

                distanceDelta = Mathf.Abs(distanceDelta);

                while (distanceDelta > 0 && i >= 0 && i < pCount)
                {
                    Vector3 newPosition = Vector3.MoveTowards(position, path[i], distanceDelta);
                    distanceDelta -= Vector3.Distance(position, newPosition);
                    position = newPosition;
                    i += direction;
                }
            }

            return position;
        }

        /// <summary>
        /// Get a random point within the navigation system
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Vector3 RandomPoint(float height = 0)
        {
            if (PathMesh.activeMeshes.Count > 0)
            {
                PathMesh pathMesh = PathMesh.activeMeshes[UnityEngine.Random.Range(0, PathMesh.activeMeshes.Count)];
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
