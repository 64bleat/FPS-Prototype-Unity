using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class BezierCurve : MonoBehaviour
{
    public Mesh template;
    public Transform[] controlPoints = new Transform[4];
    public int iterations = 100;
    public Vector3 direction = Vector3.forward;

    // Bezier Matrix
    private static readonly List<Vector3> matPos = new List<Vector3>();
    private static readonly List<Vector3> matScl = new List<Vector3>();
    private static readonly List<Quaternion> matRot = new List<Quaternion>();

    // Mesh Generation
    private static readonly List<Vector3> templateVerts = new List<Vector3>();
    private static readonly List<Vector3> templateNorms = new List<Vector3>();
    private static readonly List<Vector2> templateUv = new List<Vector2>();
    private static readonly List<int> templateTris = new List<int>();
    private static readonly List<Vertex> appVerts = new List<Vertex>();
    private static readonly List<int> appTris = new List<int>();
    private static readonly List<Matrix4x4> pointMatrices = new List<Matrix4x4>();
    private Mesh mesh;

    private void OnValidate()
    {
        direction = direction.normalized;

        pointMatrices.Clear();

        foreach (Transform trans in controlPoints)
            pointMatrices.Add(trans.localToWorldMatrix);
    }

    private void Start()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshCollider collider = GetComponent<MeshCollider>();

        // Template Setup
        template.GetVertices(templateVerts);
        template.GetNormals(templateNorms);
        template.GetUVs(0, templateUv);
        template.GetTriangles(templateTris, 0);

        Bounds bounds = template.bounds;
        Vector3 size = bounds.size;
        float totalLength = bounds.size.z * iterations;

        // Ensure Capacity
        appVerts.Clear();
        appTris.Clear();

        if (appVerts.Capacity < templateVerts.Count * iterations)
            appVerts.Capacity = templateVerts.Count * iterations;

        if (appTris.Capacity < templateTris.Count * iterations)
            appTris.Capacity = templateTris.Count * iterations;

        // Matrices
        pointMatrices.Clear();

        foreach (Transform trans in controlPoints)
            pointMatrices.Add(trans.localToWorldMatrix);

        Vector3 copyOffset = Vector3.Project(size, direction);

        // Mesh Loft
        for (int i = 0; i < iterations; i++)
        {
            // Vertices
            for (int v = 0; v < templateVerts.Count; v++)
            {
                Vector3 vertexPosition = templateVerts[v] + copyOffset * i;
                Vector3 tangentPosition = Vector3.Project(vertexPosition, direction);
                vertexPosition -= tangentPosition;
                tangentPosition /= totalLength;
                vertexPosition += tangentPosition;
                float t = Vector3.Dot(tangentPosition, direction);
                Matrix4x4 matrix = BezierMatrix(pointMatrices, t, pointMatrices.Count, 0);

                vertexPosition = matrix.MultiplyPoint(vertexPosition);
                vertexPosition = transform.InverseTransformPoint(vertexPosition);

                appVerts.Add(new Vertex() {
                    position = vertexPosition,
                    normal = transform.InverseTransformVector(matrix.MultiplyVector(templateNorms[v])),
                    uv = templateUv[v]});
            }

            // Triangles
            for (int t = 0; t < templateTris.Count; t++)
                appTris.Add(templateTris[t] + templateVerts.Count * i);
        }

        // Apply Data to Mesh
        if (mesh)
            mesh.Clear();
        else
            mesh = new Mesh();

        mesh.SetVertexBufferParams(appVerts.Count, Vertex.meshAttributes);
        mesh.SetVertexBufferData(appVerts, 0, 0, appVerts.Count);
        mesh.SetTriangles(appTris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        // Apply Mesh to GameObject
        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }

    private void OnDestroy()
    {
        Destroy(mesh);
    }

    public static Vector3 BezierVector3(IList<Vector3> points, float t, int count = 4, int start = 0)
    {
        for (int c = count - 1; c > 0; c--)
            for (int i = 1; i <= c; i++)
                points[start + i - 1] = Vector3.Lerp(points[start + i - 1], points[start + i], t);

        return points[start];
    }
    public static Vector3 BezierSmoothVector3(IList<Vector3> points, float t, int count = 4, int start = 0)
    {
        for (int c = count - 1; c > 0; c--)
            for (int i = 1; i <= c; i++)
                points[start + i - 1] = Vector3.Slerp(points[start + i - 1], points[start + i], t);

        return points[start];
    }
    public static Quaternion BezierQuaternion(IList<Quaternion> points, float t, int count = 4, int start = 0)
    {
        for (int c = count - 1; c > 0; c--)
            for (int i = 1; i <= c; i++)
                points[start + i - 1] = Quaternion.Slerp(points[start + i - 1], points[start + i], t);

        return points[start];
    }

    public static Matrix4x4 BezierMatrix(IList<Matrix4x4> points, float t, int count = 4, int start = 0)
    {
        matPos.Clear();
        matScl.Clear();
        matRot.Clear();

        for (int i = 0; i < count; i++)
        {
            Matrix4x4 mat = points[start + i];
            matPos.Add(mat.MultiplyPoint(Vector3.zero));
            matScl.Add(mat.lossyScale);
            matRot.Add(mat.rotation);
        }

        BezierVector3(matPos, t, count, 0);
        BezierSmoothVector3(matScl, t, count, 0);
        BezierQuaternion(matRot, t, count, 0);

        return Matrix4x4.TRS(matPos[0], matRot[0], matScl[0]);
    }

    private void OnDrawGizmos()
    {
        // Break
        if (pointMatrices == null)
            return;

        // Initialize
        pointMatrices.Clear();
        foreach (Transform trans in controlPoints)
            pointMatrices.Add(trans.localToWorldMatrix);
        //if (matrices == null || matrices.Length != controlPoints.Length)
        //    matrices = new Matrix4x4[controlPoints.Length];

        //for (int i = 0; i < controlPoints.Length; i++)
        //    matrices[i] = controlPoints[i].localToWorldMatrix;

        // Points
        for (int i = 0; i < controlPoints.Length; i++)
            if (controlPoints[i])
            {
                Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / controlPoints.Length);
                Gizmos.DrawSphere(controlPoints[i].position, 0.05f);
            }

        // Preview
        for (int i = 1; i <= iterations; i++)
        {
            float tLast = (float)(i - 1) / iterations;
            float tCurrent = (float)i / iterations;

            Matrix4x4 currentMat = BezierMatrix(pointMatrices, (tLast + tCurrent) * 0.5f, pointMatrices.Count, 0);

            Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / iterations);
            Gizmos.DrawMesh(template, currentMat.MultiplyPoint(template.bounds.center), currentMat.rotation, currentMat.lossyScale);
        }

        // End
        Gizmos.color = Color.white;
    }

    /// <summary> UnityEngine mesh vertex data </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly int stride = sizeof(float) * 8;
        public static readonly VertexAttributeDescriptor[] meshAttributes = new VertexAttributeDescriptor[]{
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }
}
