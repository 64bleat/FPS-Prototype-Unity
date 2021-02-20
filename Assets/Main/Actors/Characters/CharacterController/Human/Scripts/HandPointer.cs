using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class HandPointer : MonoBehaviour
    {
        [SerializeField] private Transform camCenter = null;
        [SerializeField]
        private float
            maxBendAngle = 5,
            anglePerSecond = 30f,
            maxTargetDistance = 500f;
        private CharacterBody body;
        private int rayMask;

        private void Awake()
        {
            body = GetComponentInParent<CharacterBody>();
            rayMask = LayerMask.GetMask("Default", "Physical");
        }

        void Update()
        {
            float viewDot = Vector3.Dot(camCenter.forward, body.transform.up);
            Vector3 upDirection = body.transform.up + camCenter.up * 0.3f + camCenter.right * -0.1f * viewDot;

            Quaternion restingRotation = Quaternion.LookRotation(camCenter.position + camCenter.forward * 1000 - transform.position, upDirection);
            Quaternion desiredRot;

            if (Physics.Raycast(camCenter.position - camCenter.forward * 0.1f, camCenter.forward, out RaycastHit hit, maxTargetDistance, rayMask))
            {
                desiredRot = Quaternion.LookRotation(hit.point - transform.position, upDirection);
                desiredRot = Quaternion.RotateTowards(restingRotation, desiredRot, maxBendAngle);
            }
            else
                desiredRot = restingRotation;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, anglePerSecond * Time.deltaTime * Mathf.Min(1f, Quaternion.Angle(transform.rotation, desiredRot) / 10f));
        }
    }
}
