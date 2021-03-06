﻿using UnityEngine;
using MPCore;

namespace MPWorld
{ 
    public class FloatLever : MonoBehaviour, IInteractable, IFloatValue
    {
        public Transform lever;
        public Transform leverMin;
        public Transform leverMax;
        public float restingValue = 0f;
        public float decayDelay = 0f;
        public float rateOfDecay = 0f; 

        private float lastInteractTime = 0;
        private float value;
        private float maxMag;

        public float Value
        {
            get => value;

            set
            {
                value = Mathf.Clamp01(value);
                lever.position = Vector3.Lerp(leverMin.position, leverMax.position, value);
                this.value = value;
            }
        }

        public void Start()
        {
            maxMag = (leverMax.position - leverMin.position).magnitude;
            PositionToValue(lever.position);
        }

        public void Update()
        {
            if (rateOfDecay > 0 && Time.time - lastInteractTime > decayDelay)
                Value = Mathf.Lerp(Value, restingValue + (Value - restingValue) * 0.5f, Time.deltaTime / rateOfDecay);
        }

        public void OnInteractEnd(GameObject other, RaycastHit hit) { }
        public void OnInteractStart(GameObject other, RaycastHit hit) { }
        public void OnInteractHold(GameObject other, RaycastHit hit)
        {
            {   // Project on the lever plane
                Interactor interactor = other.GetComponentInChildren<Interactor>();
                Vector3 interactDirection = (hit.point - interactor.gameObject.transform.position).normalized;
                float a = Vector3.Dot(leverMin.position - interactor.gameObject.transform.position, leverMin.transform.up);
                float b = Vector3.Dot(interactDirection, leverMin.transform.up);

                if (b != 0 && a != 0)
                    hit.point = interactor.gameObject.transform.position + interactDirection * a / b;
            }

            PositionToValue(hit.point);

            lastInteractTime = Time.time;
        }

        private void PositionToValue(Vector3 point)
        {
            Vector3 direction = leverMax.position - leverMin.position;
            Vector3 offset = point - leverMin.position;

            Value = Vector3.Project(offset, direction).magnitude / maxMag * Mathf.Sign(Vector3.Dot(offset, direction));
        }
    }
}