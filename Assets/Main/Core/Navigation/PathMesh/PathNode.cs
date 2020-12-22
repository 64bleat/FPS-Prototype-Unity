﻿using UnityEngine;

namespace MPCore
{
    /// <summary> Used for path generation, a point that can be pulled between two endpoints using 
    /// a rope-pulling algorithm to find the shortest path between two points over a mesh. </summary>
    public struct PathNode
    {
        public Vector3 left;
        public Vector3 path;
        public float lerpt;
        public float lerpClamp;

        public PathNode(Vector3 left, Vector3 path, float edgeOffset = 1f)
        {
            this.left = left;
            this.path = path;
            lerpClamp = Mathf.Min(edgeOffset / path.magnitude, 0.5f);
            lerpt = 0.5f;
        }

        /// <summary> Gets the closest point on the line segment, to the line between a and b. </summary>
        /// <returns> true if the line must bend to reach the closest point. </returns>
        public bool SlideAndCollide(Vector3 point0, Vector3 point1, out PathNode recalc)
        {
            Vector3 La = point0 - left;
            Vector3 Lb = point1 - left;
            Vector3 pNormal = path.normalized;
            float magOnA = Vector3.Dot(La, pNormal);
            float magOnB = Vector3.Dot(Lb, pNormal);
            float magOffA = Mathf.Sqrt(La.sqrMagnitude - Mathf.Pow(magOnA, 2));
            float magOffB = Mathf.Sqrt(Lb.sqrMagnitude - Mathf.Pow(magOnB, 2));
            float factor = Mathf.Lerp(magOnA, magOnB, magOffA / (magOffA + magOffB)) / path.magnitude;

            recalc = new PathNode(left, path);
            recalc.lerpClamp = lerpClamp;
            recalc.lerpt = Mathf.Clamp(factor, lerpClamp, 1f - lerpClamp);

            return factor < lerpClamp || factor > 1f - lerpClamp;
        }

        /// <summary> get the current position of the point. </summary>
        public static implicit operator Vector3(PathNode sp)
        {
            return sp.left + sp.path * sp.lerpt;
        }
    }
}