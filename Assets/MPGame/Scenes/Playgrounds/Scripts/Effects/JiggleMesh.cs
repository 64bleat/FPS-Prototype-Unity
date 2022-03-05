using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.MPWorld
{
    public class JiggleMesh : MonoBehaviour
    {
        Mesh mesh;
        Vector3[] vertices;
        Vector3[] deltas;

        private void Start()
        {
            mesh = GetComponent<MeshFilter>().mesh;

            vertices = mesh.vertices;
            deltas = new Vector3[vertices.Length];

            Invoke("timer", 1f);
        }
        void Update()
        {
            for(int p = 0; p < vertices.Length; p++)
            {
                vertices[p] += deltas[p];
            }


            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void timer()
        {
            float range = 0.001f * Time.deltaTime;

            for(int i = 0; i < deltas.Length; i++)
            {
                deltas[i] *= 0.5f;
                deltas[i].x += Random.Range(-range, range);
                deltas[i].y += Random.Range(-range, range);
                deltas[i].z += Random.Range(-range, range);
            }

            Invoke("timer", 1f);
        }
    }
}
