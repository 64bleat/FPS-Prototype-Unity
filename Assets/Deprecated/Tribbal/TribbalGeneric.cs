﻿using MPCore;
using MPWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Junk
{
    [RequireComponent(typeof(Rigidbody))]
    public class TribbalGeneric : MonoBehaviour, IInteractable
    {
        public LineRenderer line;
        public float speedFactor = 1;
        public MeshRenderer onMesh, offMesh;

        private SphereCollider sphere;
        private Rigidbody rb;
        private readonly GOAP goap = new GOAP();
        private LineRenderer lineInstance;
        private GameObject target;
        private readonly List<Vector3> path = new List<Vector3>();
        private int pathIndex;

        private void Awake()
        {
            sphere = GetComponent<SphereCollider>();
            rb = GetComponent<Rigidbody>();

            goap.AddActions(new GOAPAction(name: "FollowingPath",  // needs target && path
                priority: PFFollowingPathPriority,
                onStart: PFFollowingPathStart,
                update: PFFollowingPathUpdate,
                onEnd: PFFolowingPathEnd));
            goap.AddActions(new GOAPAction(name: "WaitingForPath", // needs target && pathRequest
                priority: PFWaitingForPathPriority,
                update: PFWaitingForPathUpdate));
            goap.AddActions(new GOAPAction(name: "RequestingPath", //needs target
                priority: PFRequestingPathPriority,
                onStart: PFRequestingPathStart));
            goap.AddActions(new GOAPAction(name: "WaitForTarget",  //needs NOTHING
                isFinal: true,
                priority: PFWaitingForTargetPriority,
                update: PFWaitingForTargetUpdate));
        }

        private void Update()
        {
            goap.GOAPUpdate();

        }

        void FixedUpdate()
        { 
            if (onMesh.enabled)
                rb.MoveRotation( Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.Max(target ? target.transform.position - transform.position : rb.velocity, transform.forward * 0.25f), Vector3.up), Vector3.up));
        }

        #region PathFollower

        // ====================================================================
        #region Following Path
        private float? PFFollowingPathPriority(IGOAPAction next)
        {
            return next == null ? (float?)1f : null;
        }

        private void PFFollowingPathStart()
        {
            pathIndex = 0;

            if (line)
            {
                if (lineInstance)
                    DestroyImmediate(lineInstance.gameObject);

                lineInstance = Instantiate(line);
                lineInstance.positionCount = path.Count;
                lineInstance.SetPositions(path.ToArray());
            }
        }

        private GOAPStatus PFFollowingPathUpdate()
        {
            if (target && path != null && path.Count > 0)
            {
                Vector3 pClamp = Navigator.ClampToPath(path, transform.position, out float pIndex);
                float distance = Vector3.Distance(transform.position, pClamp);
                pathIndex = Mathf.CeilToInt(pIndex);

                if (distance > 5f || Vector3.Distance(transform.position, path[path.Count - 1]) < sphere.radius) //|| tDistance > 5f)
                    return GOAPStatus.Fail;

                Vector3 direction = Vector3.ClampMagnitude(path[pathIndex] - transform.position + transform.up * 0.5f, 3);

                if (float.IsNaN(direction.x))
                    direction = Vector3.zero;

                if (Time.timeScale != 0)
                        rb.AddForce(direction * 6 - Physics.gravity * 0.5f, ForceMode.Acceleration);

                return GOAPStatus.Running;
            }

            return GOAPStatus.Fail;
        }


        private void PFFolowingPathEnd()
        {
            if (lineInstance)
                Destroy(lineInstance.gameObject);
        }
        #endregion

        // ====================================================================
        #region Waiting For Path
        private float? PFWaitingForPathPriority(IGOAPAction next)
        {
            if (next?.Name?.Equals("FollowingPath") == true)
                return 1;
            else 
                return null;
        }

        private GOAPStatus PFWaitingForPathUpdate()
        {
            if (!target)
                return GOAPStatus.Fail;
            else if (path == null)
                return GOAPStatus.Running;
            else
                return GOAPStatus.Continue;
        }
        #endregion

        // ====================================================================
        #region Requesting Path
        private float? PFRequestingPathPriority(IGOAPAction next)
        {
            if (next?.Name?.Equals("WaitingForPath") == true)
                return 1;
            else 
                return null;
        }

        private void PFRequestingPathStart()
        {
            Vector3 offset = (transform.position - target.transform.position).normalized;

            Navigator.RequestPath(transform.position, Navigator.RandomPoint(sphere.radius)/*target.transform.position + offset * 3f*/, path, sphere.radius);
        }
        #endregion

        // ====================================================================
        #region Waiting For Target
        private float? PFWaitingForTargetPriority(IGOAPAction next)
        {
            if (next?.Name?.Equals("RequestingPath") == true)
                return 1;
            else
                return null;
        }

        private GOAPStatus PFWaitingForTargetUpdate()
        {
            if (!target)
                return GOAPStatus.Running;
            else
                return GOAPStatus.Continue;
        }
        #endregion
        #endregion
        #region IInteractable
        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            if (!target)
            {
                target = other;
                onMesh.enabled = true;
                offMesh.enabled = false;
            }
            else
            {
                target = null;
                onMesh.enabled = false;
                offMesh.enabled = true;
            }
        }
        public void OnInteractEnd(GameObject other, RaycastHit hit) { }
        public void OnInteractHold(GameObject other, RaycastHit hit) 
        {
            //other = other.GetComponentInChildren<CharacterCamera>().gameObject;
            //transform.position = other.transform.position + other.transform.forward * 3;
        }
        #endregion
    }
}