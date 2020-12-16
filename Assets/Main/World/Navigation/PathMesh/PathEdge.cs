using UnityEngine;

namespace MPCore
{
    /// <summary> A line segment representing all possible points in which a path may slide to minimize distance. </summary>
    public class PathEdge
    {
        public Vector3 left;
        public Vector3 path;
        public float lerpt;
        private readonly float tClamp;

        public Vector3 Left { get => left; }
        public Vector3 Right { get => left + path; }

        internal PathEdge(Vector3 left, Vector3 path = default, float edgeOffset = 1f)
        {
            this.left = left;
            this.path = path;
            this.tClamp = Mathf.Min(edgeOffset / path.magnitude, 0.5f);
            lerpt = 0.5f;
        }

        /// <summary> get the current position of the point. </summary>
        public static implicit operator Vector3(PathEdge sp)
        {
            return sp.left + sp.path * sp.lerpt;
        }

        /// <summary> Gets the closest point on the line segment, to the line between a and b. </summary>
        /// <returns> true if the line must bend to reach the closest point. </returns>
        public bool SlideAndCollide(Vector3 a, Vector3 b)
        {
            Vector3 La = a - Left;
            Vector3 Lb = b - Left;
            float magOnA = Vector3.Dot(La, path.normalized);
            float magOnB = Vector3.Dot(Lb, path.normalized);
            float magOffA = Mathf.Sqrt(Mathf.Pow(La.magnitude, 2) - Mathf.Pow(magOnA, 2));
            float magOffB = Mathf.Sqrt(Mathf.Pow(Lb.magnitude, 2) - Mathf.Pow(magOnB, 2));
            float factor = Mathf.Lerp(magOnA, magOnB, magOffA / (magOffA + magOffB)) / path.magnitude;

            lerpt = Mathf.Clamp(factor, tClamp, 1f - tClamp);

            return factor < tClamp || factor > 1f - tClamp;
        }
    }
}
