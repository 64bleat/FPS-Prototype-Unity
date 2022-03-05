using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace MPCore
{
    public class ParticleMatchRenderMesh : MonoBehaviour
    {
        private void OnValidate()
        {
            Match();
        }

        private void Awake()
        {
            Match();
        }

        private void Match()
        {
            if (transform.TryGetComponentInParent(out MeshFilter mf)
                && transform.TryGetComponentInParent(out ParticleSystem ps))
            {
                ShapeModule sm = ps.shape;
                sm.mesh = mf.sharedMesh;
                sm.scale = mf.transform.localScale;
            }
        }
    }
}
