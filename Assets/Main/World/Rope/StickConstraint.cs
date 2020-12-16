using MPWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class StickConstraint : MonoBehaviour
{
    public StickNode[] nodes;
    public StickRelation[] relations;

    public void Awake()
    {
        // Nodes
        if (nodes.Length == 0 || relations.Length == 0)
        {
            List<StickNode> buildNodes = new List<StickNode>(transform.childCount);

            foreach (Transform t in transform)
                buildNodes.Add(new StickNode(t));

            nodes = buildNodes.ToArray();

            // Relations
            relations = new StickRelation[nodes.Length - 1];

            for (int i = 0; i < nodes.Length - 1; i++)
                relations[i] = new StickRelation()
                {
                    a = i,
                    b = i + 1,
                    slackPosition = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position),
                    localPosition = nodes[i + 1].transform.InverseTransformPoint(nodes[i].transform.position),
                    localUp = nodes[i + 1].transform.InverseTransformDirection(nodes[i].transform.up),
                    localForward = nodes[i + 1].transform.InverseTransformDirection(nodes[i].transform.forward)
                };
        }
    }

    public void FixedUpdate()
    {
        foreach (StickRelation stick in relations)
        {
            StickNode a = nodes[stick.a];
            StickNode b = nodes[stick.b];
            float totalMass = a.rigidbody.mass + b.rigidbody.mass;
            Vector3 correction = b.transform.position - a.transform.position;
            float rawAccel = Mathf.Max(0, correction.magnitude - stick.slackPosition) * stick.accelerationPerMeter;

            a.rigidbody.AddForce(
                correction.normalized
                * rawAccel
                * b.rigidbody.mass / totalMass,
                ForceMode.Acceleration);

            b.rigidbody.AddForce(
                -correction.normalized
                * rawAccel
                * a.rigidbody.mass / totalMass,
                ForceMode.Acceleration);
        }
        //Vector3[] nPos = new Vector3[nodes.Length];
        //Vector3[] nForward = new Vector3[nodes.Length];
        //Vector3[] nUp = new Vector3[nodes.Length];
        //Matrix4x4[] nTrans = new Matrix4x4[nodes.Length];
        //for (int i = 0; i < nPos.Length; i++)
        //{ 
        //    nPos[i] = nodes[i].rigidbody.position;
        //    nForward[i] = nodes[i].rigidbody.transform.forward;
        //    nUp[i] = nodes[i].rigidbody.transform.forward;
        //    nTrans[i] = nodes[i].rigidbody.transform.localToWorldMatrix.
        //}

        //int computeIterations = 20;
        //for (int i = 0; i < computeIterations; i++)
        //    foreach (StickRelation s in relations)
        //    {
        //        Vector3 currentOffsetPosition = nPos[s.b] + nodes[s.b].transform.TransformVector(s.localPosition) - nPos[s.a];
        //        Vector3 changeAmount = currentOffsetPosition / (nodes[s.a].rigidbody.mass + nodes[s.b].rigidbody.mass);

        //        if (!nodes[s.b].rigidbody.isKinematic)
        //            nPos[s.b] -= changeAmount * nodes[s.a].rigidbody.mass;
        //        if (!nodes[s.a].rigidbody.isKinematic)
        //            nPos[s.a] += changeAmount * nodes[s.b].rigidbody.mass;
        //    }

        //for (int i = 0; i < nPos.Length; i++)
        //{
        //    nodes[i].rigidbody.position = nPos[i];
        //}

    } 
}

[System.Serializable]
public class StickNode
{
    public Transform transform;
    public Rigidbody rigidbody;
    public Collider collider;

    public StickNode (Transform t)
    {
        transform = t;
        rigidbody = t.GetComponent<Collider>().attachedRigidbody;
        collider = t.GetComponent<Collider>();
    }
}

[System.Serializable]
public class StickRelation
{
    public int a, b;
    public Vector3 localPosition;
    public Vector3 localForward;
    public Vector3 localUp;
    public float slackPosition = 0f;
    public float slackForwardDeg = 0f;
    public float slackUpDeg = 0f;
    public float accelerationPerMeter = 10000f;
}
